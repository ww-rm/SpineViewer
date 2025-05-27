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

        public SKEncodedImageFormat Format { get => _format; set => _format = value; }
        protected SKEncodedImageFormat _format = SKEncodedImageFormat.Png;

        public int Quality { get => _quality; set => _quality = Math.Clamp(value, 0, 100); }
        protected int _quality = 80;

        public override void Export(string output, params SpineObject[] spines)
        {
            using var frame = GetFrame(spines);
            var info = new SKImageInfo(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var skImage = SKImage.FromPixelCopy(info, frame.Image.Pixels);
            using var data = skImage.Encode(_format, _quality);
            using var stream = File.OpenWrite(output);
            data.SaveTo(stream);
        }
    }
}
