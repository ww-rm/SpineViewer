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
    /// WebM 导出参数
    /// </summary>
    public class WebmExporter : FFmpegVideoExporter
    {
        public WebmExporter()
        {
            // 默认用透明黑背景
            BackgroundColor = new(0, 0, 0, 0);
        }

        public override string Format => "webm";

        public override string Suffix => ".webm";

        /// <summary>
        /// 编码器
        /// </summary>
        public string Codec { get; set; } = "libvpx-vp9";

        /// <summary>
        /// CRF
        /// </summary>
        public int CRF { get => crf; set => crf = Math.Clamp(value, 0, 63); }
        private int crf = 23;

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "yuva420p";

        public override string FileNameNoteSuffix => $"{Codec}_{CRF}_{PixelFormat}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithVideoCodec(Codec).WithConstantRateFactor(CRF).ForcePixelFormat(PixelFormat);
        }
    }
}
