using System;
using System.Collections.Generic;
using System.Linq;

namespace LibreClass.Response {
    internal class EIGen3CRC16 {
        private uint m_seed = 0;
        private uint m_value;

        public ushort Value {
            get { return (ushort)(m_value & 0xFFFF); }
        }

        public EIGen3CRC16() { }
        public EIGen3CRC16(uint seed) {
            m_seed = seed;
            m_value = seed;
        }

        public static ushort Generate(byte[] data) {
            EIGen3CRC16 crc = new EIGen3CRC16();
            crc.Update(data, 0, data.Length);
            return crc.Value;
        }

        public static ushort CRCMsg(byte[] msg, bool update = true) {
            EIGen3CRC16 crc = new EIGen3CRC16();
            crc.Update(msg, 0, 62);
            if (update)
                ByteUtil.ToBytesLE(crc.Value, msg, 62);
            return crc.Value;
        }

        public void Reset() {
            m_value = m_seed;
        }

        public void Update(byte[] data, int offset, int count) {
            for (int i = offset; i < count; i++) {
                uint n = 0;
                uint n2 = (m_value & 0xFF) ^ data[i];
                for (int j = 7; j >= 0; j--) {
                    if (((n2 ^ n) & 0x1) > 0)
                        n = (n >> 1 ^ 0xA001);
                    else
                        n >>= 1;
                    n2 >>= 1;
                }
                m_value = ((m_value >> 8 ^ n) & 0xFFFF);
            }
        }
    }
}
