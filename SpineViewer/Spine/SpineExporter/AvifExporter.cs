using FFMpegCore;
using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.SpineExporter
{
    /// <summary>
    /// MP4 导出参数
    /// </summary>
    public class AvifExporter : FFmpegVideoExporter
    {
        public AvifExporter()
        {
            FPS = 24;
        }

        public override string Format => "avif";

        public override string Suffix => ".avif";

        /// <summary>
        /// 编码器
        /// </summary>
        public string Codec { get; set; } = "av1_nvenc";

        /// <summary>
        /// CRF
        /// </summary>
        public int CRF { get => crf; set => crf = Math.Clamp(value, 0, 63); }
        private int crf = 23;

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "yuv420p";

        /// <summary>
        /// 循环次数, 0 无限循环, 取值范围 [0, 65535]
        /// </summary>
        public int Loop { get => loop; set => loop = Math.Clamp(value, 0, 65535); }
        private int loop = 0;

        public override string FileNameNoteSuffix => $"{Codec}_{CRF}_{PixelFormat}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithVideoCodec(Codec).ForcePixelFormat(PixelFormat).WithConstantRateFactor(CRF).WithCustomArgument($"-loop {Loop}");
        }
    }

    public class AvifExporterProperty(AvifExporter exporter) : FFmpegVideoExporterProperty(exporter)
    {
        [Browsable(false)]
        public override AvifExporter Exporter => (AvifExporter)base.Exporter;

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("av1_nvenc", "av1_amf", "libaom-av1", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("编码器"), Description("-c:v, 要使用的编码器\n建议使用硬件加速, libaom-av1 速度非常非常非常慢")]
        public string Codec { get => Exporter.Codec; set => Exporter.Codec = value; }

        /// <summary>
        /// CRF
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("CRF"), Description("-crf, 取值范围 0-63, 建议范围 18-28, 默认取值 23, 数值越小则输出质量越高")]
        public int CRF { get => Exporter.CRF; set => Exporter.CRF = value; }

        /// <summary>
        /// 像素格式
        /// </summary>
        [StringEnumConverter.StandardValues("yuv420p", "yuv422p", "yuv444p", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("像素格式"), Description("-pix_fmt, 要使用的像素格式")]
        public string PixelFormat { get => Exporter.PixelFormat; set => Exporter.PixelFormat = value; }

        /// <summary>
        /// 循环次数
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("循环次数"), Description("-loop, 循环次数, 0 无限循环, 取值范围 [0, 65535]")]
        public int Loop { get => Exporter.Loop; set => Exporter.Loop = value; }
    }
}
