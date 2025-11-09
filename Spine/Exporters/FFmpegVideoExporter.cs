using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using NLog;
using SFML.Graphics;
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
    /// 基于 FFmpeg 命令行的导出类, 可以导出动图及视频格式
    /// </summary>
    public class FFmpegVideoExporter : VideoExporter
    {
        public FFmpegVideoExporter(uint width = 100, uint height = 100) : base(width, height) { }
        public FFmpegVideoExporter(Vector2u resolution) : base(resolution) { }

        /// <summary>
        /// FFmpeg 导出格式
        /// </summary>
        public enum VideoFormat
        {
            Gif,
            Webp,
            Apng,
            Mp4,
            Webm,
            Mkv,
            Mov,
        }

        /// <summary>
        /// Apng 格式预测器算法
        /// </summary>
        public enum ApngPredMethod
        {
            None = 0,
            Sub = 1,
            Up = 2,
            Avg = 3,
            Paeth = 4,
            Mixed = 5,
        }

        /// <summary>
        /// Mov prores_ks 编码器 profile 参数
        /// </summary>
        public enum MovProfile
        {
            Auto = -1,
            Proxy = 0,
            Light = 1,
            Standard = 2,
            High = 3,
            Yuv4444 = 4,
            Yuv4444Extreme = 5,
        }

        /// <summary>
        /// 视频格式
        /// </summary>
        public VideoFormat Format { get => _format; set => _format = value; }
        private VideoFormat _format = VideoFormat.Mp4;

        /// <summary>
        /// [Gif/Webp/Apng] 动图是否循环
        /// </summary>
        public bool Loop { get => _loop; set => _loop = value; }
        private bool _loop = true;

        /// <summary>
        /// [Webp] 质量
        /// </summary>
        public int Quality { get => _quality; set => _quality = Math.Clamp(value, 0, 100); }
        private int _quality = 75;

        /// <summary>
        /// [Webp] 无损压缩
        /// </summary>
        public bool Lossless { get => _lossless; set => _lossless = value; }
        private bool _lossless = false;

        /// <summary>
        /// [Apng] 预测器算法
        /// </summary>
        public ApngPredMethod PredMethod { get => _predMethod; set => _predMethod = value; }
        private ApngPredMethod _predMethod = ApngPredMethod.Mixed;

        /// <summary>
        /// [Mp4/Webm/Mkv] CRF
        /// </summary>
        public int Crf { get => _crf; set => _crf = Math.Clamp(value, 0, 63); }
        private int _crf = 23;

        /// <summary>
        /// [Mov] prores_ks 编码器的配置等级, 越高质量越好, 只有 <see cref="MovProfile.Yuv4444"> 及以上才有透明通道
        /// </summary>
        public MovProfile Profile { get => _profile; set => _profile = value; }
        private MovProfile _profile = MovProfile.Yuv4444Extreme;

        /// <summary>
        /// 获取的一帧, 结果是预乘的
        /// </summary>
        protected override SFMLImageVideoFrame GetFrame(SpineObject[] spines)
        {
            // BUG: 也许和 SFML 多线程或者 FFmpeg 调用有关, 当渲染线程也在运行的时候此处并行渲染会导致和 SFML 有关的内容都卡死
            // 不知道为什么用 FFmpeg 必须临时创建 RenderTexture, 否则无法正常渲染, 会导致画面帧丢失
            using var tex = new RenderTexture(_renderTexture.Size.X, _renderTexture.Size.Y);
            using var view = _renderTexture.GetView();
            tex.SetView(view);
            tex.Clear(_backgroundColorPma);
            foreach (var sp in spines.Reverse()) tex.Draw(sp);
            tex.Display();
            return new(tex.Texture.CopyToImage());
        }

        public override void Export(string output, CancellationToken ct, params SpineObject[] spines)
        {
            var videoFramesSource = new RawVideoPipeSource(GetFrames(spines, output, ct)) { FrameRate = _fps };
            Action<FFMpegArgumentOptions> setOutputOptions = _format switch
            {
                VideoFormat.Gif => SetGifOptions,
                VideoFormat.Webp => SetWebpOptions,
                VideoFormat.Apng => SetApngOptions,
                VideoFormat.Mp4 => SetMp4Options,
                VideoFormat.Webm => SetWebmOptions,
                VideoFormat.Mkv => SetMkvOptions,
                VideoFormat.Mov => SetMovOptions,
                _ => throw new NotImplementedException(),
            };

            try
            {
                var ffmpegArgs = FFMpegArguments.FromPipeInput(videoFramesSource).OutputToFile(output, true, setOutputOptions);

                _logger.Info("FFmpeg arguments: {0}", ffmpegArgs.Arguments);
                ffmpegArgs.ProcessSynchronously();
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to export {0} {1}, {2}", _format, output, ex.Message);
            }
        }

        private void SetGifOptions(FFMpegArgumentOptions options)
        {
            // Gif 固定使用 256 调色板和 128 透明度阈值
            var v = "split [s0][s1]";
            var s0 = "[s0] palettegen=max_colors=256 [p]";
            var s1 = "[s1][p] paletteuse=alpha_threshold=128";
            var customArgs = $"-vf \"unpremultiply=inplace=1, {v};{s0};{s1}\" -loop {(_loop ? 0 : -1)}";
            options.ForceFormat("gif")
                .WithCustomArgument(customArgs);
        }

        private void SetWebpOptions(FFMpegArgumentOptions options)
        {
            var customArgs = $"-vf unpremultiply=inplace=1 -quality {_quality} -loop {(_loop ? 0 : 1)} -lossless {(_lossless ? 1 : 0)}";
            options.ForceFormat("webp").WithVideoCodec("libwebp_anim").ForcePixelFormat("yuva420p")
                .WithCustomArgument(customArgs);
        }

        private void SetApngOptions(FFMpegArgumentOptions options)
        {
            var customArgs = $"-vf unpremultiply=inplace=1 -plays {(_loop ? 0 : 1)} -pred {(int)_predMethod}";
            options.ForceFormat("apng").WithVideoCodec("apng").ForcePixelFormat("rgba")
                .WithCustomArgument(customArgs);
        }

        private void SetMp4Options(FFMpegArgumentOptions options)
        {
            // XXX: windows 默认播放器在播放 MP4 格式时对于 libx264 编码器只支持 yuv420p 的像素格式
            // 但是如果是 libx265 则没有该限制
            var customArgs = "-vf unpremultiply=inplace=1";
            options.ForceFormat("mp4").WithVideoCodec("libx264").ForcePixelFormat("yuv420p")
                .WithFastStart()
                .WithConstantRateFactor(_crf)
                .WithCustomArgument(customArgs);
        }

        private void SetWebmOptions(FFMpegArgumentOptions options)
        {
            var customArgs = "-vf unpremultiply=inplace=1";
            options.ForceFormat("webm").WithVideoCodec("libvpx-vp9").ForcePixelFormat("yuva420p")
                .WithConstantRateFactor(_crf)
                .WithCustomArgument(customArgs);
        }

        private void SetMkvOptions(FFMpegArgumentOptions options)
        {
            var customArgs = "-vf unpremultiply=inplace=1";
            options.ForceFormat("matroska").WithVideoCodec("libx265").ForcePixelFormat("yuv444p")
                .WithConstantRateFactor(_crf)
                .WithCustomArgument(customArgs);
        }

        private void SetMovOptions(FFMpegArgumentOptions options)
        {
            var customArgs = "-vf unpremultiply=inplace=1";
            options.ForceFormat("mov").WithVideoCodec("prores_ks").ForcePixelFormat("yuva444p10le")
                .WithFastStart()
                .WithCustomArgument($"-profile {(int)_profile}")
                .WithCustomArgument(customArgs);
        }
    }
}
