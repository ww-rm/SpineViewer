using NLog;
using SFML.System;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Exporters
{
    /// <summary>
    /// 帧序列导出器, 导出 png 帧序列
    /// </summary>
    public class FrameSequenceExporter : VideoExporter
    {
        public FrameSequenceExporter(uint width = 100, uint height = 100) : base(width, height) { }
        public FrameSequenceExporter(Vector2u resolution) : base(resolution) { }

        public override void Export(string output, CancellationToken ct, params SpineObject[] spines)
        {
            Directory.CreateDirectory(output);

            int frameCount = GetFrameCount();
            int frameIdx = 0;

            _progressReporter?.Invoke(frameCount, 0, $"[{frameIdx}/{frameCount}] {output}");
            foreach (var frame in GetFrames(spines))
            {
                if (ct.IsCancellationRequested)
                {
                    _logger.Info("Export cancelled");
                    frame.Dispose();
                    break;
                }

                var savePath = Path.Combine(output, $"frame_{_fps}_{frameIdx:d6}.png");
                var info = new SKImageInfo(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

                _progressReporter?.Invoke(frameCount, frameIdx, $"[{frameIdx + 1}/{frameCount}] {savePath}");
                try
                {
                    using var skImage = SKImage.FromPixelCopy(info, frame.Image.Pixels);
                    using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
                    using var stream = File.OpenWrite(savePath);
                    data.SaveTo(stream);
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to save frame {0}, {1}", savePath, ex.Message);
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
