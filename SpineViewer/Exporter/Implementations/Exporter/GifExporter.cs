using FFMpegCore.Pipes;
using FFMpegCore;
using SpineViewer.Exporter.Implementations.ExportArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.Arguments;

namespace SpineViewer.Exporter.Implementations.Exporter
{
    /// <summary>
    /// GIF 动图导出器
    /// </summary>
    [ExportImplementation(ExportType.GIF)]
    public class GifExporter : VideoExporter
    {
        public GifExporter(GifExportArgs exportArgs) : base(exportArgs) { }

        protected override void ExportSingle(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (GifExportArgs)ExportArgs;

            // 导出单个时必定提供输出文件夹
            var filename = $"{timestamp}_{args.FPS:f0}_{args.MaxColors}_{args.AlphaThreshold}.gif";
            var savePath = Path.Combine(args.OutputDir, filename);

            var videoFramesSource = new RawVideoPipeSource(GetFrames(spinesToRender, worker)) { FrameRate = args.FPS };
            try
            {
                FFMpegArguments
                .FromPipeInput(videoFramesSource)
                .OutputToFile(savePath, true, options => options
                .ForceFormat("gif")
                .WithCustomArgument(args.FFMpegCoreCustomArguments))
                .ProcessSynchronously();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Failed to export gif {}", savePath);
            }
        }

        protected override void ExportIndividual(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (GifExportArgs)ExportArgs;
            foreach (var spine in spinesToRender)
            {
                if (worker?.CancellationPending == true) break; // 取消的日志在 GetFrames 里输出

                // 如果提供了输出文件夹, 则全部导出到输出文件夹, 否则导出到各自的文件夹下
                var filename = $"{spine.Name}_{timestamp}_{args.FPS:f0}_{args.MaxColors}_{args.AlphaThreshold}.gif";
                var savePath = Path.Combine(args.OutputDir ?? spine.AssetsDir, filename);

                var videoFramesSource = new RawVideoPipeSource(GetFrames(spine, worker)) { FrameRate = args.FPS };
                try
                {
                    FFMpegArguments
                    .FromPipeInput(videoFramesSource)
                    .OutputToFile(savePath, true, options => options
                    .ForceFormat("gif")
                    .WithCustomArgument(args.FFMpegCoreCustomArguments))
                    .ProcessSynchronously();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    logger.Error("Failed to export gif {} {}", savePath, spine.SkelPath);
                }
            }
        }
    }
}
