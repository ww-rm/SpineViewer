using FFMpegCore.Pipes;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SpineViewer
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

    /// <summary>
    /// 为帧导出创建的辅助类
    /// </summary>
    public static class ExportHelper
    {
        public static Bitmap CopyToBitmap(this SFML.Graphics.Texture tex)
        {
            using var img = tex.CopyToImage();
            img.SaveToMemory(out var imgBuffer, "bmp");
            using var stream = new MemoryStream(imgBuffer);
            return new Bitmap(stream);
        }

        public static SFMLImageVideoFrame CopyToFrame(this SFML.Graphics.Texture tex) => new(tex.CopyToImage());

        public static string GetSuffix(this ImageFormat imageFormat)
        {
            if (imageFormat == ImageFormat.Icon) return ".ico";
            else if (imageFormat == ImageFormat.Exif) return ".jpg";
            else return $".{imageFormat.ToString().ToLower()}";
        }
    }
}
