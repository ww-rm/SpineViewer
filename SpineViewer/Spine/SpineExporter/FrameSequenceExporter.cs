using SpineViewer.Spine;
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
    /// 帧序列导出器
    /// </summary>
    public class FrameSequenceExporter : VideoExporter
    {
        /// <summary>
        /// 文件名后缀, 同时决定帧图像格式, 支持的格式为 <c>".png", ".jpg", ".tga", ".bmp"</c>
        /// </summary>
        public string Suffix { get; set; } = ".png";

        protected override void ExportSingle(SpineObject[] spinesToRender, BackgroundWorker? worker = null)
        {
            // 导出单个时必定提供输出文件夹, 
            var saveDir = Path.Combine(OutputDir, $"frames_{timestamp}_{FPS:f0}");
            Directory.CreateDirectory(saveDir);

            int frameIdx = 0;
            foreach (var frame in GetFrames(spinesToRender, worker))
            {
                var filename = $"frames_{timestamp}_{FPS:f0}_{frameIdx:d6}{Suffix}";
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

        protected override void ExportIndividual(SpineObject[] spinesToRender, BackgroundWorker? worker = null)
        {
            foreach (var spine in spinesToRender)
            {
                if (worker?.CancellationPending == true) break; // 取消的日志在 GetFrames 里输出

                // 如果提供了输出文件夹, 则全部导出到输出文件夹, 否则导出到各自的文件夹下
                var subDir = $"{spine.Name}_{timestamp}_{FPS:f0}";
                var saveDir = Path.Combine(OutputDir ?? spine.AssetsDir, subDir);
                Directory.CreateDirectory(saveDir);

                int frameIdx = 0;
                foreach (var frame in GetFrames(spine, worker))
                {
                    var filename = $"{spine.Name}_{timestamp}_{FPS:f0}_{frameIdx:d6}{Suffix}";
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

    public class FrameSequenceExporterProperty(VideoExporter exporter) : VideoExporterProperty(exporter)
    {
        [Browsable(false)]
        public override FrameSequenceExporter Exporter => (FrameSequenceExporter)base.Exporter;

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [TypeConverter(typeof(StringEnumConverter)), StringEnumConverter.StandardValues(".png", ".jpg", ".tga", ".bmp")]
		[LocalizedCategory(typeof(Properties.Resources), "categoryFrameSequenceParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayFilenameSuffix")]
		[LocalizedDescription(typeof(Properties.Resources), "descFrameFileExtension")]
        public string Suffix { get => Exporter.Suffix; set => Exporter.Suffix = value; }
    }
}
