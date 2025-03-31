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
    /// 帧序列导出器
    /// </summary>
    [ExportImplementation(ExportType.FrameSequence)]
    public class FrameSequenceExporter : VideoExporter
    {
        public FrameSequenceExporter(FrameSequenceExportArgs exportArgs) : base(exportArgs) { }

        protected override void ExportSingle(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (FrameSequenceExportArgs)ExportArgs;

            // 导出单个时必定提供输出文件夹, 
            var saveDir = Path.Combine(args.OutputDir, $"frames_{timestamp}_{args.FPS:f0}");
            Directory.CreateDirectory(saveDir);

            int frameIdx = 0;
            foreach (var frame in GetFrames(spinesToRender, worker))
            {
                var filename = $"frames_{timestamp}_{args.FPS:f0}_{frameIdx:d6}{args.Suffix}";
                var savePath = Path.Combine(saveDir, filename);

                try
                {
                    frame.SaveToFile(savePath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    logger.Error("Failed to save frame {}", savePath);
                }
                finally
                {
                    frame.Dispose();
                }
                frameIdx++;
            }
        }

        protected override void ExportIndividual(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            var args = (FrameSequenceExportArgs)ExportArgs;
            foreach (var spine in spinesToRender)
            {
                if (worker?.CancellationPending == true) break; // 取消的日志在 GetFrames 里输出

                // 如果提供了输出文件夹, 则全部导出到输出文件夹, 否则导出到各自的文件夹下
                var subDir = $"{spine.Name}_{timestamp}_{args.FPS:f0}";
                var saveDir = args.OutputDir is null ? Path.Combine(spine.AssetsDir, subDir) : Path.Combine(args.OutputDir, subDir);
                Directory.CreateDirectory(saveDir);

                int frameIdx = 0;
                foreach (var frame in GetFrames(spine, worker))
                {
                    var filename = $"{spine.Name}_{timestamp}_{args.FPS:f0}_{frameIdx:d6}{args.Suffix}";
                    var savePath = Path.Combine(saveDir, filename);

                    try
                    {
                        frame.SaveToFile(savePath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                        logger.Error("Failed to save frame {} {}", savePath, spine.SkelPath);
                    }
                    finally
                    {
                        frame.Dispose();
                    }
                    frameIdx++;
                }
            }
        }
    }
}
