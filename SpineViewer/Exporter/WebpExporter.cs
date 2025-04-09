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
    /// MP4 导出参数
    /// </summary>
    public class WebpExporter : FFmpegVideoExporter
    {
        public WebpExporter()
        {
            FPS = 24;
        }

        public override string Format => "webp";

        public override string Suffix => ".webp";

        /// <summary>
        /// 编码器
        /// </summary>
        public string Codec { get; set; } = "libwebp_anim";

        /// <summary>
        /// 是否无损
        /// </summary>
        public bool Lossless { get; set; } = false;

        /// <summary>
        /// 质量
        /// </summary>
        public int Quality { get => quality; set => quality = Math.Clamp(value, 0, 100); }
        private int quality = 75;

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "yuva420p";

        /// <summary>
        /// 循环次数, 0 无限循环, 取值范围 [0, 65535]
        /// </summary>
        public int Loop { get => loop; set => loop = Math.Clamp(value, 0, 65535); }
        private int loop = 0;

        public override string FileNameNoteSuffix => $"{Codec}_{Quality}_{PixelFormat}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithVideoCodec(Codec).WithCustomArgument($"-lossless {(Lossless ? 1 : 0)} -quality {Quality} -loop {Loop}");
        }
    }
}
