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
    /// MOV 导出参数
    /// </summary>
    [ExportImplementation(ExportType.Mov)]
    public class MovExportArgs : FFmpegVideoExportArgs
    {
        public MovExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly)
        {
            BackgroundColor = new(0, 255, 0);
        }

        public override string Format => "mov";

        public override string Suffix => ".mov";

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("prores_ks", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("编码器"), Description("-c:v, 要使用的编码器")]
        public string Codec { get; set; } = "prores_ks";

        /// <summary>
        /// 预设
        /// </summary>
        [StringEnumConverter.StandardValues("auto", "proxy", "lt", "standard", "hq", "4444", "4444xq")]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("预设"), Description("-profile, 预设配置")]
        public string Profile { get; set; } = "auto";

        /// <summary>
        /// 像素格式
        /// </summary>
        [StringEnumConverter.StandardValues("yuv422p10le", "yuv444p10le", "yuva444p10le", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [Category("[3] 格式参数"), DisplayName("像素格式"), Description("-pix_fmt, 要使用的像素格式")]
        public string PixelFormat { get; set; } = "yuva444p10le";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithFastStart().WithVideoCodec(Codec).WithCustomArgument($"-profile {Profile}").ForcePixelFormat(PixelFormat);
        }

        public override string FileNameNoteSuffix => $"{Codec}_{Profile}_{PixelFormat}";
    }
}
