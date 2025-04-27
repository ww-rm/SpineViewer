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
    /// MP4 导出参数
    /// </summary>
    public class AvifExporter : FFmpegVideoExporter
    {
        public AvifExporter()
        {
            FPS = 24;
        }

        public override string Format => "avif";

        public override string Suffix => ".avif";

        /// <summary>
        /// 编码器
        /// </summary>
        public string Codec { get; set; } = "av1_nvenc";

        /// <summary>
        /// CRF
        /// </summary>
        public int CRF { get => crf; set => crf = Math.Clamp(value, 0, 63); }
        private int crf = 23;

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "yuv420p";

        /// <summary>
        /// 循环次数, 0 无限循环, 取值范围 [0, 65535]
        /// </summary>
        public int Loop { get => loop; set => loop = Math.Clamp(value, 0, 65535); }
        private int loop = 0;

        public override string FileNameNoteSuffix => $"{Codec}_{CRF}_{PixelFormat}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithVideoCodec(Codec).ForcePixelFormat(PixelFormat).WithConstantRateFactor(CRF).WithCustomArgument($"-loop {Loop}");
        }
    }

    public class AvifExporterProperty(AvifExporter exporter) : FFmpegVideoExporterProperty(exporter)
    {
        [Browsable(false)]
        public override AvifExporter Exporter => (AvifExporter)base.Exporter;

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("av1_nvenc", "av1_amf", "libaom-av1", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
        [LocalizedCategory(typeof(Properties.Resources), "categoryFormatParameters")]
        [LocalizedDisplayName(typeof(Properties.Resources), "displayEncoder")]
        [LocalizedDescription(typeof(Properties.Resources), "descAvifEncoder")]
		public string Codec { get => Exporter.Codec; set => Exporter.Codec = value; }

		/// <summary>
		/// CRF
		/// </summary>
		[LocalizedCategory(typeof(Properties.Resources), "categoryFormatParameters")]
		[DisplayName("CRF")]
		[LocalizedDescription(typeof(Properties.Resources), "descCRF")]
        public int CRF { get => Exporter.CRF; set => Exporter.CRF = value; }

        /// <summary>
        /// 像素格式
        /// </summary>
        [StringEnumConverter.StandardValues("yuv420p", "yuv422p", "yuv444p", Customizable = true)]
        [TypeConverter(typeof(StringEnumConverter))]
		[LocalizedCategory(typeof(Properties.Resources), "categoryFormatParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayPixelFormat")]
		[LocalizedDescription(typeof(Properties.Resources), "descPixelFormat")]
        public string PixelFormat { get => Exporter.PixelFormat; set => Exporter.PixelFormat = value; }

		/// <summary>
		/// 循环次数
		/// </summary>
		[LocalizedCategory(typeof(Properties.Resources), "categoryFormatParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayLoopCount")]
		[LocalizedDescription(typeof(Properties.Resources), "descLoopCount")]
        public int Loop { get => Exporter.Loop; set => Exporter.Loop = value; }
    }
}
