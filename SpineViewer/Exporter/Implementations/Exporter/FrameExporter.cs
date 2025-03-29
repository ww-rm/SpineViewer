using SpineViewer.Exporter.Implementations.ExportArgs;
using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.Exporter
{
    /// <summary>
    /// 单帧画面导出器
    /// </summary>
    [ExportImplementation(ExportType.Frame)]
    public class FrameExporter : SpineViewer.Exporter.Exporter
    {
        public FrameExporter(FrameExportArgs exportArgs) : base(exportArgs) { }

        protected override void ExportSingle(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (FrameExportArgs)ExportArgs;

            // 导出单个时必定提供输出文件夹
            var filename = $"frame_{timestamp}{args.FileSuffix}";
            var savePath = Path.Combine(args.OutputDir, filename);

            worker?.ReportProgress(0, $"已处理 0/1");
            try
            {
                using var frame = GetFrame(spinesToRender);
                using var img = frame.CopyToBitmap();
                img.SetResolution(args.DPI.Width, args.DPI.Height);
                img.Save(savePath, args.ImageFormat);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Failed to save single frame");
            }
            worker?.ReportProgress(100, $"已处理 1/1");
        }

        protected override void ExportIndividual(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (FrameExportArgs)ExportArgs;

            int total = spinesToRender.Length;
            int success = 0;
            int error = 0;

            worker?.ReportProgress(0, $"已处理 0/{total}");
            for (int i = 0; i < total; i++)
            {
                var spine = spinesToRender[i];

                // 逐个导出时如果提供了输出文件夹, 则全部导出到输出文件夹, 否则输出到各自的文件夹
                var filename = $"{spine.Name}_{timestamp}{args.FileSuffix}";
                var savePath = args.OutputDir is null ? Path.Combine(spine.AssetsDir, filename) : Path.Combine(args.OutputDir, filename);

                try
                {
                    using var frame = GetFrame(spine);
                    using var img = frame.CopyToBitmap();
                    img.SetResolution(args.DPI.Width, args.DPI.Height);
                    img.Save(savePath, args.ImageFormat);
                    success++;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    logger.Error("Failed to save frame {} {}", savePath, spine.SkelPath);
                    error++;
                }

                worker?.ReportProgress((int)((i + 1) * 100.0) / total, $"已处理 {i + 1}/{total}");
            }

            if (error > 0)
                logger.Warn("Frames save {} successfully, {} failed", success, error);
            else
                logger.Info("{} frames saved successfully", success);
        }
    }

}
