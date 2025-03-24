using FFMpegCore.Pipes;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ExportHelper
{
    /// <summary>
    /// 为帧导出创建的辅助类
    /// </summary>
    public static class ExportHelper
    {
        /// <summary>
        /// 从纹理对象获取 Winforms Bitmap 对象
        /// </summary>
        public static Bitmap CopyToBitmap(this SFML.Graphics.Texture tex)
        {
            using var img = tex.CopyToImage();
            img.SaveToMemory(out var imgBuffer, "bmp");
            using var stream = new MemoryStream(imgBuffer);
            return new Bitmap(stream);
        }

        /// <summary>
        /// 从纹理获取适合 FFMpegCore 的帧对象
        /// </summary>
        public static SFMLImageVideoFrame CopyToFrame(this SFML.Graphics.Texture tex) => new(tex.CopyToImage());

        /// <summary>
        /// 根据文件格式获取合适的文件后缀
        /// </summary>
        public static string GetSuffix(this ImageFormat imageFormat)
        {
            if (imageFormat == ImageFormat.Icon) return ".ico";
            else if (imageFormat == ImageFormat.Exif) return ".jpg";
            else return $".{imageFormat.ToString().ToLower()}";
        }

        /// <summary>
        /// 获取某个包围盒下合适的视图
        /// </summary>
        public static SFML.Graphics.View GetView(this RectangleF bounds, Size resolution, Padding padding)
            => GetView(bounds, (uint)resolution.Width, (uint)resolution.Height, (uint)padding.Left, (uint)padding.Right, (uint)padding.Top, (uint)padding.Bottom);

        /// <summary>
        /// 获取某个包围盒下合适的视图
        /// </summary>
        public static SFML.Graphics.View GetView(this RectangleF bounds, uint width, uint height, Padding padding)
            => GetView(bounds, width, height, (uint)padding.Left, (uint)padding.Right, (uint)padding.Top, (uint)padding.Bottom);

        /// <summary>
        /// 获取某个包围盒下合适的视图
        /// </summary>
        public static SFML.Graphics.View GetView(this RectangleF bounds, Size resolution, uint paddingL = 1, uint paddingR = 1, uint paddingT = 1, uint paddingB = 1)
            => GetView(bounds, (uint)resolution.Width, (uint)resolution.Height, paddingL, paddingR, paddingT, paddingB);

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
            if ((sizeY / sizeX) < (innerH / innerW))
                scale = sizeX / innerW; // 相同的 X, 视窗 Y 更大
            else
                scale = sizeY / innerH; // 相同的 Y, 视窗 X 更大

            var x = bounds.X + bounds.Width / 2 + ((float)paddingL - (float)paddingR) * scale;
            var y = bounds.Y + bounds.Height / 2 + ((float)paddingT - (float)paddingB) * scale;
            var viewX = width * scale;
            var viewY = height * scale;

            return new(new(x, y), new(viewX, -viewY));
        }
    }
}
