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

        /// <summary>
        /// 文件格式
        /// </summary>
        [Category("[3] 自定义参数"), DisplayName("文件格式"), Description("文件格式")]
        public string CustomFormat { get; set; } = "mp4";

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[3] 自定义参数"), DisplayName("文件名后缀"), Description("文件名后缀")]
        public string CustomSuffix { get; set; } = ".mp4";
    }
}
