using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.ExportArgs
{
    /// <summary>
    /// FFmpeg 自定义视频导出参数
    /// </summary>
    [ExportImplementation(ExportType.Custom)]
    public class CustomExportArgs : FFmpegVideoExportArgs
    {
        public CustomExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly) { }

        public override string Format => CustomFormat;

        public override string Suffix => CustomSuffix;

        public override string FileNameNoteSuffix => string.Empty;

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
