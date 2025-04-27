using SpineViewer.Utils.Localize;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.SpineExporter
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

    public class CustomExporterProperty(CustomExporter exporter) : FFmpegVideoExporterProperty(exporter)
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
		[LocalizedCategory(typeof(Properties.Resources), "categoryFFmpegParameter")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayFileFormat")]
		[LocalizedDescription(typeof(Properties.Resources), "descFileFormat")]
        public string CustomFormat { get => Exporter.CustomFormat; set => Exporter.CustomFormat = value; }

		/// <summary>
		/// 文件名后缀
		/// </summary>
		[LocalizedCategory(typeof(Properties.Resources), "categoryFFmpegParameter")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayFilenameSuffix")]
		[LocalizedDescription(typeof(Properties.Resources), "descFilenameSuffix")]
        public string CustomSuffix { get => Exporter.CustomSuffix; set => Exporter.CustomSuffix = value; }
    }
}
