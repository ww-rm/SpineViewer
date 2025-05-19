using SpineViewer.Spine;
using SpineViewer.Utils.Localize;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.SpineExporter
{
    /// <summary>
    /// 单帧画面导出器
    /// </summary>
    public class FrameExporter : Exporter
    {
        /// <summary>
        /// 单帧画面格式
        /// </summary>
        public ImageFormat ImageFormat
        {
            get => imageFormat;
            set
            {
                if (value == ImageFormat.MemoryBmp) value = ImageFormat.Bmp;
                imageFormat = value;
            }
        }
        private ImageFormat imageFormat = ImageFormat.Png;

        /// <summary>
        /// DPI
        /// </summary>
        public SizeF DPI
        {
            get => dpi;
            set
            {
                if (value.Width <= 0) value.Width = 144;
                if (value.Height <= 0) value.Height = 144;
                dpi = value;
            }
        }
        private SizeF dpi = new(144, 144);

        protected override void ExportSingle(SpineObject[] spinesToRender, BackgroundWorker? worker = null)
        {
            // 导出单个时必定提供输出文件夹
            var filename = $"frame_{timestamp}_{Guid.NewGuid().ToString()[..6]}{ImageFormat.GetSuffix()}";
            var savePath = Path.Combine(OutputDir, filename);

            worker?.ReportProgress(0, $"{Properties.Resources.process} 0/1");
            try
            {
                using var frame = GetFrame(spinesToRender);
                using var img = frame.CopyToBitmap();
                img.SetResolution(DPI.Width, DPI.Height);
                img.Save(savePath, ImageFormat);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Failed to save single frame");
            }
            worker?.ReportProgress(100, $"{Properties.Resources.process} 1/1");
        }

        protected override void ExportIndividual(SpineObject[] spinesToRender, BackgroundWorker? worker = null)
        {
            int total = spinesToRender.Length;
            int success = 0;
            int error = 0;

            worker?.ReportProgress(0, $"{Properties.Resources.process} 0/{total}");
            for (int i = 0; i < total; i++)
            {
                var spine = spinesToRender[i];

                // 逐个导出时如果提供了输出文件夹, 则全部导出到输出文件夹, 否则输出到各自的文件夹
                var filename = $"{spine.Name}_{timestamp}_{spine.ID[..6]}{ImageFormat.GetSuffix()}";
                var savePath = Path.Combine(OutputDir ?? spine.AssetsDir, filename);

                try
                {
                    using var frame = GetFrame(spine);
                    using var img = frame.CopyToBitmap();
                    img.SetResolution(DPI.Width, DPI.Height);
                    img.Save(savePath, ImageFormat);
                    success++;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    logger.Error("Failed to save single frame {} {}", savePath, spine.SkelPath);
                    error++;
                }

                worker?.ReportProgress((int)((i + 1) * 100.0) / total, $"{Properties.Resources.process} {i + 1}/{total}");
            }

            if (error > 0)
                logger.Warn("Frames save {} successfully, {} failed", success, error);
            else
                logger.Info("{} frames saved successfully", success);
        }
    }

    public class FrameExporterProperty(FrameExporter exporter) : ExporterProperty(exporter)
    {
        [Browsable(false)]
        public override FrameExporter Exporter => (FrameExporter)base.Exporter;

        /// <summary>
        /// 单帧画面格式
        /// </summary>
        [TypeConverter(typeof(ImageFormatConverter))]
		[LocalizedCategory(typeof(Properties.Resources), "categorySingleFrame")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayImageFormat")]
        public ImageFormat ImageFormat { get => Exporter.ImageFormat; set => Exporter.ImageFormat = value; }

		/// <summary>
		/// 文件名后缀
		/// </summary>
		[LocalizedCategory(typeof(Properties.Resources), "categorySingleFrame")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayFilenameSuffix")]
		[LocalizedDescription(typeof(Properties.Resources), "descFileNameExtension")]
        public string Suffix { get => Exporter.ImageFormat.GetSuffix(); }

        /// <summary>
        /// DPI
        /// </summary>
        [TypeConverter(typeof(SizeFConverter))]
		[LocalizedCategory(typeof(Properties.Resources), "categorySingleFrame")]
		[LocalizedDescription(typeof(Properties.Resources), "descDPI")]
        public SizeF DPI { get => Exporter.DPI; set => Exporter.DPI = value; }
    }
}
