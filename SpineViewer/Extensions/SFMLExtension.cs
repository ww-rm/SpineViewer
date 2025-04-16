using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Extensions
{
    public static class SFMLExtension
    {
        /// <summary>
        /// 获取 Winforms Bitmap 对象, 需要使用 Dispose 释放对象
        /// </summary>
        public static Bitmap CopyToBitmap(this SFML.Graphics.Image image)
        {
            image.SaveToMemory(out var imgBuffer, "bmp");
            using var stream = new MemoryStream(imgBuffer);
            using var bitmap = new Bitmap(stream);
            return new(bitmap); // 必须重复构造一个副本才能摆脱对流的依赖, 否则之后使用会报错
        }

        /// <summary>
        /// 获取 Winforms Bitmap 对象, 需要使用 Dispose 释放对象
        /// </summary>
        public static Bitmap CopyToBitmap(this SFML.Graphics.Texture texture)
        {
            using var image = texture.CopyToImage();
            return image.CopyToBitmap();
        }

        /// <summary>
        /// 获取适合指定画布参数下能够覆盖包围盒的视区包围盒
        /// </summary>
        public static RectangleF GetResolutionBounds(this RectangleF bounds, Size resolution, Padding padding, Padding margin)
        {
            float sizeW = bounds.Width;
            float sizeH = bounds.Height;
            float innerW = resolution.Width - padding.Horizontal;
            float innerH = resolution.Height - padding.Vertical;
            float scale = Math.Max(sizeW / innerW, sizeH / innerH); // 取两方向上较大的缩放比, 以此让画布可以覆盖内容
            return new(
                bounds.X + (padding.Left + margin.Left - padding.Right - margin.Right) * scale,
                bounds.Y + (padding.Top + margin.Top - padding.Bottom - margin.Bottom) * scale,
                (resolution.Width + margin.Horizontal) * scale,
                (resolution.Height + margin.Vertical) * scale
            );
        }
    }
}
