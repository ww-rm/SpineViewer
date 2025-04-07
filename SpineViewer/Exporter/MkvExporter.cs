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
    /// MKV 导出参数
    /// </summary>
    public class MkvExporter : FFmpegVideoExporter
    {
        public MkvExporter()
        {
            BackgroundColor = new(0, 255, 0);
        }

        public override string Format => "matroska";

        public override string Suffix => ".mkv";

        /// <summary>
        /// 编码器
        /// </summary>
        public string Codec { get; set; } = "libx265";

        /// <summary>
        /// CRF
        /// </summary>
        public int CRF { get => crf; set => crf = Math.Clamp(value, 0, 63); }
        private int crf = 23;

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "yuv444p";

        public override string FileNameNoteSuffix => $"{Codec}_{CRF}_{PixelFormat}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithVideoCodec(Codec).WithConstantRateFactor(CRF).ForcePixelFormat(PixelFormat);
        }
    }
}
