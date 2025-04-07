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
    }
}
