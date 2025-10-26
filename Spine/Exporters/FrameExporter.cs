using SFML.System;
using SkiaSharp;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Exporters
{
    /// <summary>
    /// 单帧画面导出类
    /// </summary>
    public class FrameExporter : BaseExporter
    {
        public FrameExporter(uint width = 100, uint height = 100) : base(width, height) { }
        public FrameExporter(Vector2u resolution) : base(resolution) { }

        public SKEncodedImageFormat Format 
        { 
            get => _format; 
            set {
                switch (value)
                {
                    case SKEncodedImageFormat.Jpeg:
                    case SKEncodedImageFormat.Png:
                    case SKEncodedImageFormat.Webp:
                        _format = value;
                        break;
                    default:
                        _logger.Warn("Omit unsupported exporter format: {0}", value);
                        break;
                }
            }
        }
        protected SKEncodedImageFormat _format = SKEncodedImageFormat.Png;

        public int Quality { get => _quality; set => _quality = Math.Clamp(value, 0, 100); }
        protected int _quality = 100;

        public override void Export(string output, params SpineObject[] spines)
        {
            using var frame = GetFrame(spines);
            var info = new SKImageInfo(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var skImage = SKImage.FromPixelCopy(info, frame.Image.Pixels);
            using var data = skImage.Encode(_format, _quality);
            using var stream = File.OpenWrite(output);
            data.SaveTo(stream);
        }

        /// <summary>
        /// 获取帧图像, 结果是预乘的
        /// </summary>
        public SKImage Export(params SpineObject[] spines)
        {
            using var frame = GetFrame(spines);
            var info = new SKImageInfo(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            return SKImage.FromPixelCopy(info, frame.Image.Pixels);
        }
    }
}
