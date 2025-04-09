using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class WebpExporterWrapper(WebpExporter exporter) : FFmpegVideoExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override WebpExporter Exporter => (WebpExporter)base.Exporter;

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("libwebp", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("编码器"), Description("-c:v, 要使用的编码器")]
        public string Codec { get => Exporter.Codec; set => Exporter.Codec = value; }

        /// <summary>
        /// 是否无损
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("无损"), Description("-lossless, 0 表示有损, 1 表示无损")]
        public bool Lossless { get => Exporter.Lossless; set => Exporter.Lossless = value; }

        /// <summary>
        /// CRF
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("质量"), Description("-quality, 取值范围 0-100, 默认值 75")]
        public int Quality { get => Exporter.Quality; set => Exporter.Quality = value; }

        /// <summary>
        /// 像素格式
        /// </summary>
        [StringEnumConverter.StandardValues("yuv420p", "yuva420p", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("像素格式"), Description("-pix_fmt, 要使用的像素格式")]
        public string PixelFormat { get => Exporter.PixelFormat; set => Exporter.PixelFormat = value; }

        /// <summary>
        /// 透明度阈值
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("循环次数"), Description("循环次数, 0 无限循环, 取值范围 [0, 65535]")]
        public int Loop { get => Exporter.Loop; set => Exporter.Loop = value; }
    }
}
