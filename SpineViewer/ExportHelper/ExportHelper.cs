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
    }
}
