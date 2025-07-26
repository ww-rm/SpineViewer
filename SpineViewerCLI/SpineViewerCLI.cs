using System.Globalization;
using System.IO;
using SFML.Graphics;
using SFML.System;
using Spine;
using Spine.Exporters;

namespace SpineViewerCLI
{
    public class CLI
    {
        const string USAGE = @"
usage: SpineViewerCLI.exe [--skel PATH] [--atlas PATH] [--output PATH] [--animation STR] [--pma] [--fps INT] [--loop] [--crf INT] [--width INT] [--height INT] [--centerx INT] [--centery INT] [--zoom FLOAT] [--speed FLOAT] [--color HEX] [--quiet]

options:
  --skel PATH           Path to the .skel file
  --atlas PATH          Path to the .atlas file, default searches in the skel file directory
  --output PATH         Output file path
  --animation STR       Animation name
  --pma                 Use premultiplied alpha, default false
  --fps INT             Frames per second, default 24
  --loop                Whether to loop the animation, default false
  --crf INT             Constant Rate Factor i.e. video quality, from 0 (lossless) to 51 (worst), default 23
  --width INT           Output width, default 512
  --height INT          Output height, default 512
  --centerx INT         Center X offset, default automatically finds bounds
  --centery INT         Center Y offset, default automatically finds bounds
  --zoom FLOAT          Zoom level, default 1.0
  --speed FLOAT         Speed of animation, default 1.0
  --color HEX           Background color as a hex RGBA color, default 000000ff (opaque black)
  --quiet               Removes console progress log, default false
";

        public static void Main(string[] args)
        {
            string? skelPath = null;
            string? atlasPath = null;
            string? output = null;
            string? animation = null;
            bool pma = false;
            uint fps = 24;
            bool loop = false;
            int crf = 23;
            uint? width = null;
            uint? height = null;
            int? centerx = null;
            int? centery = null;
            float zoom = 1;
            float speed = 1;
            Color backgroundColor = Color.Black;
            bool quiet = false;

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
            if (!Enum.TryParse<FFmpegVideoExporter.VideoFormat>(Path.GetExtension(output).TrimStart('.'), true, out var videoFormat))
            {
                var validExtensions = string.Join(", ", Enum.GetNames(typeof(FFmpegVideoExporter.VideoFormat)));
                Console.Error.WriteLine($"Invalid output extension. Supported formats are: {validExtensions}");
                Environment.Exit(2);
            }

            var sp = new SpineObject(skelPath, atlasPath);
            sp.UsePma = pma;

            if (string.IsNullOrEmpty(animation))
            {
                var availableAnimations = string.Join(", ", sp.Data.Animations);
                Console.Error.WriteLine($"Missing --animation. Available animations for {sp.Name}: {availableAnimations}");
                Environment.Exit(2);
            }
            var trackEntry = sp.AnimationState.SetAnimation(0, animation, loop);
            sp.Update(0);

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
                Console.WriteLine();

            Environment.Exit(0);
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