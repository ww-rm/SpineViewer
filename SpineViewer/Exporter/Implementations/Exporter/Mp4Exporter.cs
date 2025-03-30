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
using System.Diagnostics;

namespace SpineViewer.Exporter.Implementations.Exporter
{
    /// <summary>
    /// MP4 导出器
    /// </summary>
    [ExportImplementation(ExportType.MP4)]
    public class Mp4Exporter : VideoExporter
    {
        public Mp4Exporter(Mp4ExportArgs exportArgs) : base(exportArgs) { }

        protected override void ExportSingle(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (Mp4ExportArgs)ExportArgs;

            // 导出单个时必定提供输出文件夹
            var filename = $"{timestamp}_{args.FPS:f0}_{args.CRF}.mp4";
            var savePath = Path.Combine(args.OutputDir, filename);

            var videoFramesSource = new RawVideoPipeSource(GetFrames(spinesToRender, worker)) { FrameRate = args.FPS };
            try
            {
                var ffmpegArgs = FFMpegArguments
                .FromPipeInput(videoFramesSource)
                .OutputToFile(savePath, true, options => options
                .ForceFormat("mp4")
                .WithVideoCodec(args.Codec)
                .WithConstantRateFactor(args.CRF)
                .WithFastStart());

                logger.Info("FFMpeg arguments: {}", ffmpegArgs.Arguments);
                ffmpegArgs.ProcessSynchronously();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Failed to export mp4 {}", savePath);
            }
        }

        protected override void ExportIndividual(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (Mp4ExportArgs)ExportArgs;
            foreach (var spine in spinesToRender)
            {
                if (worker?.CancellationPending == true) break; // 取消的日志在 GetFrames 里输出

                // 如果提供了输出文件夹, 则全部导出到输出文件夹, 否则导出到各自的文件夹下
                var filename = $"{spine.Name}_{timestamp}_{args.FPS:f0}_{args.CRF}.mp4";
                var savePath = Path.Combine(args.OutputDir ?? spine.AssetsDir, filename);

                var videoFramesSource = new RawVideoPipeSource(GetFrames(spine, worker)) { FrameRate = args.FPS };
                try
                {
                    var ffmpegArgs = FFMpegArguments
                    .FromPipeInput(videoFramesSource)
                    .OutputToFile(savePath, true, options => options
                    .ForceFormat("mp4")
                    .WithVideoCodec(args.Codec)
                    .WithConstantRateFactor(args.CRF)
                    .WithFastStart());

                    logger.Info("FFMpeg arguments: {}", ffmpegArgs.Arguments);
                    ffmpegArgs.ProcessSynchronously();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    logger.Error("Failed to export mp4 {} {}", savePath, spine.SkelPath);
                }
            }
        }
    }
}
