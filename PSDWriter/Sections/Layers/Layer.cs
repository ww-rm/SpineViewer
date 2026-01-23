using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PSDWriter.Sections.Layers
{
    internal abstract class Layer
    {
        protected readonly string _name;

        protected readonly int _top = 0;
        protected readonly int _left = 0;
        protected readonly int _right;
        protected readonly int _bottom;

        protected readonly ushort _channels = 4;

        protected readonly string _blendModeSignature = "8BIM";
        protected readonly string _blendMode = "norm";

        protected readonly byte _opacity = 255;
        protected readonly byte _clipping = 0;
        protected readonly byte _flags = 8;

        protected readonly List<byte[]> _additionalInfo = [];

        protected byte[] _channelDataR = [];
        protected byte[] _channelDataG = [];
        protected byte[] _channelDataB = [];
        protected byte[] _channelDataA = [];

        public Layer(string name, uint width, uint height)
        {
            _name = name;
            _right = (int)width;
            _bottom = (int)height;

            using (var ms = new MemoryStream())
            {
                var nameUnicodeBytes = GetNameUnicodeBytes();

                // 12 + 4n bytes (Unicode layer name)
                ms.Write(Encoding.ASCII.GetBytes("8BIMluni"));
                ms.WriteI32BE(nameUnicodeBytes.Length);
                ms.Write(nameUnicodeBytes);
                _additionalInfo.Add(ms.ToArray());
            }
        }

        /// <summary>
        /// 4n bytes (first length byte + valid name bytes + zeros padding)
        /// </summary>
        protected byte[] GetNameBytes()
        {
            var nameBytes = Encoding.UTF8.GetBytes(_name);
            var nameBytesLength = Math.Min(nameBytes.Length, 31); // Maximum to 31 bytes
            var nameLength = 1 + nameBytesLength + 3 >> 2 << 2; // Pad to a multiple of 4 bytes including first length byte.

            // 4n bytes (first length byte + valid name bytes + zeros padding)
            var bytes = new byte[nameLength];
            bytes[0] = (byte)nameBytesLength;
            Buffer.BlockCopy(nameBytes, 0, bytes, 1, nameBytesLength);
            return bytes;
        }

        /// <summary>
        /// 4n bytes (4 bytes for length of char + valid name bytes + zeros padding)
        /// </summary>
        protected byte[] GetNameUnicodeBytes()
        {
            var nameBytes = Encoding.BigEndianUnicode.GetBytes(_name);
            var nameBytesLength = Math.Min(nameBytes.Length, 255 * 2); // Maximum to 255 chars
            var nameLength = nameBytesLength + 3 >> 2 << 2; // Pad to a multiple of 4 bytes.

            // 4n bytes (4 bytes for length of char + valid name bytes + zeros padding)
            var bytes = new byte[4 + nameLength];
            BinaryPrimitives.WriteInt32BigEndian(bytes, nameBytesLength / 2);
            Buffer.BlockCopy(nameBytes, 0, bytes, 4, nameBytesLength);
            return bytes;
        }

        public virtual int RecordLength { get => 18 + 6 * _channels + 64 + GetNameBytes().Length + _additionalInfo.Select(x => x.Length).Sum(); }

        public virtual int ChannelDataLength { get => _channelDataA.Length + _channelDataR.Length + _channelDataG.Length + _channelDataB.Length; }

        public virtual void WriteRecordTo(Stream stream)
        {
            var nameBytes = GetNameBytes();
            var nameUnicodeBytes = GetNameUnicodeBytes();

            // 16 bytes (Rectangle of top, legt, bottom, right)
            stream.WriteI32BE(_top);
            stream.WriteI32BE(_left);
            stream.WriteI32BE(_bottom);
            stream.WriteI32BE(_right);

            // 2 bytes (Number of channels)
            stream.WriteU16BE(_channels);

            // 6 * Channels bytes (Channel information)
            stream.WriteI16BE(-1);
            stream.WriteI32BE(_channelDataA.Length);
            stream.WriteI16BE(0);
            stream.WriteI32BE(_channelDataR.Length);
            stream.WriteI16BE(1);
            stream.WriteI32BE(_channelDataG.Length);
            stream.WriteI16BE(2);
            stream.WriteI32BE(_channelDataB.Length);

            // 4 bytes (Blend mode signature)
            stream.Write(Encoding.ASCII.GetBytes(_blendModeSignature));

            // 4 bytes (Blend mode key)
            stream.Write(Encoding.ASCII.GetBytes(_blendMode));

            // 1 byte (Opacity)
            stream.WriteByte(_opacity);

            // 1 byte (Clipping)
            stream.WriteByte(_clipping);

            // 1 byte (Flags)
            stream.WriteByte(_flags);

            // 1 byte (Zero filler)
            stream.WriteByte(0);

            // 4 bytes (Length of extra data length, the rest data below)
            stream.WriteI32BE(48 + nameBytes.Length + _additionalInfo.Select(x => x.Length).Sum());

            // 4 bytes (Layer mask data)
            stream.WriteU32BE(0);

            // 44 bytes (Blending ranges)
            stream.WriteU32BE(40);
            stream.WriteU32Repeats(2 + 2 * _channels, 0x0000FFFF);

            // 4n bytes (Layer name padded with zero bytes)
            stream.Write(nameBytes);

            // 4n bytes (Additinal informations)
            foreach (var info in _additionalInfo)
                stream.Write(info);
        }

        public virtual void WriteChannelDataTo(Stream stream)
        {
            stream.Write(_channelDataA);
            stream.Write(_channelDataR);
            stream.Write(_channelDataG);
            stream.Write(_channelDataB);
        }
    }
}
