using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewerCLI
{
    public class CanvasAscii : Renderable
    {
        private readonly Color?[,] _pixels;

        /// <summary>
        /// Gets the width of the canvas.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the canvas.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets or sets the render width of the canvas.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not
        /// to scale the canvas when rendering.
        /// </summary>
        public bool Scale { get; set; } = true;

        /// <summary>
        /// Gets or sets the pixel width.
        /// </summary>
        public int PixelWidth { get; set; } = 2;

        /// <summary>
        /// Gets or sets the pixel lettters, ordered by transparency.
        /// </summary>
        public string PixelLetters { get; set; } = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/|()1{}[]?-_+~<>i!lI;:,^`'."; // ".:-=+*#%@" ... "@%#*+=-:."

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasAscii"/> class.
        /// </summary>
        /// <param name="width">The canvas width.</param>
        /// <param name="height">The canvas height.</param>
        public CanvasAscii(int width, int height)
        {
            if (width < 1)
            {
                throw new ArgumentException("Must be > 1", nameof(width));
            }

            if (height < 1)
            {
                throw new ArgumentException("Must be > 1", nameof(height));
            }

            Width = width;
            Height = height;

            _pixels = new Color?[Width, Height];
        }

        /// <summary>
        /// Sets a pixel with the specified color in the canvas at the specified location.
        /// </summary>
        /// <param name="x">The X coordinate for the pixel.</param>
        /// <param name="y">The Y coordinate for the pixel.</param>
        /// <param name="color">The pixel color.</param>
        /// <returns>The same <see cref="CanvasAscii"/> instance so that multiple calls can be chained.</returns>
        public CanvasAscii SetPixel(int x, int y, Color color)
        {
            _pixels[x, y] = color;
            return this;
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
            if (PixelWidth < 0)
            {
                throw new InvalidOperationException("Pixel width must be greater than zero.");
            }

            if (PixelLetters.Length <= 0)
            {
                throw new InvalidOperationException("Pixel letters can't be empty.");
            }

            var pixels = _pixels;
            var emptyPixel = new string(' ', PixelWidth);
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

                // If it's not possible to scale the canvas sufficiently, it's too small to render.
                if (height == 0)
                {
                    yield break;
                }
            }

            // Need to rescale the pixel buffer?
            if (Scale && (width != Width || height != Height))
            {
                pixels = ScaleDown(width, height);
            }

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var color = pixels[x, y];
                    if (color.HasValue)
                    {
                        yield return new Segment(emptyPixel, new Style(background: color));
                    }
                    else
                    {
                        yield return new Segment(emptyPixel);
                    }
                }

                yield return Segment.LineBreak;
            }
        }

        private Color?[,] ScaleDown(int newWidth, int newHeight)
        {
            var buffer = new Color?[newWidth, newHeight];
            var xRatio = ((Width << 16) / newWidth) + 1;
            var yRatio = ((Height << 16) / newHeight) + 1;

            for (var i = 0; i < newHeight; i++)
            {
                for (var j = 0; j < newWidth; j++)
                {
                    buffer[j, i] = _pixels[(j * xRatio) >> 16, (i * yRatio) >> 16];
                }
            }

            return buffer;
        }

        private static float GetBrightness(Color c) => (0.299f * c.R + 0.587f * c.G + 0.114f * c.B) / 255f;

        private string GetPixelLetter(Color c)
        {
            var index = Math.Min((int)(GetBrightness(c) * PixelLetters.Length), PixelLetters.Length - 1);
            return new(PixelLetters[index], PixelWidth);
        }
    }
}
