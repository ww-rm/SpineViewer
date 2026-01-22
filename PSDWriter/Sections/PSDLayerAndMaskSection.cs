using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDWriter.Sections
{
    internal class PSDLayerAndMaskSection
    {
        public List<PSDLayer> Layers { get; } = [];

        public void WriteTo(Stream stream)
        {
            if (Layers.Count >= 0x7fff)
                throw new ArgumentOutOfRangeException(nameof(Layers), "Too many layers");

            // 4 bytes (Section length)
            stream.WriteI32BE(4 + 2 + Layers.Select(it => it.RecordLength + it.ChannelDataLength).Sum() + 4);

            // 4 bytes (Length of layer info)
            stream.WriteI32BE(2 + Layers.Select(it => it.RecordLength + it.ChannelDataLength).Sum());

            // 2 bytes (Layer count)
            stream.WriteI16BE((short)Layers.Count);

            // 2n bytes (Layer records)
            foreach (var layer in Layers)
                layer.WriteRecordTo(stream);

            // 2n bytes (Layer channel data)
            foreach (var layer in Layers)
                layer.WriteChannelDataTo(stream);

            // 4 bytes (Global layer mask info)
            stream.WriteU32BE(0);

            // 0 bytes (Tagged blocks)
        }
    }

    internal class PSDLayer
    {
        public PSDLayer(string name, uint width, uint height)
        {
            Name = name;
            Right = (int)width;
            Bottom = (int)height;
        }

        public string Name { get; }
        public int Top { get; } = 0;
        public int Left { get; } = 0;
        public int Right { get; }
        public int Bottom { get; }

        public ushort Channels { get; } = 4;
        public ReadOnlySpan<byte> ChannelDataR { get => _channelDataR; }
        public ReadOnlySpan<byte> ChannelDataG { get => _channelDataG; }
        public ReadOnlySpan<byte> ChannelDataB { get => _channelDataB; }
        public ReadOnlySpan<byte> ChannelDataA { get => _channelDataA; }

        public string BlendModeSignature { get; } = "8BIM";
        public string BlendMode { get; } = "norm";

        public byte Opacity { get; } = 255;
        public byte Clipping { get; } = 0;
        public byte Flags { get; } = 8;

        private byte[] _channelDataR = [];
        private byte[] _channelDataG = [];
        private byte[] _channelDataB = [];
        private byte[] _channelDataA = [];

        /// <summary>
        /// 4n bytes (first length byte + valid name bytes + zeros padding)
        /// </summary>
        private byte[] GetNameBytes()
        {
            var nameBytes = Encoding.UTF8.GetBytes(Name);
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
        private byte[] GetNameUnicodeBytes()
        {
            var nameBytes = Encoding.BigEndianUnicode.GetBytes(Name);
            var nameBytesLength = Math.Min(nameBytes.Length, 255 * 2); // Maximum to 255 chars
            var nameLength = nameBytesLength + 3 >> 2 << 2; // Pad to a multiple of 4 bytes.

            // 4n bytes (4 bytes for length of char + valid name bytes + zeros padding)
            var bytes = new byte[4 + nameLength];
            BinaryPrimitives.WriteInt32BigEndian(bytes, nameBytesLength / 2);
            Buffer.BlockCopy(nameBytes, 0, bytes, 4, nameBytesLength);
            return bytes;
        }

        public void SetRgbaImageData(byte[] imageData, bool preMultipliedAlpha = false)
        {
            ArgumentNullException.ThrowIfNull(imageData, nameof(imageData));
            if (imageData.Length != Right * Bottom * Channels)
                throw new ArgumentException($"Incorrect data length {imageData.Length}", nameof(imageData));

            List<byte[]> rowsR = new(Bottom);
            List<byte[]> rowsG = new(Bottom);
            List<byte[]> rowsB = new(Bottom);
            List<byte[]> rowsA = new(Bottom);

            for (int y = 0; y < Bottom; y++)
            {
                byte[] chR = new byte[Right];
                byte[] chG = new byte[Right];
                byte[] chB = new byte[Right];
                byte[] chA = new byte[Right];

                var offset = y * Right;
                for (int x = 0; x < Right; x++)
                {
                    var i = (offset + x) * Channels;
                    var r = imageData[i];
                    var g = imageData[i + 1];
                    var b = imageData[i + 2];
                    var a = imageData[i + 3];

                    if (preMultipliedAlpha && a > 0 && a < 255)
                    {
                        float alpha = a / 255f;
                        r = (byte)Math.Min(r / alpha, 255);
                        g = (byte)Math.Min(g / alpha, 255);
                        b = (byte)Math.Min(b / alpha, 255);
                    }

                    chR[x] = r;
                    chG[x] = g;
                    chB[x] = b;
                    chA[x] = a;
                }

                rowsR.Add(PackBits.Encode(chR));
                rowsG.Add(PackBits.Encode(chG));
                rowsB.Add(PackBits.Encode(chB));
                rowsA.Add(PackBits.Encode(chA));
            }

            _channelDataR = new byte[2 + 2 * rowsR.Count + rowsR.Select(r => r.Length).Sum()];
            _channelDataG = new byte[2 + 2 * rowsG.Count + rowsG.Select(r => r.Length).Sum()];
            _channelDataB = new byte[2 + 2 * rowsB.Count + rowsB.Select(r => r.Length).Sum()];
            _channelDataA = new byte[2 + 2 * rowsA.Count + rowsA.Select(r => r.Length).Sum()];

            static void _SetChannelData(byte[] channelData, List<byte[]> rows)
            {
                channelData[0] = 0;
                channelData[1] = 1;
                int i1 = 2;
                int i2 = i1 + 2 * rows.Count;
                foreach (var row in rows)
                {
                    ushort count = (ushort)row.Length;
                    channelData[i1] = (byte)(count >> 8);
                    channelData[i1 + 1] = (byte)count;
                    Buffer.BlockCopy(row, 0, channelData, i2, row.Length);
                    i1 += 2;
                    i2 += row.Length;
                }
            }

            _SetChannelData(_channelDataR, rowsR);
            _SetChannelData(_channelDataG, rowsG);
            _SetChannelData(_channelDataB, rowsB);
            _SetChannelData(_channelDataA, rowsA);
        }

        public int RecordLength
        {
            get
            {
                var nameBytes = GetNameBytes();
                var nameUnicodeBytes = GetNameUnicodeBytes();
                return 16 + 2 + 6 * Channels + 4 + 4 + 4 + 4 + 4 + 44 + nameBytes.Length + 12 + nameUnicodeBytes.Length;
            }
        }

        public int ChannelDataLength
        {
            get
            {
                return _channelDataA.Length + _channelDataR.Length + _channelDataG.Length + _channelDataB.Length;
            }
        }

        public void WriteRecordTo(Stream stream)
        {
            var nameBytes = GetNameBytes();
            var nameUnicodeBytes = GetNameUnicodeBytes();

            // 16 bytes (Rectangle of top, legt, bottom, right)
            stream.WriteI32BE(Top);
            stream.WriteI32BE(Left);
            stream.WriteI32BE(Bottom);
            stream.WriteI32BE(Right);

            // 2 bytes (Number of channels)
            stream.WriteU16BE(Channels);

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
            stream.Write(Encoding.ASCII.GetBytes(BlendModeSignature));

            // 4 bytes (Blend mode key)
            stream.Write(Encoding.ASCII.GetBytes(BlendMode));

            // 1 byte (Opacity)
            stream.WriteByte(Opacity);

            // 1 byte (Clipping)
            stream.WriteByte(Clipping);

            // 1 byte (Flags)
            stream.WriteByte(Flags);

            // 1 byte (Zero filler)
            stream.WriteByte(0);

            // 4 bytes (Length of extra data length, the rest data below)
            stream.WriteI32BE(4 + 44 + nameBytes.Length + 12 + nameUnicodeBytes.Length);

            // 4 bytes (Layer mask data)
            stream.WriteU32BE(0);

            // 44 bytes (Blending ranges)
            stream.WriteU32BE(40);
            stream.WriteU32Repeats(2 + 2 * Channels, 0x0000FFFF);

            // 4n bytes (Layer name padded with zero bytes)
            stream.Write(nameBytes);

            // 12 + 4n bytes (Unicode layer name)
            stream.Write(Encoding.ASCII.GetBytes("8BIMluni"));
            stream.WriteI32BE(nameUnicodeBytes.Length);
            stream.Write(nameUnicodeBytes);
        }

        public void WriteChannelDataTo(Stream stream)
        {
            stream.Write(_channelDataA);
            stream.Write(_channelDataR);
            stream.Write(_channelDataG);
            stream.Write(_channelDataB);
        }
    }
}
