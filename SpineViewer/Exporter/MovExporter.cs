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
    /// MOV 导出参数
    /// </summary>
    public class MovExporter : FFmpegVideoExporter
    {
        public MovExporter()
        {
            BackgroundColor = new(0, 255, 0);
        }

        public override string Format => "mov";

        public override string Suffix => ".mov";

        /// <summary>
        /// 编码器
        /// </summary>
        public string Codec { get; set; } = "prores_ks";

        /// <summary>
        /// 预设
        /// </summary>
        public string Profile { get; set; } = "auto";

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "yuva444p10le";

        public override string FileNameNoteSuffix => $"{Codec}_{Profile}_{PixelFormat}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithFastStart().WithVideoCodec(Codec).WithCustomArgument($"-profile {Profile}").ForcePixelFormat(PixelFormat);
        }
    }
}
