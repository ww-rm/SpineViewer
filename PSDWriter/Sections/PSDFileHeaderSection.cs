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
    internal class PSDFileHeaderSection
    {
        public PSDFileHeaderSection(uint width,  uint height)
        {
            if (width < 1 || width > 30000)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Supperted range is 1 to 30000");
            if (height < 1 || height > 30000)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Supperted range is 1 to 30000");
            Width = width; 
            Height = height;
        }

        public string Signature { get; } = "8BPS";
        public ushort Version { get; } = 1;
        public ushort Channels { get; } = 4;
        public uint Height { get; }
        public uint Width { get; }
        public ushort Depth { get; } = 8;
        public ushort ColorMode { get; } = 3; // RGB

        /// <summary>
        /// 26 bytes totally
        /// </summary>
        public void WriteTo(Stream stream)
        {
            // 4 bytes (Signature)
            stream.Write(Encoding.ASCII.GetBytes(Signature));

            // 2 bytes (Version)
            stream.WriteU16BE(Version);

            // 6 bytes (Reserved)
            stream.WriteZeros(6);

            // 2 bytes (Number of channels)
            stream.WriteU16BE(Channels);

            // 4 bytes (Height in pixels)
            stream.WriteU32BE(Height);

            // 4 bytes (Width in pixels)
            stream.WriteU32BE(Width);

            // 2 bytes (Number of bits per channel)
            stream.WriteU16BE(Depth);

            // 2 bytes (Color mode)
            stream.WriteU16BE(ColorMode);
        }
    }
}
