using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class CustomExporterWrapper(CustomExporter exporter) : FFmpegVideoExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override CustomExporter Exporter => (CustomExporter)base.Exporter;

        [Browsable(false)]
        public override string Format => Exporter.Format;

        [Browsable(false)]
        public override string Suffix => Exporter.Suffix;

        /// <summary>
        /// 文件格式
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("文件格式"), Description("-f, 文件格式")]
        public string CustomFormat { get => Exporter.CustomFormat; set => Exporter.CustomFormat = value; }

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("文件名后缀"), Description("文件名后缀")]
        public string CustomSuffix { get => Exporter.CustomSuffix; set => Exporter.CustomSuffix = value; }

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("自定义参数"), Description("提供给 FFmpeg 的自定义参数")]
        public override string CustomArgument { get => Exporter.CustomArgument; set => Exporter.CustomArgument = value; }

    }
}
