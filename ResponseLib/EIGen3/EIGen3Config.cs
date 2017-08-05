using System;
using System.Collections.Generic;
using System.Linq;

namespace LibreClass.Response {
    internal class EIGen3Config {
        #region Properties
        /// <summary>
        /// How many student clickers are being polled
        /// </summary>
        public ushort NumClickers { get; set; }

        public ushort NumQuestions { get; set; }

        public byte HubPowerLevel { get; set; } = 4;

        public EIGen3Keypad KeypadConfig { get; } = new EIGen3Keypad();

        public bool MaskAnswer { get; set; }

        public bool ShowCorrectAnswer { get; set; }

        public bool RequireCorrectAnswer { get; set; }

        /// <summary>
        /// Informs the clickers something is going down
        /// </summary>
        /// <remarks>If AcceptingAnswers is false, the clicker displays "Please Wait"</remarks>
        /// <seealso cref="AcceptingAnswers" />
        public bool PingClickers { get; set; }

        public bool HasDataPayload { get; set; }

        public bool PowerOffClickers { get; set; }

        public bool LockClickerKeypad { get; set; }

        /// <summary>
        /// Allows students to input and send answers on their SRS devices
        /// </summary>
        /// <remarks>
        /// Clickers will not "Join" unless this is set
        /// </remarks>
        public bool AcceptingAnswers { get; set; }

        public IQuestion Question { get; set; }

        public AssessmentType Mode { get; set; } = AssessmentType.None;

        public byte BaseChannel { get; set; } = 26;
        #endregion

        /// <summary>
        /// Generates a keypad mask for use matching the current configuration
        /// </summary>
        public void PrepareKeypad() {
            if (LockClickerKeypad) {
                KeypadConfig.LockAll();
            } else if (Mode == AssessmentType.None) {
                KeypadConfig.LockAll();
                return;
            } else {
                KeypadConfig.UnlockAll();
            }

            if (Question != null) {
                switch (Question.QuestionType) {
                    case QuestionType.MultipleChoice:
                        KeypadConfig.LockKey(EIGen3Key.EntryMode);
                        KeypadConfig.LockKey(EIGen3Key.PlusMinus);
                        KeypadConfig.LockKey(EIGen3Key.Symbols);
                        KeypadConfig.LockChoiceKeys(Question.NumChoices);
                        break;
                    case QuestionType.TrueFalse:
                        KeypadConfig.LockKey(EIGen3Key.EntryMode);
                        KeypadConfig.LockKey(EIGen3Key.PlusMinus);
                        KeypadConfig.LockKey(EIGen3Key.Symbols);
                        KeypadConfig.LockChoiceKeys(2);
                        break;
                }
            }

            if (Mode == AssessmentType.TMA) {
                KeypadConfig.LockKey(EIGen3Key.LeftArrow);
                KeypadConfig.LockKey(EIGen3Key.RightArrow);
                KeypadConfig.LockKey(EIGen3Key.Search);
            }
        }

        public void Reset() {
            // todo
        }

        /// <summary>
        /// Serializes the student response system's state to a byte array for communication with
        /// the response hub device.
        /// </summary>
        /// <returns>SRS state</returns>
        internal byte[] Serialize() {
            byte[] msg = new byte[64];
            msg[0] = 0x10;

            if (NumClickers >= 1000)
                throw new NotSupportedException("the number of clickers IS TOO DAMN HIGH!");
            ByteUtil.ToBytesBE(NumClickers, msg, 1);
            ByteUtil.ToBytesBE(NumQuestions, msg, 3);

            // getNumberOfTimeSlotsForAsynchronousJoinRequests()
            if (NumClickers <= 200)
                msg[5] = 32;
            else if (NumClickers <= 500)
                msg[5] = 64;
            else
                msg[5] = 128;

            // Hub Power Level
            msg[6] = (byte)(HubPowerLevel << 4);

            // Keypad mask
            Buffer.BlockCopy(KeypadConfig.Mask, 0, msg, 7, 3);

            // Answer flags
            if (MaskAnswer)
                msg[10] |= 0x20;
            if (ShowCorrectAnswer)
                msg[10] |= 0x90;
            if (RequireCorrectAnswer)
                msg[10] |= 0x40;

            // Data flags (not all flags imp'd)
            if (PingClickers)
                msg[11] |= 0x04;
            if (HasDataPayload)
                msg[11] |= 0x40;

            // Clicker flags
            if (!PowerOffClickers)
                msg[12] |= 0x01;
            if (LockClickerKeypad)
                msg[12] |= 0x02;
            if (AcceptingAnswers)
                msg[12] |= 0x04;

            // Test flags
            if (Question != null) {
                switch (Question.QuestionType) {
                    case QuestionType.Numeric:
                        msg[13] |= 0x01;
                        break;
                    case QuestionType.MultipleChoice:
                        msg[13] |= 0x02;
                        break;
                    case QuestionType.Essay:
                        msg[13] |= (0x01 | 0x02);
                        break;
                }
            }
            if (Mode != AssessmentType.None) {
                // NOTE: 0x80 is for Tlsma, but this seems to be unused in eInstruction land
                // as such, if an assessment is active, we simply use the Sma flag
                msg[13] |= 0x40;
            }

            // TODO: SMA

            // Garbage?
            ByteUtil.Fill(0xFF, msg, 42, 10);
            msg[51] &= 0x07;

            // getMessagingPriority()
            msg[55] = 1;

            // Join message perf
            if (Mode == AssessmentType.TMA)
                msg[56] = 1;
            else if (NumClickers <= 100)
                msg[56] = 32;
            else if (NumClickers <= 300)
                msg[56] = 64;
            else if (NumClickers <= 600)
                msg[56] = 128;
            else
                msg[56] = 180;

            msg[57] = BaseChannel;
            return msg;
        }
    }
}
