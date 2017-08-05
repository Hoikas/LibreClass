/*    This file is part of LibreClass.
 *
 *    LibreClass is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    LibreClass is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with LibreClass.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace LibreClass.Response {
    internal class EIGen3Key {
        public byte[] Mask { get; private set; }
        string m_key;

        #region Static Keys
        public static EIGen3Key RightArrow { get; } = new EIGen3Key(0x01, ">");
        public static EIGen3Key Menu { get; } = new EIGen3Key(0x02, "(Menu)");
        public static EIGen3Key LeftArrow { get; } = new EIGen3Key(0x04, "<");
        public static EIGen3Key Send { get; } = new EIGen3Key(0x08, "(Send)");
        public static EIGen3Key Clear { get; } = new EIGen3Key(0x10, "(Clear)");
        public static EIGen3Key Num0 { get; } = new EIGen3Key(0, 0x01, "0");
        public static EIGen3Key Num1 { get; } = new EIGen3Key(0, 0x02, "1");
        public static EIGen3Key Num2 { get; } = new EIGen3Key(0, 0x04, "2");
        public static EIGen3Key Num3 { get; } = new EIGen3Key(0, 0x08, "3");
        public static EIGen3Key Num4 { get; } = new EIGen3Key(0, 0x10, "4");
        public static EIGen3Key Num5 { get; } = new EIGen3Key(0, 0x20, "5");
        public static EIGen3Key Num6 { get; } = new EIGen3Key(0, 0x40, "6");
        public static EIGen3Key Num7 { get; } = new EIGen3Key(0, 0x80, "7");
        public static EIGen3Key Num8 { get; } = new EIGen3Key(0, 0, 0x01, "8");
        public static EIGen3Key Num9 { get; } = new EIGen3Key(0, 0, 0x02, "9");
        public static EIGen3Key PlusMinus { get; } = new EIGen3Key(0, 0, 0x04, "+/-");
        public static EIGen3Key Symbols { get; } = new EIGen3Key(0, 0, 0x08, "(Symbols)");
        public static EIGen3Key EntryMode { get; } = new EIGen3Key(0, 0, 0x10, "(Entry Mode)");
        public static EIGen3Key Search { get; } = new EIGen3Key(0, 0, 0x20, "(Search)");
        public static EIGen3Key Power { get; } = new EIGen3Key(0, 0, 0x40, "(Power)");

        public static EIGen3Key[] AnswerChoices { get; } = new EIGen3Key[] { Num1, Num2, Num3,
                                                                             Num4, Num5, Num6,
                                                                             Num7, Num8, Num9,
                                                                             Num0 };
        #endregion

        private EIGen3Key(byte b1, string key) {
            Mask = new byte[3] { b1, 0, 0 };
            m_key = key;
        }

        private EIGen3Key(byte b1, byte b2, string key) {
            Mask = new byte[3] { b1, b2, 0 };
            m_key = key;
        }

        private EIGen3Key(byte b1, byte b2, byte b3, string key) {
            Mask = new byte[3] { b1, b2, b3 };
            m_key = key;
        }

        public override bool Equals(object obj) {
            if (!(obj is EIGen3Key))
                return false;

            EIGen3Key rhs = (EIGen3Key)obj;
            return Mask.Equals(rhs.Mask);
        }

        public override int GetHashCode() {
            return (Mask[0] & 0xFF) << 16 |
                   (Mask[1] & 0xFF) << 8 |
                   (Mask[2] & 0xFF) << 0;
        }

        public override string ToString() {
            return m_key;
        }
    }

    internal class EIGen3Keypad {
        public byte[] Mask { get; } = new byte[3];
        Logger m_log = LogManager.GetCurrentClassLogger();

        public void LockAll() {
            m_log.Trace("Locking all clicker keys");
            Mask[0] = 0xFF;
            Mask[1] = 0xFF;
            Mask[2] = 0xBF; // Might not be a good idea to lock the POWER key :) :) :)
        }

        public void LockKey(EIGen3Key key) {
            m_log.Trace("Locking clicker key '{0}'", key.ToString());
            for (int i = 0; i < Mask.Length; i++)
                Mask[i] |= key.Mask[i];
        }

        public void LockKeys(IEnumerable<EIGen3Key> keys) {
            foreach (var key in keys)
                LockKey(key);
        }

        /// <summary>
        /// Locks unused choice keys
        /// </summary>
        public void LockChoiceKeys(int numChoices) {
            if (numChoices == -1)
                return;
            for (int i = numChoices; i < EIGen3Key.AnswerChoices.Length; i++) {
                EIGen3Key key = EIGen3Key.AnswerChoices[i];
                m_log.Trace("Locking clicker key '{0}'", key.ToString());
                LockKey(key);
            }
        }

        public void UnlockAll() {
            m_log.Trace("Unlocking all clicker keys");
            for (int i = 0; i < Mask.Length; i++)
                Mask[i] = 0;
        }

        public void UnlockKey(EIGen3Key key) {
            m_log.Trace("Unlocking clicker key '{0}'", key.ToString());
            for (int i = 0; i < Mask.Length; i++)
                Mask[i] ^= key.Mask[i];
        }

        public void UnlockKeys(IEnumerable<EIGen3Key> keys) {
            foreach (var key in keys)
                UnlockKey(key);
        }
    }
}
