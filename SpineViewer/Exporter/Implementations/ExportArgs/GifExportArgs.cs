using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.ExportArgs
{
    /// <summary>
    /// GIF 导出参数
    /// </summary>
    [ExportImplementation(ExportType.GIF)]
    public class GifExportArgs : VideoExportArgs
    {
        public GifExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly) 
        {
            // 给一个纯白的背景
            BackgroundColor = new(255, 255, 255, 0);

            // GIF 的帧率不能太高, 超过 50 帧反而会变慢
            FPS = 12;
        }

        /// <summary>
        /// 调色板最大颜色数量
        /// </summary>
        [Category("[2] GIF 参数"), DisplayName("调色板最大颜色数量"), Description("设置调色板使用的最大颜色数量, 越多则色彩保留程度越高")]
        public uint MaxColors { get => maxColors; set => maxColors = Math.Clamp(value, 2, 256); }
        private uint maxColors = 256;

        /// <summary>
        /// 透明度阈值
        /// </summary>
        [Category("[2] GIF 参数"), DisplayName("透明度阈值"), Description("小于该值的像素点会被认为是透明像素")]
        public byte AlphaThreshold { get => alphaThreshold; set => alphaThreshold = value; }
        private byte alphaThreshold = 128;

        /// <summary>
        /// 获取构造好的 FFMpegCore 自定义参数
        /// </summary>
        [Browsable(false)]
        public string FFMpegCoreCustomArguments
        {
            get
            {
                var v = $"[0:v] split [s0][s1]";
                var s0 = $"[s0] palettegen=reserve_transparent=1:max_colors={MaxColors} [p]";
                var s1 = $"[s1][p] paletteuse=dither=bayer:alpha_threshold={AlphaThreshold}";
                return $"-filter_complex \"{v};{s0};{s1}\"";
            }
        }
    }
}
