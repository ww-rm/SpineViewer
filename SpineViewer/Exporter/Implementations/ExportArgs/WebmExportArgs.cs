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
    /// WebM 导出参数
    /// </summary>
    [ExportImplementation(ExportType.Webm)]
    public class WebmExportArgs : FFmpegVideoExportArgs
    {
        public WebmExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly)
        {
            // 默认用透明黑背景
            BackgroundColor = new(0, 0, 0, 0);
        }

        public override string Format => "webm";

        public override string Suffix => ".webm";

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("libvpx-vp9", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("编码器"), Description("-c:v, 要使用的编码器")]
        public string Codec { get; set; } = "libvpx-vp9";

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
        [Category("[3] 格式参数"), DisplayName("像素格式"), Description("-pix_fmt, 要使用的像素格式")]
        public string PixelFormat { get; set; } = "yuva420p";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithVideoCodec(Codec).WithConstantRateFactor(CRF).ForcePixelFormat(PixelFormat);
        }

        public override string FileNameNoteSuffix => $"{Codec}_{CRF}_{PixelFormat}";
    }
}
