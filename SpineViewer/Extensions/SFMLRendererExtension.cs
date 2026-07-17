using SFMLRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Extensions
{
    public static class SFMLRendererExtension
    {
        /// <summary>
        /// 获取边长为正数的视区包围盒矩形
        /// </summary>
        public static Rect GetAbsBounds(this ISFMLRenderer renderer)
        {
            using var view = renderer.GetView();
            var cx = view.Center.X;
            var cy = view.Center.Y;
            var w = MathF.Abs(view.Size.X);
            var h = MathF.Abs(view.Size.Y);
            var rendererBounds = new Rect(cx - w / 2, cy - h / 2, w, h);
            return rendererBounds;
        }
    }
}
