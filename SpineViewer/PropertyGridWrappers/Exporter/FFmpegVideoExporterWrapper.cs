using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class FFmpegVideoExporterWrapper(FFmpegVideoExporter exporter) : VideoExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override FFmpegVideoExporter Exporter => (FFmpegVideoExporter)base.Exporter;

        /// <summary>
        /// 文件格式
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("文件格式"), Description("-f, 文件格式")]
        public string Format => Exporter.Format;

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("文件名后缀"), Description("文件名后缀")]
        public string Suffix => Exporter.Format;

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("自定义参数"), Description("提供给 FFmpeg 的自定义参数, 除非很清楚自己在做什么, 否则请勿填写此参数")]
        public string CustomArgument { get => Exporter.CustomArgument; set => Exporter.CustomArgument = value; }
    }
}
