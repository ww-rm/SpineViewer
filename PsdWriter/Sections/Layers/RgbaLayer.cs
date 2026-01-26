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
            if (_channels != 4)
                throw new InvalidOperationException("Channels must be 4");

            ArgumentNullException.ThrowIfNull(imageData, nameof(imageData));
            if (imageData.Length != _width * _height * _channels)
                throw new ArgumentException($"Incorrect data length {imageData.Length}", nameof(imageData));

            _top = (int)_height;
            _left = (int)_width;
            _right = 0;
            _bottom = 0;

            for (int y = 0; y < _height; y++)
            {
                var offset = y * _width;
                for (int x = 0; x < _width; x++)
                {
                    var pi = (offset + x) * _channels;
                    var a = imageData[pi + 3];
                    if (a > 0)
                    {
                        _left = Math.Min(_left, x);
                        _top = Math.Min(_top, y);
                        _right = Math.Max(_right, x + 1);
                        _bottom = Math.Max(_bottom, y + 1);
                    }
                }
            }

            var rowCount = _bottom - _top;
            var colCount = _right - _left;

            if (rowCount <= 0 || colCount <= 0)
            {
                ClearPixels();
                return;
            }

            List<byte[]> rowsR = new(rowCount);
            List<byte[]> rowsG = new(rowCount);
            List<byte[]> rowsB = new(rowCount);
            List<byte[]> rowsA = new(rowCount);

            for (int y = _top; y < _bottom; y++)
            {
                byte[] chR = new byte[colCount];
                byte[] chG = new byte[colCount];
                byte[] chB = new byte[colCount];
                byte[] chA = new byte[colCount];

                var offset = y * _width;
                for (int x = _left, i = 0; x < _right; x++, i++)
                {
                    var pi = (offset + x) * _channels;
                    var r = imageData[pi];
                    var g = imageData[pi + 1];
                    var b = imageData[pi + 2];
                    var a = imageData[pi + 3];

                    if (preMultipliedAlpha && a > 0 && a < 255)
                    {
                        float alpha = a / 255f;
                        r = (byte)Math.Min(r / alpha, 255);
                        g = (byte)Math.Min(g / alpha, 255);
                        b = (byte)Math.Min(b / alpha, 255);
                    }

                    chR[i] = r;
                    chG[i] = g;
                    chB[i] = b;
                    chA[i] = a;
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
