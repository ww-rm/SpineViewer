using FFMpegCore.Pipes;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter
{
    /// <summary>
    /// 导出类型
    /// </summary>
    public enum ExportType
    {
        Frame,
        FrameSequence,
        Gif,
        Mp4,
        Webm,
        Mkv,
        Mov,
        Custom,
    }

    /// <summary>
    /// 导出实现类标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ExportImplementationAttribute(ExportType exportType) : Attribute, IImplementationKey<ExportType>
    {
        public ExportType ImplementationKey { get; private set; } = exportType;
    }

    /// <summary>
    /// SFML.Graphics.Image 帧对象包装类, 将接管给定的 image 对象生命周期
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

        /// <summary>
        /// 获取 Winforms Bitmap 对象
        /// </summary>
        public Bitmap CopyToBitmap() => image.CopyToBitmap();
    }

    /// <summary>
    /// 为帧导出创建的辅助类
    /// </summary>
    public static class ExportHelper
    {
        /// <summary>
        /// 根据 Bitmap 文件格式获取合适的文件后缀
        /// </summary>
        public static string GetSuffix(this ImageFormat imageFormat)
        {
            if (imageFormat == ImageFormat.Icon) return ".ico";
            else if (imageFormat == ImageFormat.Exif) return ".jpeg";
            else return $".{imageFormat.ToString().ToLower()}";
        }
    }
}
