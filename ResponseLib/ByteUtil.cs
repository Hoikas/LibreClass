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
