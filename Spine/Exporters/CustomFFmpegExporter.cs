using FFMpegCore;
using FFMpegCore.Pipes;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Exporters
{
    /// <summary>
    /// 自定义参数的 FFmpeg 导出类
    /// </summary>
    public class CustomFFmpegExporter : VideoExporter
    {
        public CustomFFmpegExporter(uint width = 100, uint height = 100) : base(width, height) { }
        public CustomFFmpegExporter(Vector2u resolution) : base(resolution) { }

        /// <summary>
        /// <c>-f</c>
        /// </summary>
        public string? Format { get => _format; set => _format = value; }
        private string? _format;

        /// <summary>
        /// <c>-c:v</c>
        /// </summary>
        public string? Codec { get => _codec; set => _codec = value; }
        private string? _codec;

        /// <summary>
        /// <c>-pix_fmt</c>
        /// </summary>
        public string? PixelFormat { get => _pixelFormat; set => _pixelFormat = value; }
        private string? _pixelFormat;

        /// <summary>
        /// <c>-b:v</c>
        /// </summary>
        public string? Bitrate { get => _bitrate; set => _bitrate = value; }
        private string? _bitrate;

        /// <summary>
        /// <c>-vf</c>
        /// </summary>
        public string? Filter { get => _filter; set => _filter = value; }
        private string? _filter;

        /// <summary>
        /// 其他自定义参数
        /// </summary>
        public string? CustomArgs { get => _customArgs; set => _customArgs = value; }
        private string? _customArgs;

        private void SetOutputOptions(FFMpegArgumentOptions options)
        {
            if (!string.IsNullOrEmpty(_format)) options.ForceFormat(_format);
            if (!string.IsNullOrEmpty(_codec)) options.WithVideoCodec(_codec);
            if (!string.IsNullOrEmpty(_pixelFormat)) options.ForcePixelFormat(_pixelFormat);
            if (!string.IsNullOrEmpty(_bitrate)) options.WithCustomArgument($"-b:v {_bitrate}");
            if (!string.IsNullOrEmpty(_filter)) options.WithCustomArgument($"-vf unpremultiply=inplace=1, {_customArgs}");
            else options.WithCustomArgument("-vf unpremultiply=inplace=1");
        }

        public override void Export(string output, CancellationToken ct, params SpineObject[] spines)
        {
            var videoFramesSource = new RawVideoPipeSource(GetFrames(spines, output, ct)) { FrameRate = _fps };
            try
            {
                var ffmpegArgs = FFMpegArguments.FromPipeInput(videoFramesSource).OutputToFile(output, true, SetOutputOptions);
                _logger.Info("FFmpeg arguments: {0}", ffmpegArgs.Arguments);
                ffmpegArgs.ProcessSynchronously();
            }
            catch (Exception ex)
            {
                _logger.Trace(ex.ToString());
                _logger.Error("Failed to export {0} {1}, {2}", _format, output, ex.Message);
            }
        }
    }
}
