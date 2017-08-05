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
