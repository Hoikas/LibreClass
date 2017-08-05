using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;
using NLog;

namespace LibreClass.Response {
    internal class EIGen3Hub : ResponseDevice {
        HidDevice m_device;
        HidStream m_stream;
        Logger m_log = LogManager.GetCurrentClassLogger();

        public string Firmware { get; private set; }
        public string Name { get { return m_device.ProductName; } }

        #region Device Init/Destroy
        internal override bool IBeginSoftRemoval() {
            return true;
        }

        internal override void IDeviceRemoved() {
            m_active = false;
            m_stream.Close();
            base.IDeviceRemoved();
        }

        internal override async Task<bool> Initialize(HidDevice hid) {
            if (m_device != null)
                throw new InvalidOperationException();
            m_device = hid;
            return await IQueryDevice();
        }
        #endregion

        #region Class Begin/End
        public override async Task<bool> StartClass(IRoster roster) {
            m_config.AcceptingAnswers = true;
            // begin HAX
            if (roster == null)
                m_config.NumClickers = 32;
            else
                m_config.NumClickers = (from student in roster.Students select student.SrsDeviceID).Max();
            // end HAX
            m_config.PingClickers = true;
            m_config.PrepareKeypad();
            if (!await ISendMsgAndAck(m_config.Serialize())) {
                m_config.Reset();
                m_log.Error("Failed to start class");
                return false;
            }
            return true;
        }

        public override async Task<bool> EndClass() {
            throw new NotImplementedException();
        }
        #endregion

        #region Device IO
        SemaphoreSlim m_ackSema = new SemaphoreSlim(0);
        Task m_readTask;
        EIGen3Config m_config = new EIGen3Config();
        bool m_active = true;

        enum IncomingMsgIDs {
            INVALID = -1,
            TIMEOUT = -2,

            Acknowledgement = 0x90,
            Join = 0x81,
        }

        enum JoinType {
            INVALID,
            Request,
            Confirmation,
            Ping,
        }

        static int MsgBufSize {
            get {
                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        return 65;
                    default:
                        return 64;
                }
            }
        }

        private async Task<bool> IQueryDevice() {
            // Prepare the stream
            m_stream = m_device.Open();
            m_readTask = Task.Run(async () => { while (m_active) await IReadMsg(); });

            // Announce ourselves
            byte[] msg = new byte[64];
            msg[0] = 0x11;
            if (!await ISendMsgAndAck(msg)) {
                m_log.Error("Device did not ACK ping packet");
                return false;
            }

            m_log.Info("Connected `{0}` (v{1}) [POWER:{3}] [CHANNEL:{2}]",
                       Name, Firmware, m_config.BaseChannel, m_config.HubPowerLevel);

            // Prepare a baseline config
            m_config.PrepareKeypad();
            if (!await ISendMsgAndAck(m_config.Serialize())) {
                m_log.Warn("Baseline config failed--prepare for oddities???");
            }
            return true;
        }

        private bool IProcessAck(byte[] msg) {
            m_log.Trace("CPS Pulse Hub packet acknowledged...");
            Firmware = String.Format("{0}.{1}", msg[3], msg[2]);
            m_config.BaseChannel = msg[4];
            m_config.HubPowerLevel = msg[7];

            // Acknowledgement schtuff
            if ((msg[11] & 0x01) != 0) {
                m_log.Error("ACK EEPROM CRC Fail");
                return false; // TODO: better handle errors?
            }
            if ((msg[11] & 0x02) != 0) {
                m_log.Error("ACK Message Buffer Full");
                return false; // TODO: better handle errors
            }
            if ((msg[11] & 0x04) != 0) {
                m_log.Error("ACK CRC Fail");
                return false; // TODO: better handle errors
            }

            m_ackSema.Release();
            return true;
        }

        private void IProcessJoins(byte[] msg) {
            // observation: only one join request is posted at once
            // will log any exceptions to the ASSumption
            int numJoins = msg[1] & 0x0F;
            if (numJoins != 1) {
                m_log.Fatal("NOT SUPPORTED: MULTIPLE JOINS '{0}'", BitConverter.ToString(msg).Replace('-', ' '));
                throw new NotSupportedException();
            }

            // begin crazy ass-message
            int offset = 0;
            for (int i = 0; i < numJoins; i++) {
                // decode everything the message will give us
                JoinType joinType = (JoinType)((msg[offset+9] & 0xE0) >> 5);
                byte[] serial = new byte[4];
                serial[0] = (byte)((((msg[offset + 5] & 0x30) >> 4) | (msg[offset + 9] & 0xC)) << 2);
                Buffer.BlockCopy(msg, offset+2, serial, 1, 3);
                ushort clickerId = ByteUtil.FromBytesLE(msg, offset+6);
                int clickerType = (msg[offset+9] & 0xC) >> 2;
                byte[] studentId = new byte[20];
                Buffer.BlockCopy(msg, offset+11, studentId, 0, 20);
                int batteryLevel = (msg[offset+5] >> 6 & 0x3);

                // todo: acknowledge join with hub
            }
        }

        private async Task<IncomingMsgIDs> IReadMsg() {
            byte[] buf = new byte[MsgBufSize];
            m_stream.ReadTimeout = Timeout.Infinite;
            await m_stream.ReadAsync(buf, 0, MsgBufSize);

            // Lop off that pesky leading zero...
            byte[] msg = new byte[64];
            Buffer.BlockCopy(buf, MsgBufSize == 65 ? 1 : 0, msg, 0, 64);
            m_log.Trace("<RCV> '{0}'", BitConverter.ToString(msg).Replace('-', ' '));

            // Check the CRC of the incoming message for kicks
            ushort softCRC = EIGen3CRC16.CRCMsg(msg, false);
            ushort rcvrCRC = ByteUtil.FromBytesLE(msg, 62);
            if (softCRC != rcvrCRC) {
                m_log.Fatal("Incoming message CRC fail [MINE:{0:X}] [THEIRS:{1:X}]", softCRC, rcvrCRC);
                return IncomingMsgIDs.INVALID;
            }

            // Incoming message handlers...
            IncomingMsgIDs id = (IncomingMsgIDs)msg[0];
            switch (id) {
                case IncomingMsgIDs.Acknowledgement:
                    IProcessAck(msg); // FIXME: do something with return value???
                    break;

                case IncomingMsgIDs.Join:
                    IProcessJoins(msg);
                    break;
            }

            return id;
        }

        private async Task<bool> ISendMsgAndAck(byte[] msg, bool crc=true) {
            for (int i = 0; i < 3; i++) {
                await ISendMsg(msg, crc);
                // currently, an error returned by the hub's ack message is the same as 
                // not receiving an ack at all. the error is logged, at least...
                bool success = await m_ackSema.WaitAsync(20);
                if (!success) {
                    m_log.Warn("Timeout waiting for success ACK from hub. Resending msg {0:X} in 50ms", msg[0]);
                    await Task.Delay(50);
                } else {
                    return true;
                }
            };

            m_log.Error("Failed to receive success ACK from hub for msg {0:X}", msg[0]);
            return false;
        }

        private async Task ISendMsg(byte[] msg, bool crc=true) {
            if (crc)
                EIGen3CRC16.CRCMsg(msg);
            m_log.Trace("<SND> '{0}'", BitConverter.ToString(msg).Replace('-', ' '));
            byte[] buf = new byte[MsgBufSize];
            if (MsgBufSize == 65 && msg.Length != 65)
                Buffer.BlockCopy(msg, 0, buf, 1, 64);
            else
                msg.CopyTo(buf, 0);
            await m_stream.WriteAsync(buf, 0, MsgBufSize);
        }
        #endregion
    }
}
