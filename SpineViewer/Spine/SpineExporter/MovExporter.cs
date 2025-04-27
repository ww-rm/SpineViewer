using FFMpegCore;
using SpineViewer.Utils;
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
    /// MOV 导出参数
    /// </summary>
    public class MovExporter : FFmpegVideoExporter
    {
        public MovExporter()
        {
            BackgroundColor = new(0, 255, 0);
        }

        public override string Format => "mov";

        public override string Suffix => ".mov";

        /// <summary>
        /// 编码器
        /// </summary>
        public string Codec { get; set; } = "prores_ks";

        /// <summary>
        /// 预设
        /// </summary>
        public string Profile { get; set; } = "auto";

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "yuva444p10le";

        public override string FileNameNoteSuffix => $"{Codec}_{Profile}_{PixelFormat}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithFastStart().WithVideoCodec(Codec).WithCustomArgument($"-profile {Profile}").ForcePixelFormat(PixelFormat);
        }
    }

    public class MovExporterProperty(FFmpegVideoExporter exporter) : FFmpegVideoExporterProperty(exporter)
    {
        [Browsable(false)]
        public override MovExporter Exporter => (MovExporter)base.Exporter;

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("prores_ks", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
		[LocalizedCategory(typeof(Properties.Resources), "categoryFormatParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayEncoder")]
		[LocalizedDescription(typeof(Properties.Resources), "descAvifEncoder")]
		public string Codec { get => Exporter.Codec; set => Exporter.Codec = value; }

        /// <summary>
        /// 预设
        /// </summary>
        [StringEnumConverter.StandardValues("auto", "proxy", "lt", "standard", "hq", "4444", "4444xq")]
        [TypeConverter(typeof(StringEnumConverter))]
		[LocalizedCategory(typeof(Properties.Resources), "categoryFormatParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayPreset")]
		[LocalizedDescription(typeof(Properties.Resources), "descPreset")]
        public string Profile { get => Exporter.Profile; set => Exporter.Profile = value; }

        /// <summary>
        /// 像素格式
        /// </summary>
        [StringEnumConverter.StandardValues("yuv422p10le", "yuv444p10le", "yuva444p10le", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
		[LocalizedCategory(typeof(Properties.Resources), "categoryFormatParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayPixelFormat")]
		[LocalizedDescription(typeof(Properties.Resources), "descPixelFormat")]
		public string PixelFormat { get => Exporter.PixelFormat; set => Exporter.PixelFormat = value; }
    }
}
