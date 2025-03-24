using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.Pipes;

namespace SpineViewer.ExportHelper
{
    /// <summary>
    /// SFML.Graphics.Image 帧对象包装类
    /// </summary>
    public class SFMLImageVideoFrame(SFML.Graphics.Image image) : IVideoFrame, IDisposable
    {
        public int Width => (int)image.Size.X;
        public int Height => (int)image.Size.Y;
        public string Format => "rgba";
        public void Serialize(Stream pipe) => pipe.Write(image.Pixels);
        public async Task SerializeAsync(Stream pipe, CancellationToken token) => await pipe.WriteAsync(image.Pixels, token);
        public void Dispose() => image.Dispose();

        /// <summary>
        /// Save the contents of the image to a file
        /// </summary>
        /// <param name="filename">Path of the file to save (overwritten if already exist)</param>
        /// <returns>True if saving was successful</returns>
        public bool SaveToFile(string filename) => image.SaveToFile(filename);

        /// <summary>
        /// Save the image to a buffer in memory The format of the image must be specified.
        /// The supported image formats are bmp, png, tga and jpg. This function fails if
        /// the image is empty, or if the format was invalid.
        /// </summary>
        /// <param name="output">Byte array filled with encoded data</param>
        /// <param name="format">Encoding format to use</param>
        /// <returns>True if saving was successful</returns>
        public bool SaveToMemory(out byte[] output, string format) => image.SaveToMemory(out output, format);
    }
}
