using FFMpegCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter
{
    /// <summary>
    /// GIF 导出参数
    /// </summary>
    public class GifExporter : FFmpegVideoExporter
    {
        public GifExporter()
        {
            FPS = 24;
        }

        public override string Format => "gif";

        public override string Suffix => ".gif";

        /// <summary>
        /// 调色板最大颜色数量
        /// </summary>
        public uint MaxColors { get => maxColors; set => maxColors = Math.Clamp(value, 2, 256); }
        private uint maxColors = 256;

        /// <summary>
        /// 透明度阈值
        /// </summary>
        public byte AlphaThreshold { get => alphaThreshold; set => alphaThreshold = value; }
        private byte alphaThreshold = 128;

        /// <summary>
        /// 循环次数, -1 不循环, 0 无限循环, 取值范围 [-1, 65535]
        /// </summary>
        public int Loop { get => loop; set => loop = Math.Clamp(value, -1, 65535); }
        private int loop = 0;

        public override string FileNameNoteSuffix => $"{MaxColors}_{AlphaThreshold}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            var v = $"[0:v] split [s0][s1]";
            var s0 = $"[s0] palettegen=reserve_transparent=1:max_colors={MaxColors} [p]";
            var s1 = $"[s1][p] paletteuse=dither=bayer:alpha_threshold={AlphaThreshold}";
            var customArgs = $"-filter_complex \"{v};{s0};{s1}\" -loop {Loop}";
            options.WithCustomArgument(customArgs);
        }
    }
}
