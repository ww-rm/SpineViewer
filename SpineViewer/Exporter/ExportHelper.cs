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
        GIF,
        MKV,
        MP4,
        MOV,
        WebM
    }

    /// <summary>
    /// 导出实现类标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportImplementationAttribute(ExportType exportType) : Attribute, IImplementationKey<ExportType>
    {
        public ExportType ImplementationKey { get; private set; } = exportType;
    }

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

        /// <summary>
        /// 获取 Winforms Bitmap 对象
        /// </summary>
        public Bitmap CopyToBitmap()
        {
            image.SaveToMemory(out var imgBuffer, "bmp");
            using var stream = new MemoryStream(imgBuffer);
            return new(new Bitmap(stream)); // 必须重复构造一个副本才能摆脱对流的依赖, 否则之后使用会报错
        }
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

        #region 包围盒辅助函数

        /// <summary>
        /// 获取某个包围盒下合适的视图
        /// </summary>
        public static SFML.Graphics.View GetView(this RectangleF bounds, Size resolution, Padding padding)
            => bounds.GetView((uint)resolution.Width, (uint)resolution.Height, (uint)padding.Left, (uint)padding.Right, (uint)padding.Top, (uint)padding.Bottom);

        /// <summary>
        /// 获取某个包围盒下合适的视图
        /// </summary>
        public static SFML.Graphics.View GetView(this RectangleF bounds, uint width, uint height, Padding padding)
            => bounds.GetView(width, height, (uint)padding.Left, (uint)padding.Right, (uint)padding.Top, (uint)padding.Bottom);

        /// <summary>
        /// 获取某个包围盒下合适的视图
        /// </summary>
        public static SFML.Graphics.View GetView(this RectangleF bounds, Size resolution, uint paddingL = 1, uint paddingR = 1, uint paddingT = 1, uint paddingB = 1)
            => bounds.GetView((uint)resolution.Width, (uint)resolution.Height, paddingL, paddingR, paddingT, paddingB);

        /// <summary>
        /// 获取某个包围盒下合适的视图
        /// </summary>
        public static SFML.Graphics.View GetView(this RectangleF bounds, uint width, uint height, uint paddingL = 1, uint paddingR = 1, uint paddingT = 1, uint paddingB = 1)
        {
            float sizeX = bounds.Width;
            float sizeY = bounds.Height;
            float innerW = width - paddingL - paddingR;
            float innerH = height - paddingT - paddingB;

            float scale = 1;
            if (sizeY / sizeX < innerH / innerW)
                scale = sizeX / innerW; // 相同的 X, 视窗 Y 更大
            else
                scale = sizeY / innerH; // 相同的 Y, 视窗 X 更大

            var x = bounds.X + bounds.Width / 2 + (paddingL - (float)paddingR) * scale;
            var y = bounds.Y + bounds.Height / 2 + (paddingT - (float)paddingB) * scale;
            var viewX = width * scale;
            var viewY = height * scale;

            return new(new(x, y), new(viewX, -viewY));
        }

        #endregion
    }
}
