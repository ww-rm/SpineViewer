using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDWriter.Sections
{
    internal class ImageDataSection
    {
        public ImageDataSection(uint width, uint height)
        {
            Width = width; 
            Height = height;
        }

        public uint Width { get; }
        public uint Height { get; }

        public void WriteTo(Stream stream)
        {
            stream.WriteU16BE(1);
            var packedRow = PackBits.Encode(new byte[Width]);
            stream.WriteU16Repeats((int)Height * 4, (ushort)packedRow.Length);
            for (int i = 0; i < Height * 4; i++)
            {
                stream.Write(packedRow);
            }
        }
    }
}
