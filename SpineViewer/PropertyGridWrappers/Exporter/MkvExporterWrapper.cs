using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class MkvExporterWrapper(FFmpegVideoExporter exporter) : FFmpegVideoExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override MkvExporter Exporter => (MkvExporter)base.Exporter;

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("libx264", "libx265", "libvpx-vp9", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("编码器"), Description("-c:v, 要使用的编码器")]
        public string Codec { get => Exporter.Codec; set => Exporter.Codec = value; }

        /// <summary>
        /// CRF
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("CRF"), Description("-crf, 取值范围 0-63, 建议范围 18-28, 默认取值 23, 数值越小则输出质量越高")]
        public int CRF { get => Exporter.CRF; set => Exporter.CRF = value; }

        /// <summary>
        /// 像素格式
        /// </summary>
        [StringEnumConverter.StandardValues("yuv420p", "yuv422p", "yuv444p", "yuva420p", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("像素格式"), Description("-pix_fmt, 要使用的像素格式")]
        public string PixelFormat { get => Exporter.PixelFormat; set => Exporter.PixelFormat = value; }
    }
}
