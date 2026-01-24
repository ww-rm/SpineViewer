using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdWriter.Sections.Layers
{
    internal class RgbaLayer : Layer
    {
        public RgbaLayer(string name, uint width, uint height) : base(name, width, height) { }

        public void SetRgbaImageData(byte[] imageData, bool preMultipliedAlpha)
        {
            ArgumentNullException.ThrowIfNull(imageData, nameof(imageData));
            if (imageData.Length != _right * _bottom * _channels)
                throw new ArgumentException($"Incorrect data length {imageData.Length}", nameof(imageData));

            List<byte[]> rowsR = new(_bottom);
            List<byte[]> rowsG = new(_bottom);
            List<byte[]> rowsB = new(_bottom);
            List<byte[]> rowsA = new(_bottom);

            for (int y = 0; y < _bottom; y++)
            {
                byte[] chR = new byte[_right];
                byte[] chG = new byte[_right];
                byte[] chB = new byte[_right];
                byte[] chA = new byte[_right];

                var offset = y * _right;
                for (int x = 0; x < _right; x++)
                {
                    var i = (offset + x) * _channels;
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
    }
}
