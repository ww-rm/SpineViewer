﻿using FFMpegCore.Pipes;
using FFMpegCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SpineViewer.Exporter
{
    /// <summary>
    /// 使用 FFmpeg 的视频导出器
    /// </summary>
    public abstract class FFmpegVideoExporter : VideoExporter
    {
        /// <summary>
        /// 文件格式
        /// </summary>
        public abstract string Format { get; }

        /// <summary>
        /// 文件名后缀
        /// </summary>
        public abstract string Suffix { get; }

        /// <summary>
        /// 文件名后缀
        /// </summary>
        public string CustomArgument { get; set; }

        /// <summary>
        /// 要追加在文件名末尾的信息字串, 首尾不需要提供额外分隔符
        /// </summary>
        public abstract string FileNameNoteSuffix { get; }

        /// <summary>
        /// 获取输出附加选项
        /// </summary>
        public virtual void SetOutputOptions(FFMpegArgumentOptions options) => options.ForceFormat(Format).WithCustomArgument(CustomArgument);

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

        protected override void ExportSingle(Spine.SpineObject[] spinesToRender, BackgroundWorker? worker = null)
        {
            var noteSuffix = FileNameNoteSuffix;
            if (!string.IsNullOrWhiteSpace(noteSuffix)) noteSuffix = $"_{noteSuffix}";

            var filename = $"ffmpeg_{timestamp}_{FPS:f0}{noteSuffix}{Suffix}";

            // 导出单个时必定提供输出文件夹
            var savePath = Path.Combine(OutputDir, filename);

            var videoFramesSource = new RawVideoPipeSource(GetFrames(spinesToRender, worker)) { FrameRate = FPS };
            try
            {
                var ffmpegArgs = FFMpegArguments.FromPipeInput(videoFramesSource).OutputToFile(savePath, true, SetOutputOptions);

                logger.Info("FFmpeg arguments: {}", ffmpegArgs.Arguments);
                ffmpegArgs.ProcessSynchronously();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Failed to export {} {}", Format, savePath);
            }
        }

        protected override void ExportIndividual(Spine.SpineObject[] spinesToRender, BackgroundWorker? worker = null)
        {
            var noteSuffix = FileNameNoteSuffix;
            if (!string.IsNullOrWhiteSpace(noteSuffix)) noteSuffix = $"_{noteSuffix}";

            foreach (var spine in spinesToRender)
            {
                if (worker?.CancellationPending == true) break; // 取消的日志在 GetFrames 里输出

                var filename = $"{spine.Name}_{timestamp}_{FPS:f0}{noteSuffix}{Suffix}";

                // 如果提供了输出文件夹, 则全部导出到输出文件夹, 否则导出到各自的文件夹下
                var savePath = Path.Combine(OutputDir ?? spine.AssetsDir, filename);

                var videoFramesSource = new RawVideoPipeSource(GetFrames(spine, worker)) { FrameRate = FPS };
                try
                {
                    var ffmpegArgs = FFMpegArguments
                    .FromPipeInput(videoFramesSource)
                    .OutputToFile(savePath, true, SetOutputOptions);

                    logger.Info("FFmpeg arguments: {}", ffmpegArgs.Arguments);
                    ffmpegArgs.ProcessSynchronously();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    logger.Error("Failed to export {} {} {}", Format, savePath, spine.SkelPath);
                }
            }
        }
    }
}
