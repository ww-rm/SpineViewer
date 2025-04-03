using FFMpegCore.Pipes;
using FFMpegCore;
using SpineViewer.Exporter.Implementations.ExportArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SpineViewer.Exporter.Implementations.Exporter
{
    /// <summary>
    /// 使用 FFmpeg 的视频导出器
    /// </summary>
    [ExportImplementation(ExportType.Gif)]
    [ExportImplementation(ExportType.Mp4)]
    [ExportImplementation(ExportType.Webm)]
    [ExportImplementation(ExportType.Mkv)]
    [ExportImplementation(ExportType.Mov)]
    [ExportImplementation(ExportType.Custom)]
    public class FFmpegVideoExporter : VideoExporter
    {
        public FFmpegVideoExporter(FFmpegVideoExportArgs exportArgs) : base(exportArgs) { }

        protected override void ExportSingle(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (FFmpegVideoExportArgs)ExportArgs;
            var noteSuffix = args.FileNameNoteSuffix;
            if (!string.IsNullOrWhiteSpace(noteSuffix)) noteSuffix = $"_{noteSuffix}";

            var filename = $"{timestamp}_{args.FPS:f0}{noteSuffix}{args.Suffix}";

            // 导出单个时必定提供输出文件夹
            var savePath = Path.Combine(args.OutputDir, filename);

            var videoFramesSource = new RawVideoPipeSource(GetFrames(spinesToRender, worker)) { FrameRate = args.FPS };
            try
            {
                var ffmpegArgs = FFMpegArguments.FromPipeInput(videoFramesSource).OutputToFile(savePath, true, args.SetOutputOptions);

                logger.Info("FFmpeg arguments: {}", ffmpegArgs.Arguments);
                ffmpegArgs.ProcessSynchronously();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Failed to export {} {}", args.Format, savePath);
            }
        }

        protected override void ExportIndividual(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (FFmpegVideoExportArgs)ExportArgs;
            var noteSuffix = args.FileNameNoteSuffix;
            if (!string.IsNullOrWhiteSpace(noteSuffix)) noteSuffix = $"_{noteSuffix}";

            foreach (var spine in spinesToRender)
            {
                if (worker?.CancellationPending == true) break; // 取消的日志在 GetFrames 里输出

                var filename = $"{spine.Name}_{timestamp}_{args.FPS:f0}{noteSuffix}{args.Suffix}";

                // 如果提供了输出文件夹, 则全部导出到输出文件夹, 否则导出到各自的文件夹下
                var savePath = Path.Combine(args.OutputDir ?? spine.AssetsDir, filename);

                var videoFramesSource = new RawVideoPipeSource(GetFrames(spine, worker)) { FrameRate = args.FPS };
                try
                {
                    var ffmpegArgs = FFMpegArguments
                    .FromPipeInput(videoFramesSource)
                    .OutputToFile(savePath, true, args.SetOutputOptions);

                    logger.Info("FFmpeg arguments: {}", ffmpegArgs.Arguments);
                    ffmpegArgs.ProcessSynchronously();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    logger.Error("Failed to export {} {} {}", args.Format, savePath, spine.SkelPath);
                }
            }
        }
    }
}
