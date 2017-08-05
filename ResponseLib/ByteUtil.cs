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

namespace LibreClass.Response {
    internal static class ByteUtil {
        public static void Fill(byte value, byte[] desination, int offset, int count) {
            for (int i = 0; i < count; i++)
                desination[offset + i] = value;
        }

        public static ushort FromBytesLE(byte[] source, int offset) {
            byte[] temp = new byte[2];
            Buffer.BlockCopy(source, offset, temp, 0, 2);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(temp);
            return BitConverter.ToUInt16(temp, 0);
        }

        public static void ToBytesBE(ushort source, byte[] destination, int offset) {
            byte[] buf = BitConverter.GetBytes(source);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            Buffer.BlockCopy(buf, 0, destination, offset, 2);
        }

        public static void ToBytesLE(ushort source, byte[] destination, int offset) {
            byte[] buf = BitConverter.GetBytes(source);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            Buffer.BlockCopy(buf, 0, destination, offset, 2);
        }
    }
}
