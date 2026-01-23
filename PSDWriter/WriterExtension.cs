using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDWriter
{
    internal static class WriterExtension
    {
        public static void WriteU32BE(this Stream self, uint u32)
        {
            uint x = u32;
            self.WriteByte((byte)(x >> 24));
            self.WriteByte((byte)(x >> 16));
            self.WriteByte((byte)(x >> 8));
            self.WriteByte((byte)(x >> 0));
        }

        public static void WriteI32BE(this Stream self, int i32)
        {
            uint x = (uint)i32;
            self.WriteByte((byte)(x >> 24));
            self.WriteByte((byte)(x >> 16));
            self.WriteByte((byte)(x >> 8));
            self.WriteByte((byte)(x >> 0));
        }

        public static void WriteU16BE(this Stream self, ushort u16)
        {
            ushort x = u16;
            self.WriteByte((byte)(x >> 8));
            self.WriteByte((byte)(x >> 0));
        }

        public static void WriteI16BE(this Stream self, short i16)
        {
            ushort x = (ushort)i16;
            self.WriteByte((byte)(x >> 8));
            self.WriteByte((byte)(x >> 0));
        }

        public static void WriteRepeats(this Stream self, int count, byte value)
        {
            for (int i = 0; i < count; i++)
                self.WriteByte(value);
        }

        public static void WriteU16Repeats(this Stream self, int count, ushort value)
        {
            for (int i = 0; i < count; i++)
                self.WriteU16BE(value);
        }

        public static void WriteI16Repeats(this Stream self, int count, short value)
        {
            for (int i = 0; i < count; i++)
                self.WriteI16BE(value);
        }

        public static void WriteU32Repeats(this Stream self, int count, uint value)
        {
            for (int i = 0; i < count; i++)
                self.WriteU32BE(value);
        }

        public static void WriteI32Repeats(this Stream self, int count, int value)
        {
            for (int i = 0; i < count; i++)
                self.WriteI32BE(value);
        }

        public static void WriteZeros(this Stream self, int count)
        {
            self.WriteRepeats(count, 0);
        }
    }
}
