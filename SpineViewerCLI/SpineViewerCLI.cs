using System.Globalization;
using SFML.Graphics;
using SFML.System;
using Spine;
using Spine.Exporters;
using SkiaSharp;

namespace SpineViewerCLI
{
    public class CLI
    {
        const string USAGE = @"
usage: SpineViewerCLI.exe [--skel PATH] [--atlas PATH] [--output PATH] [--animation STR] [--skin STR] [--hide-slot STR] [--pma] [--fps INT] [--loop] [--crf INT] [--time FLOAT] [--quality INT] [--width INT] [--height INT] [--centerx INT] [--centery INT] [--zoom FLOAT] [--speed FLOAT] [--color HEX] [--quiet]

options:
  --skel PATH           Path to the .skel file
  --atlas PATH          Path to the .atlas file, default searches in the skel file directory
  --output PATH         Output file path. Extension determines export type (.mp4, .webm for video; .png, .jpg for frame)
  --animation STR       Animation name
  --skin STR            Skin name to apply. Can be used multiple times to stack skins.
  --hide-slot STR       Slot name to hide. Can be used multiple times.
  --pma                 Use premultiplied alpha, default false
  --fps INT             Frames per second (for video), default 24
  --loop                Whether to loop the animation (for video), default false
  --crf INT             Constant Rate Factor (for video), from 0 (lossless) to 51 (worst), default 23
  --time FLOAT          Time in seconds to export a single frame. Providing this argument forces frame export mode.
  --quality INT         Quality for lossy image formats (jpg, webp), from 0 to 100, default 80
  --width INT           Output width, default 512
  --height INT          Output height, default 512
  --centerx INT         Center X offset, default automatically finds bounds
  --centery INT         Center Y offset, default automatically finds bounds
  --zoom FLOAT          Zoom level, default 1.0
  --speed FLOAT         Speed of animation (for video), default 1.0
  --color HEX           Background color as a hex RGBA color, default 000000ff (opaque black)
  --quiet               Removes console progress log, default false
  --warmup INT          Warm Up Physics, default 2 loops before export
";

        public static void Main(string[] args)
        {
            string? skelPath = null;
            string? atlasPath = null;
            string? output = null;
            string? animation = null;
            var skins = new List<string>();
            var hideSlots = new List<string>();
            bool pma = false;
            uint fps = 24;
            bool loop = false;
            int crf = 23;
            float? time = null;
            int quality = 80;
            uint? width = null;
            uint? height = null;
            int? centerx = null;
            int? centery = null;
            float zoom = 1;
            float speed = 1;
            Color backgroundColor = Color.Black;
            bool quiet = false;
            bool warmup = false;
            int warmUpLoops = 2;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--help":
                        Console.Write(USAGE);
                        Environment.Exit(0);
                        break;
                    case "--skel":
                        skelPath = args[++i];
                        break;
                    case "--atlas":
                        atlasPath = args[++i];
                        break;
                    case "--output":
                        output = args[++i];
                        break;
                    case "--animation":
                        animation = args[++i];
                        break;
                    case "--skin":
                        skins.Add(args[++i]);
                        break;
                    case "--hide-slot":
                        hideSlots.Add(args[++i]);
                        break;
                    case "--pma":
                        pma = true;
                        break;
                    case "--fps":
                        fps = uint.Parse(args[++i]);
                        break;
                    case "--loop":
                        loop = true;
                        break;
                    case "--crf":
                        crf = int.Parse(args[++i]);
                        break;
                    case "--time":
                        time = float.Parse(args[++i]);
                        break;
                    case "--quality":
                        quality = int.Parse(args[++i]);
                        break;
                    case "--width":
                        width = uint.Parse(args[++i]);
                        break;
                    case "--height":
                        height = uint.Parse(args[++i]);
                        break;
                    case "--centerx":
                        centerx = int.Parse(args[++i]);
                        break;
                    case "--centery":
                        centery = int.Parse(args[++i]);
                        break;
                    case "--zoom":
                        zoom = float.Parse(args[++i]);
                        break;
                    case "--speed":
                        speed = float.Parse(args[++i]);
                        break;
                    case "--color":
                        backgroundColor = new Color(uint.Parse(args[++i], NumberStyles.HexNumber));
                        break;
                    case "--quiet":
                        quiet = true;
                        break;
                    case "--warmup":
                        warmUpLoops = int.Parse(args[++i]);
                        warmup = true;
                        break;
                    default:
                        Console.Error.WriteLine($"Unknown argument: {args[i]}");
                        Environment.Exit(2);
                        break;
                }
            }

            if (string.IsNullOrEmpty(skelPath))
            {
                Console.Error.WriteLine("Missing --skel");
                Environment.Exit(2);
            }
            if (string.IsNullOrEmpty(output))
            {
                Console.Error.WriteLine("Missing --output");
                Environment.Exit(2);
            }
            var outputExtension = Path.GetExtension(output).TrimStart('.').ToLowerInvariant();

            var sp = new SpineObject(skelPath, atlasPath);
            sp.UsePma = pma;

            foreach (var skinName in skins)
            {
                if (!sp.SetSkinStatus(skinName, true))
                {
                    var availableSkins = string.Join(", ", sp.Data.Skins.Select(s => s.Name));
                    Console.Error.WriteLine($"Error: Skin '{skinName}' not found. Available skins: {availableSkins}");
                    Environment.Exit(2);
                }
            }

            if (string.IsNullOrEmpty(animation))
            {
                var availableAnimations = string.Join(", ", sp.Data.Animations.Select(a => a.Name));
                Console.Error.WriteLine($"Missing --animation. Available animations for {sp.Name}: {availableAnimations}");
                Environment.Exit(2);
            }

            var trackEntry = sp.AnimationState.SetAnimation(0, animation, loop);
            if (time.HasValue)
            {
                trackEntry.TrackTime = time.Value;
            }
            sp.Update(0);

            if (warmup)
            {
                sp.Update(trackEntry.Animation.Duration * warmUpLoops);
            }

            foreach (var slotName in hideSlots)
            {
                if (!sp.SetSlotVisible(slotName, false))
                {
                    if (!quiet) Console.WriteLine($"Warning: Slot '{slotName}' not found, cannot hide.");
                }
            }

            if (time.HasValue)
            {
                if (TryGetImageFormat(outputExtension, out var imageFormat))
                {
                    if (!quiet) Console.WriteLine($"Exporting single frame at {time.Value:F2}s to {output}...");

                    FrameExporter exporter;
                    if (width is uint w && height is uint h && centerx is int cx && centery is int cy)
                    {
                        exporter = new FrameExporter(w, h)
                        {
                            Center = (cx, cy),
                            Size = (w / zoom, -h / zoom),
                        };
                    }
                    else
                    {
                        var frameBounds = GetSpineObjectBounds(sp);
                        var bounds = GetFloatRectCanvasBounds(frameBounds, new(width ?? 512, height ?? 512));
                        exporter = new FrameExporter(width ?? (uint)Math.Ceiling(bounds.Width), height ?? (uint)Math.Ceiling(bounds.Height))
                        {
                            Center = bounds.Position + bounds.Size / 2,
                            Size = (bounds.Width, -bounds.Height),
                        };
                    }
                    exporter.Format = imageFormat;
                    exporter.Quality = quality;
                    exporter.BackgroundColor = backgroundColor;

                    exporter.Export(output, sp);

                    if (!quiet)
                        Console.WriteLine("Frame export complete.");
                }
                else
                {
                    var validImageExtensions = "png, jpg, jpeg, webp, bmp";
                    Console.Error.WriteLine($"Error: --time argument requires a valid image format extension. Supported formats are: {validImageExtensions}.");
                    Environment.Exit(2);
                }
            }
            else if (Enum.TryParse<FFmpegVideoExporter.VideoFormat>(outputExtension, true, out var videoFormat))
            {
                FFmpegVideoExporter exporter;
                if (width is uint w && height is uint h && centerx is int cx && centery is int cy)
                {
                    exporter = new FFmpegVideoExporter(w, h)
                    {
                        Center = (cx, cy),
                        Size = (w / zoom, -h / zoom),
                    };
                }
                else
                {
                    var bounds = GetFloatRectCanvasBounds(GetSpineObjectAnimationBounds(sp, fps), new(width ?? 512, height ?? 512));
                    exporter = new FFmpegVideoExporter(width ?? (uint)Math.Ceiling(bounds.Width), height ?? (uint)Math.Ceiling(bounds.Height))
                    {
                        Center = bounds.Position + bounds.Size / 2,
                        Size = (bounds.Width, -bounds.Height),
                    };
                }
                exporter.Duration = trackEntry.Animation.Duration;
                exporter.Fps = fps;
                exporter.Format = videoFormat;
                exporter.Loop = loop;
                exporter.Crf = crf;
                exporter.Speed = speed;
                exporter.BackgroundColor = backgroundColor;

                if (!quiet)
                    exporter.ProgressReporter = (total, done, text) => Console.Write($"\r{text}");

                using var cts = new CancellationTokenSource();
                exporter.Export(output, cts.Token, sp);

                if (!quiet)
                    Console.WriteLine("\nVideo export complete.");
            }
            else
            {
                var validVideoExtensions = string.Join(", ", Enum.GetNames(typeof(FFmpegVideoExporter.VideoFormat)));
                var validImageExtensions = "png, jpg, jpeg, webp, bmp";
                Console.Error.WriteLine($"Invalid output extension or missing --time for image export. Supported video formats are: {validVideoExtensions}. Supported image formats (with --time) are: {validImageExtensions}.");
                Environment.Exit(2);
            }

            Environment.Exit(0);
        }

        private static bool TryGetImageFormat(string extension, out SKEncodedImageFormat format)
        {
            switch (extension)
            {
                case "png":
                    format = SKEncodedImageFormat.Png;
                    return true;
                case "jpg":
                case "jpeg":
                    format = SKEncodedImageFormat.Jpeg;
                    return true;
                case "webp":
                    format = SKEncodedImageFormat.Webp;
                    return true;
                case "bmp":
                    format = SKEncodedImageFormat.Bmp;
                    return true;
                default:
                    format = default;
                    return false;
            }
        }

        public static SpineObject CopySpineObject(SpineObject sp)
        {
            var spineObject = new SpineObject(sp, true);
            foreach (var tr in sp.AnimationState.IterTracks().Where(t => t is not null))
            {
                var t = spineObject.AnimationState.SetAnimation(tr!.TrackIndex, tr.Animation, tr.Loop);
            }
            spineObject.Update(0);
            return spineObject;
        }

        static FloatRect GetSpineObjectBounds(SpineObject sp)
        {
            sp.Skeleton.GetBounds(out var x, out var y, out var w, out var h);
            return new(x, y, Math.Max(w, 1e-6f), Math.Max(h, 1e-6f));
        }
        static FloatRect FloatRectUnion(FloatRect a, FloatRect b)
        {
            float left = Math.Min(a.Left, b.Left);
            float top = Math.Min(a.Top, b.Top);
            float right = Math.Max(a.Left + a.Width, b.Left + b.Width);
            float bottom = Math.Max(a.Top + a.Height, b.Top + b.Height);
            return new FloatRect(left, top, right - left, bottom - top);
        }
        static FloatRect GetSpineObjectAnimationBounds(SpineObject sp, float fps = 10)
        {
            sp = CopySpineObject(sp);
            var bounds = GetSpineObjectBounds(sp);
            var maxDuration = sp.AnimationState.IterTracks().Select(t => t?.Animation.Duration ?? 0).DefaultIfEmpty(0).Max();
            sp.Update(0);
            for (float tick = 0, delta = 1 / fps; tick < maxDuration; tick += delta)
            {
                bounds = FloatRectUnion(bounds, GetSpineObjectBounds(sp));
                sp.Update(delta);
            }
            return bounds;
        }
        static FloatRect GetFloatRectCanvasBounds(FloatRect rect, Vector2u resolution)
        {
            float sizeW = rect.Width;
            float sizeH = rect.Height;
            float innerW = resolution.X;
            float innerH = resolution.Y;
            var scale = Math.Max(Math.Abs(sizeW / innerW), Math.Abs(sizeH / innerH));
            var scaleW = scale * Math.Sign(sizeW);
            var scaleH = scale * Math.Sign(sizeH);

            innerW *= scaleW;
            innerH *= scaleH;

            var x = rect.Left - (innerW - sizeW) / 2;
            var y = rect.Top - (innerH - sizeH) / 2;
            var w = resolution.X * scaleW;
            var h = resolution.Y * scaleH;
            return new(x, y, w, h);
        }
    }
}
