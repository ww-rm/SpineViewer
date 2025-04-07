using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class MovExporterWrapper(FFmpegVideoExporter exporter) : FFmpegVideoExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override MovExporter Exporter => (MovExporter)base.Exporter;

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("prores_ks", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("编码器"), Description("-c:v, 要使用的编码器")]
        public string Codec { get => Exporter.Codec; set => Exporter.Codec = value; }

        /// <summary>
        /// 预设
        /// </summary>
        [StringEnumConverter.StandardValues("auto", "proxy", "lt", "standard", "hq", "4444", "4444xq")]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("预设"), Description("-profile, 预设配置")]
        public string Profile { get => Exporter.Profile; set => Exporter.Profile = value; }

        /// <summary>
        /// 像素格式
        /// </summary>
        [StringEnumConverter.StandardValues("yuv422p10le", "yuv444p10le", "yuva444p10le", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("像素格式"), Description("-pix_fmt, 要使用的像素格式")]
        public string PixelFormat { get => Exporter.PixelFormat; set => Exporter.PixelFormat = value; }
    }
}
