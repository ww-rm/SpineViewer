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
    public class Mp4Exporter : FFmpegVideoExporter
    {
        public Mp4Exporter()
        {
            BackgroundColor = new(0, 255, 0);
        }

        public override string Format => "mp4";

        public override string Suffix => ".mp4";

        /// <summary>
        /// 编码器
        /// </summary>
        public string Codec { get; set; } = "libx264";

        /// <summary>
        /// CRF
        /// </summary>
        public int CRF { get => crf; set => crf = Math.Clamp(value, 0, 63); }
        private int crf = 23;

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat { get; set; } = "yuv444p";

        public override string FileNameNoteSuffix => $"{Codec}_{CRF}_{PixelFormat}";

        public override void SetOutputOptions(FFMpegArgumentOptions options)
        {
            base.SetOutputOptions(options);
            options.WithFastStart().WithVideoCodec(Codec).WithConstantRateFactor(CRF).ForcePixelFormat(PixelFormat);
        }
    }

    public class Mp4ExporterProperty(FFmpegVideoExporter exporter) : FFmpegVideoExporterProperty(exporter)
    {
        [Browsable(false)]
        public override Mp4Exporter Exporter => (Mp4Exporter)base.Exporter;

        /// <summary>
        /// 编码器
        /// </summary>
        [StringEnumConverter.StandardValues("libx264", "libx265", Customizable = true)]
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
    }
}
