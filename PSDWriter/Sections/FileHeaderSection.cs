using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDWriter.Sections
{
    /// <summary>
    /// Total 26 bytes PSD file header section
    /// </summary>
    internal class FileHeaderSection
    {
        public FileHeaderSection(uint width,  uint height)
        {
            if (width < 1 || width > 30000)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Supperted range is 1 to 30000");
            if (height < 1 || height > 30000)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Supperted range is 1 to 30000");
            _width = width; 
            _height = height;
        }

        public readonly string _signature = "8BPS";
        public readonly ushort _version = 1;
        public readonly ushort _channels = 4;
        public readonly uint _height;
        public readonly uint _width;
        public readonly ushort _depth = 8;
        public readonly ushort _colorMode = 3; // RGB

        /// <summary>
        /// 26 bytes totally
        /// </summary>
        public void WriteTo(Stream stream)
        {
            // 4 bytes (Signature)
            stream.Write(Encoding.ASCII.GetBytes(_signature));

            // 2 bytes (Version)
            stream.WriteU16BE(_version);

            // 6 bytes (Reserved)
            stream.WriteZeros(6);

            // 2 bytes (Number of channels)
            stream.WriteU16BE(_channels);

            // 4 bytes (Height in pixels)
            stream.WriteU32BE(_height);

            // 4 bytes (Width in pixels)
            stream.WriteU32BE(_width);

            // 2 bytes (Number of bits per channel)
            stream.WriteU16BE(_depth);

            // 2 bytes (Color mode)
            stream.WriteU16BE(_colorMode);
        }
    }
}
