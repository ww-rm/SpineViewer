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
        public virtual string Format => Exporter.Format;

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("文件名后缀"), Description("文件名后缀")]
        public virtual string Suffix => Exporter.Suffix;

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("自定义参数"), Description("使用 \"ffmpeg -h -encoder=<编码器>\" 查看编码器支持的参数\n使用 \"ffmpeg -h -muxer=<文件格式>\" 查看文件格式支持的参数")]
        public string CustomArgument { get => Exporter.CustomArgument; set => Exporter.CustomArgument = value; }
    }
}
