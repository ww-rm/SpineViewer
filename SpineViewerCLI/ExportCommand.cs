using NLog;
using Spectre.Console;
using Spine;
using Spine.Exporters;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewerCLI
{
    public enum ExportFormat
    {
        Png = 0x0100,
        Jpg = 0x0101,
        Webp = 0x0102,
        Frames = 0x0200,
        Gif = 0x0300,
        Webpa = 0x0301,
        Apng = 0x0302,
        Mp4 = 0x0303,
        Webm = 0x0304,
        Mkv = 0x0305,
        Mov = 0x0306,
        Custom = 0x0400,
        Psd = 0x0500,
    }

    public struct FixedViewOptions
    {
        public uint Width;
        public uint Height;
        public float CenterX;
        public float CenterY;
        public float Scale = 1f;

        public FixedViewOptions() { }

        public readonly float ViewWidth { get => Width / Scale; }
        public readonly float ViewHeight { get => Height / Scale; }
    }

    public class ExportCommand : Command
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly string _name = "export";
        private static readonly string _desc = "Export single model";

        #region >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 基本参数 <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        public Argument<FileInfo> ArgSkel { get; } = new("skel")
        {
            Description = "Path of skel file.",
        };

        public Option<ExportFormat> OptFormat { get; } = new("--format", "-f")
        {
            Description = "Export format.",
            Required = true,
        };

        public Option<string> OptOutput { get; } = new("--output", "-o")
        {
            Description = "Output file or directory. Use a directory for frame sequence export.",
            Required = true,
        };

        public Option<string[]> OptAnimations { get; } = new("--animations", "-a")
        {
            Description = "Animations to export. Supports multiple entries, placed in order on tracks starting from 0.",
            Required = true,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        public Option<FileInfo> OptAtlas { get; } = new("--atlas")
        {
            Description = "Path to the atlas file that matches the skel file.",
        };

        public Option<float> OptScale { get; } = new("--scale")
        {
            Description = "Scale factor of the model.",
            DefaultValueFactory = _ => 1f,
        };

        public Option<bool> OptPma { get; } = new("--pma")
        {
            Description = "Specifies whether the texture uses PMA (premultiplied alpha) format.",
        };

        public Option<string[]> OptSkins { get; } = new("--skins")
        {
            Description = "Skins to export. Multiple skins can be specified.",
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        public Option<string[]> OptDisableSlots { get; } = new("--disable-slots")
        {
            Description = "Slots to disable during export. Multiple slots can be specified.",
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        public Option<bool> OptDisableTrackLoop { get; } = new("--disable-track-loop")
        {
            Description = "Disable track animation looping. When disabled, all track animations will play only once.",
        };

        public Option<float> OptWarmUp { get; } = new("--warm-up")
        {
            Description = "Warm-up duration used to stabilize physics effects. If negative, the warm-up duration is set to the maximum animation duration multiplied by the absolute value.",
            DefaultValueFactory = _ => 0f,
        };

        public Option<bool> OptNoProgress { get; } = new("--no-progress")
        {
            Description = "Do not display real-time progress.",
        };

        #endregion

        #region >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 基本导出参数 <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        public Option<SFML.Graphics.Color> OptColor { get; } = new("--color")
        {
            Description = "Background color of content, omitted for PSD format.",
            //DefaultValueFactory = ...
            CustomParser = Utils.ParseColor,
        };

        public Option<uint> OptMargin { get; } = new("--margin")
        {
            Description = "Size of the margin (in pixels) around the content.",
            DefaultValueFactory = _ => 0u,
        };

        public Option<uint> OptMaxResolution { get; } = new("--max-resolution")
        {
            Description = "Maximum width or height (in pixels) for exported images.",
            DefaultValueFactory = _ => 2048u,
        };

        public Option<FixedViewOptions?> OptFixedView { get; } = new("--fixed-view")
        {
            Description = "Manually set a fixed export view. Format: `arg1=value1[,arg2=value2][...]`. Each parameter is specified as `name=value` and separated by `,`. Supported parameters are `w=uint,h=uint,x=float,y=float,s=float`, representing canvas width and height (in pixels), view center coordinates, and view scale.",
            CustomParser = Utils.ParseFixedView,
        };

        public Option<float> OptTime { get; } = new("--time")
        {
            Description = "Start time offset of the animation.",
            DefaultValueFactory = _ => 0f,
        };

        public Option<float> OptDuration { get; } = new("--duration")
        {
            Description = "Export duration. Negative values indicate automatic duration calculation.",
            DefaultValueFactory = _ => -1f,
        };

        public Option<uint> OptFps { get; } = new("--fps")
        {
            Description = "Frame rate for export.",
            DefaultValueFactory = _ => 30u,
        };

        public Option<float> OptSpeed { get; } = new("--speed")
        {
            Description = "Speed factor for the exported animation.",
            DefaultValueFactory = _ => 1f,
        };

        public Option<bool> OptDropLastFrame { get; } = new("--drop-last-frame")
        {
            Description = "Whether to drop the incomplete last frame.",
        };

        #endregion

        #region >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 格式参数 <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        public Option<uint> OptQuality { get; } = new("--quality")
        {
            Description = "Image quality.",
            DefaultValueFactory = _ => 80u,
        };

        public Option<bool> OptLoop { get; } = new("--loop")
        {
            Description = "Whether the animation should loop.",
        };

        public Option<bool> OptLossless { get; } = new("--lossless")
        {
            Description = "Whether to encode the WebP animation losslessly.",
        };

        public Option<FFmpegVideoExporter.ApngPredMethod> OptApngPredMethod { get; } = new("--apng-pred")
        {
            Description = "Prediction method used for APNG animations.",
            DefaultValueFactory = _ => FFmpegVideoExporter.ApngPredMethod.Mixed,
        };

        public Option<uint> OptCrf { get; } = new("--crf")
        {
            Description = "CRF (Constant Rate Factor) value for encoding.",
            DefaultValueFactory = _ => 23u,
        };

        public Option<FFmpegVideoExporter.MovProfile> OptMovProfile { get; } = new("--mov-profile")
        {
            Description = "Profile setting for MOV format export.",
            DefaultValueFactory = _ => FFmpegVideoExporter.MovProfile.Yuv4444Extreme,
        };

        #endregion

        #region >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 自定义导出格式参数 <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        public Option<string> OptFFFormat { get; } = new("--ff-format")
        {
            Description = "format option of ffmpeg",
        };

        public Option<string> OptFFCodec { get; } = new("--ff-codec")
        {
            Description = "codec option of ffmpeg",
        };

        public Option<string> OptFFPixelFormat { get; } = new("--ff-pixfmt")
        {
            Description = "pixel format option of ffmpeg",
        };

        public Option<string> OptFFBitrate { get; } = new("--ff-bitrate")
        {
            Description = "bitrate option of ffmpeg",
        };

        public Option<string> OptFFFilter { get; } = new("--ff-filter")
        {
            Description = "filter option of ffmpeg",
        };

        public Option<string> OptFFArgs { get; } = new("--ff-args")
        {
            Description = "other arguments of ffmpeg",
        };

        #endregion

        public ExportCommand() : base(_name, _desc)
        {
            OptColor.DefaultValueFactory = r =>
            {
                var defVal = SFML.Graphics.Color.Black;
                try
                {
                    switch (r.GetValue(OptFormat))
                    {
                        case ExportFormat.Png:
                        case ExportFormat.Webp:
                        case ExportFormat.Frames:
                        case ExportFormat.Gif:
                        case ExportFormat.Webpa:
                        case ExportFormat.Apng:
                        case ExportFormat.Webm:
                        case ExportFormat.Psd:
                            defVal = SFML.Graphics.Color.Transparent;
                            break;
                    }
                }
                catch (InvalidOperationException) { } // 未提供 OptFormat 的时候 GetValue 会报错
                return defVal;
            };
            OptScale.Validators.Add(r =>
            {
                if (r.Tokens.Count > 0 && float.TryParse(r.Tokens[0].Value, out var v) && v < 0)
                    r.AddError($"{OptScale.Name} must be non-negative.");
            });
            OptTime.Validators.Add(r =>
            {
                if (r.Tokens.Count > 0 && float.TryParse(r.Tokens[0].Value, out var v) && v < 0)
                    r.AddError($"{OptTime.Name} must be non-negative.");
            });
            OptSpeed.Validators.Add(r =>
            {
                if (r.Tokens.Count > 0 && float.TryParse(r.Tokens[0].Value, out var v) && v < 0)
                    r.AddError($"{OptSpeed.Name} must be non-negative.");
            });

            this.AddArgsAndOpts();
            SetAction(ExportAction);
        }

        private void ExportAction(ParseResult result)
        {
            // 读取模型
            using var spine = new SpineObject(result.GetValue(ArgSkel)!.FullName, result.GetValue(OptAtlas)?.FullName);

            // 设置模型参数
            spine.Skeleton.ScaleX = spine.Skeleton.ScaleY = result.GetValue(OptScale);
            spine.UsePma = result.GetValue(OptPma);

            // 设置要导出的动画
            var isTrackLoop = !result.GetValue(OptDisableTrackLoop);
            int trackIdx = 0;
            foreach (var name in result.GetValue(OptAnimations))
            {
                if (!spine.Data.AnimationsByName.ContainsKey(name))
                {
                    _logger.Warn("No animation named '{0}', skip it", name);
                    continue;
                }
                spine.AnimationState.SetAnimation(trackIdx, name, isTrackLoop);
                trackIdx++;
            }

            // 设置需要启用的皮肤
            foreach (var name in result.GetValue(OptSkins))
            {
                if (!spine.SetSkinStatus(name, true))
                {
                    _logger.Warn("Failed to enable skin '{0}'", name);
                }
            }

            // 设置需要屏蔽的插槽
            foreach (var name in result.GetValue(OptDisableSlots))
            {
                if (!spine.SetSlotVisible(name, false))
                {
                    _logger.Warn("Failed to disable slot '{0}'", name);
                }
            }

            // TODO: 设置要启用的插槽

            // 时间轴处理
            var warmup = result.GetValue(OptWarmUp);
            if (warmup < 0) warmup = spine.GetAnimationMaxDuration() * -warmup;

            // 按传入的帧率进行逐帧预热, 不能直接更新整个动画时长, 否则物理效果无法预热
            for (float t = 0, step = 1f / result.GetValue(OptFps); t < warmup; t += step)
                spine.Update(Math.Min(step, warmup - t));
            spine.Update(result.GetValue(OptTime));

            using var exporter = GetExporterFilledWithArgs(result, spine);

            // 创建输出目录
            string output = Path.GetFullPath(result.GetValue(OptOutput));
            Directory.CreateDirectory(exporter is FrameSequenceExporter ? output : Path.GetDirectoryName(output));

            // 挂载进度报告函数
            if (exporter is VideoExporter ve && !result.GetValue(OptNoProgress))
            {
                AnsiConsole.Progress().Columns(
                [
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn(),
                ]).Start(ctx =>
                {
                    var task = ctx.AddTask($"Exporting '{spine.Name}'");
                    task.MaxValue = ve.GetFrameCount();
                    ve.ProgressReporter = (total, done, text) => task.Value = done;
                    ve.Export(output, spine);
                });
            }
            else
            {
                exporter.Export(output, spine);
            }

            _logger.Info($"{spine.SkelPath} export completed");
        }

        private BaseExporter GetExporterFilledWithArgs(ParseResult result, SpineObject spine)
        {
            var formatType = (int)result.GetValue(OptFormat) >> 8;

            var margin = result.GetValue(OptMargin);
            SFML.System.Vector2u resolution;
            SFML.Graphics.FloatRect viewBounds;

            if (result.GetValue(OptFixedView) is FixedViewOptions fvopts)
            {
                // 手动设置导出视区
                resolution = new(fvopts.Width, fvopts.Height);
                viewBounds = new(
                    fvopts.CenterX - fvopts.ViewWidth / 2, 
                    fvopts.CenterY - fvopts.ViewHeight / 2, 
                    fvopts.ViewWidth, 
                    fvopts.ViewHeight
                );
            }
            else
            {
                // 根据模型获取自动分辨率和视区参数
                var maxResolution = result.GetValue(OptMaxResolution);
                var bounds = formatType == 0x01 ? spine.GetCurrentBounds() : spine.GetAnimationBounds(result.GetValue(OptFps));
                resolution = new((uint)bounds.Size.X, (uint)bounds.Size.Y);
                if (resolution.X >= maxResolution || resolution.Y >= maxResolution)
                {
                    // 缩小到最大像素限制
                    var scale = Math.Min(maxResolution / bounds.Width, maxResolution / bounds.Height);
                    resolution.X = (uint)(bounds.Width * scale);
                    resolution.Y = (uint)(bounds.Height * scale);
                }
                viewBounds = bounds.GetCanvasBounds(resolution, margin);
            }

            var duration = result.GetValue(OptDuration);
            
            // 除以速度才能覆盖整数个循环, 否则 speed != 1 时接缝跳变
            if (duration < 0) 
                duration = spine.GetAnimationMaxDuration() / Math.Max(result.GetValue(OptSpeed), 1e-6f);

            if (formatType == 0x01)
            {
                return new FrameExporter(resolution.X + margin * 2, resolution.Y + margin * 2)
                {
                    Size = new(viewBounds.Width, -viewBounds.Height),
                    Center = viewBounds.Position + viewBounds.Size / 2,
                    Rotation = 0,
                    BackgroundColor = result.GetValue(OptColor),

                    Format = result.GetValue(OptFormat) switch
                    {
                        ExportFormat.Png => SkiaSharp.SKEncodedImageFormat.Png,
                        ExportFormat.Jpg => SkiaSharp.SKEncodedImageFormat.Jpeg,
                        ExportFormat.Webp => SkiaSharp.SKEncodedImageFormat.Webp,
                        var v => throw new InvalidOperationException($"{v}"),
                    },
                    Quality = (int)result.GetValue(OptQuality),
                };
            }
            else if (formatType == 0x02)
            {
                return new FrameSequenceExporter(resolution.X + margin * 2, resolution.Y + margin * 2)
                {
                    Size = new(viewBounds.Width, -viewBounds.Height),
                    Center = viewBounds.Position + viewBounds.Size / 2,
                    Rotation = 0,
                    BackgroundColor = result.GetValue(OptColor),

                    Fps = result.GetValue(OptFps),
                    Speed = result.GetValue(OptSpeed),
                    KeepLast = !result.GetValue(OptDropLastFrame),
                    Duration = duration,
                };
            }
            else if (formatType == 0x03)
            {
                return new FFmpegVideoExporter(resolution.X + margin * 2, resolution.Y + margin * 2)
                {
                    Size = new(viewBounds.Width, -viewBounds.Height),
                    Center = viewBounds.Position + viewBounds.Size / 2,
                    Rotation = 0,
                    BackgroundColor = result.GetValue(OptColor),

                    Fps = result.GetValue(OptFps),
                    Speed = result.GetValue(OptSpeed),
                    KeepLast = !result.GetValue(OptDropLastFrame),
                    Duration = duration,

                    Format = result.GetValue(OptFormat) switch
                    {
                        ExportFormat.Gif => FFmpegVideoExporter.VideoFormat.Gif,
                        ExportFormat.Webpa => FFmpegVideoExporter.VideoFormat.Webp,
                        ExportFormat.Apng => FFmpegVideoExporter.VideoFormat.Apng,
                        ExportFormat.Mp4 => FFmpegVideoExporter.VideoFormat.Mp4,
                        ExportFormat.Webm => FFmpegVideoExporter.VideoFormat.Webm,
                        ExportFormat.Mkv => FFmpegVideoExporter.VideoFormat.Mkv,
                        ExportFormat.Mov => FFmpegVideoExporter.VideoFormat.Mov,
                        var v => throw new InvalidOperationException($"{v}"),
                    },
                    Quality = (int)result.GetValue(OptQuality),
                    Loop = result.GetValue(OptLoop),
                    Lossless = result.GetValue(OptLossless),
                    PredMethod = result.GetValue(OptApngPredMethod),
                    Crf = (int)result.GetValue(OptCrf),
                    Profile = result.GetValue(OptMovProfile),
                }
                ;
            }
            else if (formatType == 0x04)
            {
                return new CustomFFmpegExporter(resolution.X + margin * 2, resolution.Y + margin * 2)
                {
                    Size = new(viewBounds.Width, -viewBounds.Height),
                    Center = viewBounds.Position + viewBounds.Size / 2,
                    Rotation = 0,
                    BackgroundColor = result.GetValue(OptColor),

                    Fps = result.GetValue(OptFps),
                    Speed = result.GetValue(OptSpeed),
                    KeepLast = !result.GetValue(OptDropLastFrame),
                    Duration = duration,

                    Format = result.GetValue(OptFFFormat),
                    Codec = result.GetValue(OptFFCodec),
                    PixelFormat = result.GetValue(OptFFPixelFormat),
                    Bitrate = result.GetValue(OptFFBitrate),
                    Filter = result.GetValue(OptFFFilter),
                    CustomArgs = result.GetValue(OptFFArgs),
                };
            }
            else if (formatType == 0x05)
            {
                return new PsdExporter(resolution.X + margin * 2, resolution.Y + margin * 2)
                {
                    Size = new(viewBounds.Width, -viewBounds.Height),
                    Center = viewBounds.Position + viewBounds.Size / 2,
                    Rotation = 0,
                    BackgroundColor = SFML.Graphics.Color.Transparent, // 固定为透明色, 且内部忽略背景色
                };
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Unknown format type {formatType}");
            }
        }
    }
}
