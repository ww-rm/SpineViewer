using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter
{
    /// <summary>
    /// FFmpeg 自定义视频导出参数
    /// </summary>
    public class CustomExporter : FFmpegVideoExporter
    {
        public CustomExporter() 
        {
            CustomArgument = "-c:v libx264 -crf 23 -pix_fmt yuv420p"; // 提供一个示例参数
        }

        public override string Format => CustomFormat;

        public override string Suffix => CustomSuffix;

        public override string FileNameNoteSuffix => string.Empty;

        /// <summary>
        /// 文件格式
        /// </summary>
        public string CustomFormat { get; set; } = "mp4";

        /// <summary>
        /// 文件名后缀
        /// </summary>
        public string CustomSuffix { get; set; } = ".mp4";
    }
}
