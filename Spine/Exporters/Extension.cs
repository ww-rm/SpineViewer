using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Exporters
{
    public static class Extension
    {
        /// <summary>
        /// 获取适合指定画布参数下能够覆盖包围盒的画布视区包围盒
        /// </summary>
        public static FloatRect GetCanvasBounds(this FloatRect self, Vector2u resolution) => GetCanvasBounds(self, resolution, 0, 0);

        /// <summary>
        /// 获取适合指定画布参数下能够覆盖包围盒的画布视区包围盒
        /// </summary>
        public static FloatRect GetCanvasBounds(this FloatRect self, Vector2u resolution, uint margin) => GetCanvasBounds(self, resolution, margin, 0);

        /// <summary>
        /// 获取适合指定画布参数下能够覆盖包围盒的画布视区包围盒
        /// </summary>
        public static FloatRect GetCanvasBounds(this FloatRect self, Vector2u resolution, uint margin, uint padding)
        {
            float sizeW = self.Width;
            float sizeH = self.Height;
            float innerW = resolution.X - padding * 2;
            float innerH = resolution.Y - padding * 2;
            var scale = Math.Max(Math.Abs(sizeW / innerW), Math.Abs(sizeH / innerH)); // 取两方向上较大的缩放比, 以此让画布可以覆盖内容
            var scaleW = scale * Math.Sign(sizeW);
            var scaleH = scale * Math.Sign(sizeH);

            innerW *= scaleW;
            innerH *= scaleH;

            var x = self.Left - (innerW - sizeW) / 2 - (margin + padding) * scaleW;
            var y = self.Top - (innerH - sizeH) / 2 - (margin + padding) * scaleH;
            var w = (resolution.X + margin * 2) * scaleW;
            var h = (resolution.Y + margin * 2) * scaleH;
            return new(x, y, w, h);
        }

        /// <summary>
        /// 获取视区的包围盒
        /// </summary>
        public static FloatRect GetBounds(this View self)
        {
            return new(
                self.Center.X - self.Size.X / 2,
                self.Center.Y - self.Size.Y / 2,
                self.Size.X,
                self.Size.Y
            );
        }
    }
}
