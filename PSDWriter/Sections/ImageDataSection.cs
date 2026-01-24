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
            _width = width; 
            _height = height;
        }

        public uint _width;
        public uint _height;

        public void WriteTo(Stream stream)
        {
            stream.WriteU16BE(1);
            var packedRow = PackBits.Encode(new byte[_width]);
            stream.WriteU16Repeats((int)_height * 4, (ushort)packedRow.Length);
            for (int i = 0; i < _height * 4; i++)
            {
                stream.Write(packedRow);
            }
        }
    }
}
