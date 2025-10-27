using NLog;
using Spectre.Console;
using Spine;
using Spine.Exporters;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewerCLI
{
    public class PreviewCommand : Command
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly string _name = "preview";
        private static readonly string _desc = "Preview a model";
        private static readonly int MaxResolution = 1024;

        public Argument<FileInfo> ArgSkel { get; } = new("skel")
        {
            Description = "Path of skel file.",
        };

        public Option<FileInfo> OptAtlas { get; } = new("--atlas")
        {
            Description = "Path to the atlas file that matches the skel file.",
        };

        public Option<bool> OptPma { get; } = new("--pma")
        {
            Description = "Specifies whether the texture uses PMA (premultiplied alpha) format.",
        };

        public Option<string[]> OptSkins { get; } = new("--skins")
        {
            Description = "Skins to enable. Multiple skins can be specified.",
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        public Option<string[]> OptAnimations { get; } = new("--animations")
        {
            Description = "Animations to export. Supports multiple entries, placed in order on tracks starting from 0.",
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        public Option<float> OptTime { get; } = new("--time")
        {
            Description = "Start time offset of the animation.",
            DefaultValueFactory = _ => 0f,
        };

        public PreviewCommand() : base(_name, _desc)
        {
            OptTime.Validators.Add(r =>
            {
                if (r.Tokens.Count > 0 && float.TryParse(r.Tokens[0].Value, out var v) && v < 0)
                    r.AddError($"{OptTime.Name} must be non-negative.");
            });

            this.AddArgsAndOpts();
            SetAction(PreviewAction);
        }

        private void PreviewAction(ParseResult result)
        {
            // 读取模型
            using var spine = new SpineObject(result.GetValue(ArgSkel)!.FullName, result.GetValue(OptAtlas)?.FullName);

            spine.UsePma = result.GetValue(OptPma);

            // 设置要导出的动画
            int trackIdx = 0;
            foreach (var name in result.GetValue(OptAnimations))
            {
                if (!spine.Data.AnimationsByName.ContainsKey(name))
                {
                    _logger.Warn("No animation named '{0}', skip it", name);
                    continue;
                }
                spine.AnimationState.SetAnimation(trackIdx, name, true);
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

            // 设置时间偏移量
            spine.Update(result.GetValue(OptTime));

            using var exporter = GetExporterFilledWithArgs(result, spine);
            using var skImage = exporter.ExportMemoryImage(spine);
            var img = new CanvasImageAscii(skImage);
            AnsiConsole.Write(img);
        }

        private FrameExporter GetExporterFilledWithArgs(ParseResult result, SpineObject spine)
        {
            // 根据模型获取自动分辨率和视区参数
            var bounds = spine.GetCurrentBounds();
            var resolution = new SFML.System.Vector2u((uint)bounds.Size.X, (uint)bounds.Size.Y);
            if (resolution.X >= MaxResolution || resolution.Y >= MaxResolution)
            {
                // 缩小到最大像素限制
                var scale = Math.Min(MaxResolution / bounds.Width, MaxResolution / bounds.Height);
                resolution.X = (uint)(bounds.Width * scale);
                resolution.Y = (uint)(bounds.Height * scale);
            }
            var viewBounds = bounds.GetCanvasBounds(resolution);

            return new FrameExporter(resolution)
            {
                Size = new(viewBounds.Width, -viewBounds.Height),
                Center = viewBounds.Position + viewBounds.Size / 2,
                Rotation = 0,
                BackgroundColor = SFML.Graphics.Color.Transparent,

                Format = SkiaSharp.SKEncodedImageFormat.Png,
                Quality = 100,
            };
        }
    }
}
