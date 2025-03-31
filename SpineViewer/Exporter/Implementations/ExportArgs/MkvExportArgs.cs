using FFMpegCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.ExportArgs
{
    /// <summary>
    /// MKV 导出参数
    /// </summary>
    [ExportImplementation(ExportType.Mkv)]
    public class MkvExportArgs : FFmpegVideoExportArgs
    {
        public MkvExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly)
        {
            BackgroundColor = new(0, 255, 0, 0);
        }

        public override string Format => "matroska";

        public override string Suffix => ".mkv";

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("libx264", "libx265", "libvpx-vp9", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("编码器"), Description("要使用的编码器")]
        public string Codec { get; set; } = "libx265";

        /// <summary>
        /// CRF
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("CRF"), Description("-crf, 取值范围 0-63, 建议范围 18-28, 默认取值 23, 数值越小则输出质量越高")]
        public int CRF { get => crf; set => crf = Math.Clamp(value, 0, 63); }
        private int crf = 23;

        /// <summary>
        /// 像素格式
        /// </summary>
        [StringEnumConverter.StandardValues("yuv420p", "yuv422p", "yuv444p", "yuva420p", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("像素格式"), Description("要使用的像素格式")]
        public string PixelFormat { get; set; } = "yuv444p";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithVideoCodec(Codec).WithConstantRateFactor(CRF).ForcePixelFormat(PixelFormat);
        }

        public override string FileNameNoteSuffix => $"{Codec}_{CRF}_{PixelFormat}";
    }
}
