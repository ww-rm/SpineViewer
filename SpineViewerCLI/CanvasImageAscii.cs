using SkiaSharp;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewerCLI
{
    internal class CanvasImageAscii : Renderable
    {
        private static readonly SKSamplingOptions _defaultSamplingOptions = new(new SKCubicResampler());

        /// <summary>
        /// Gets the image width.
        /// </summary>
        public int Width => Image.Width;

        /// <summary>
        /// Gets the image height.
        /// </summary>
        public int Height => Image.Height;

        /// <summary>
        /// Gets or sets the render width of the canvas.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the render width of the canvas.
        /// </summary>
        public int PixelWidth { get; set; } = 2;

        /// <summary>
        /// Gets or sets the <see cref="SKSamplingOptions"/> that should
        /// be used when scaling the image. Defaults to bicubic sampling.
        /// </summary>
        public SKSamplingOptions? SamplingOptions { get; set; }

        internal SKBitmap Image { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImageAscii"/> class.
        /// </summary>
        /// <param name="filename">The image filename.</param>
        public CanvasImageAscii(string filename)
        {
            Image = SKBitmap.Decode(filename);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImageAscii"/> class.
        /// </summary>
        /// <param name="data">Buffer containing an image.</param>
        public CanvasImageAscii(ReadOnlySpan<byte> data)
        {
            Image = SKBitmap.Decode(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImageAscii"/> class.
        /// </summary>
        /// <param name="data">Stream containing an image.</param>
        public CanvasImageAscii(Stream data)
        {
            Image = SKBitmap.Decode(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasImageAscii"/> class.
        /// </summary>
        /// <param name="image">The <see cref="SKImage"/> object.</param>
        public CanvasImageAscii(SKImage image)
        {
            Image = SKBitmap.FromImage(image);
        }

        /// <inheritdoc/>
        protected override Measurement Measure(RenderOptions options, int maxWidth)
        {
            if (PixelWidth < 0)
            {
                throw new InvalidOperationException("Pixel width must be greater than zero.");
            }

            var width = MaxWidth ?? Width;
            if (maxWidth < width * PixelWidth)
            {
                return new Measurement(maxWidth, maxWidth);
            }

            return new Measurement(width * PixelWidth, width * PixelWidth);
        }

        /// <inheritdoc/>
        protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            var image = Image;

            var width = Width;
            var height = Height;

            // Got a max width?
            if (MaxWidth != null)
            {
                height = (int)(height * ((float)MaxWidth.Value) / Width);
                width = MaxWidth.Value;
            }

            // Exceed the max width when we take pixel width into account?
            if (width * PixelWidth > maxWidth)
            {
                height = (int)(height * (maxWidth / (float)(width * PixelWidth)));
                width = maxWidth / PixelWidth;
            }

            // Need to rescale the pixel buffer?
            if (width != Width || height != Height)
            {
                var samplingOptions = SamplingOptions ?? _defaultSamplingOptions;
                image = image.Resize(new SKSizeI(width, height), samplingOptions);
            }

            var canvas = new CanvasAscii(width, height)
            {
                MaxWidth = MaxWidth,
                PixelWidth = PixelWidth,
                Scale = false,
            };

            // XXX: 也许是 SkiaSharp@3.119.0 的 bug, 此处像素值一定是非预乘的格式
            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var p = image.GetPixel(x, y);
                    if (p.Alpha == 0) continue;
                    float a = p.Alpha / 255f;
                    byte r = (byte)(p.Red * a);
                    byte g = (byte)(p.Green * a);
                    byte b = (byte)(p.Blue * a);
                    canvas.SetPixel(x, y, new(r, g, b));
                }
            }

            return ((IRenderable)canvas).Render(options, maxWidth);
        }
    }
}
