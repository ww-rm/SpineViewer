using FFMpegCore.Enums;
using FFMpegCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.ExportArgs
{
    /// <summary>
    /// 使用 FFmpeg 视频导出参数
    /// </summary>
    public abstract class FFmpegVideoExportArgs : VideoExportArgs
    {
        public FFmpegVideoExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly) { }

        /// <summary>
        /// 文件格式
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("文件格式"), Description("文件格式")]
        public abstract string Format { get; }

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("文件名后缀"), Description("文件名后缀")]
        public abstract string Suffix { get; }

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[2] FFmpeg 基本参数"), DisplayName("自定义参数"), Description("提供给 FFmpeg 的自定义参数, 除非很清楚自己在做什么, 否则请勿填写此参数")]
        public string CustomArgument { get; set; }

        /// <summary>
        /// 获取输出附加选项
        /// </summary>
        public virtual void SetOutputOptions(FFMpegArgumentOptions options) => options.ForceFormat(Format).WithCustomArgument(CustomArgument);

        /// <summary>
        /// 要追加在文件名末尾的信息字串, 首尾不需要提供额外分隔符
        /// </summary>
        [Browsable(false)]
        public abstract string FileNameNoteSuffix { get; }

        public override string? Validate()
        {
            if (base.Validate() is string error)
                return error;
            if (string.IsNullOrWhiteSpace(Format))
                return "需要提供有效的格式";
            if (string.IsNullOrWhiteSpace(Suffix))
                return "需要提供有效的文件名后缀";
            return null;
        }
    }
}
