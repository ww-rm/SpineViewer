using Spine.Exporters;
using Spine;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Immutable;
using SpineViewer.ViewModels.MainWindow;

namespace SpineViewer.ViewModels.Exporters
{
    public class FFmpegVideoExporterViewModel(MainWindowViewModel vmMain) : VideoExporterViewModel(vmMain)
    {
        public static ImmutableArray<FFmpegVideoExporter.VideoFormat> VideoFormatOptions { get; } = Enum.GetValues<FFmpegVideoExporter.VideoFormat>().ToImmutableArray();
        public static ImmutableArray<FFmpegVideoExporter.ApngPredMethod> ApngPredMethodOptions { get; } = Enum.GetValues<FFmpegVideoExporter.ApngPredMethod>().ToImmutableArray();
        public static ImmutableArray<FFmpegVideoExporter.MovProfile> MovProfileOptions { get; } = Enum.GetValues<FFmpegVideoExporter.MovProfile>().ToImmutableArray();

        public FFmpegVideoExporter.VideoFormat Format 
        { 
            get => _format;
            set
            {
                if (!SetProperty(ref _format, value))
                    return;
                OnPropertyChanged(nameof(EnableParamLoop));
                OnPropertyChanged(nameof(EnableParamQuality));
                OnPropertyChanged(nameof(EnableParamLossless));
                OnPropertyChanged(nameof(EnableParamApngPred));
                OnPropertyChanged(nameof(EnableParamCrf));
                OnPropertyChanged(nameof(EnableParamProfile));
            }
        }
        protected FFmpegVideoExporter.VideoFormat _format = FFmpegVideoExporter.VideoFormat.Mp4;

        public bool Loop { get => _loop; set => SetProperty(ref _loop, value); }
        protected bool _loop = true;

        public bool EnableParamLoop =>
            _format == FFmpegVideoExporter.VideoFormat.Gif ||
            _format == FFmpegVideoExporter.VideoFormat.Webp ||
            _format == FFmpegVideoExporter.VideoFormat.Apng;

        public int Quality { get => _quality; set => SetProperty(ref _quality, Math.Clamp(value, 0, 100)); }
        protected int _quality = 75;

        public bool EnableParamQuality =>
            _format == FFmpegVideoExporter.VideoFormat.Webp;

        public bool Lossless { get => _lossless; set => SetProperty(ref _lossless, value); }
        protected bool _lossless = false;

        public bool EnableParamLossless =>
            _format == FFmpegVideoExporter.VideoFormat.Webp;

        public FFmpegVideoExporter.ApngPredMethod PredMethod { get => _predMethod; set => SetProperty(ref _predMethod, value); }
        protected FFmpegVideoExporter.ApngPredMethod _predMethod = FFmpegVideoExporter.ApngPredMethod.Mixed;

        public bool EnableParamApngPred =>
            _format == FFmpegVideoExporter.VideoFormat.Apng;

        public int Crf { get => _crf; set => SetProperty(ref _crf, Math.Clamp(value, 0, 63)); }
        protected int _crf = 23;

        public bool EnableParamCrf =>
            _format == FFmpegVideoExporter.VideoFormat.Mp4 ||
            _format == FFmpegVideoExporter.VideoFormat.Webm ||
            _format == FFmpegVideoExporter.VideoFormat.Mkv;

        public FFmpegVideoExporter.MovProfile Profile { get => _profile; set => SetProperty(ref _profile, value); }
        protected FFmpegVideoExporter.MovProfile _profile = FFmpegVideoExporter.MovProfile.Yuv4444Extreme;

        public bool EnableParamProfile =>
            _format == FFmpegVideoExporter.VideoFormat.Mov;

        private string FormatSuffix => $".{_format.ToString().ToLowerInvariant()}";

        protected override void Export(SpineObjectModel[] models)
        {
            base.Export(models);
            if (!DialogService.ShowFFmpegVideoExporterDialog(this)) return;
            SpineObject[] spines = models.Select(m => m.GetSpineObject()).ToArray();
            ProgressService.RunAsync((pr, ct) => ExportTask(spines, pr, ct), AppResource.Str_FFmpegVideoExporterTitle);
            foreach (var sp in spines) sp.Dispose();
        }

        protected override VideoExporter GetExporter()
        {
            return new FFmpegVideoExporter(_renderer.Resolution.X + _margin * 2, _renderer.Resolution.Y + _margin * 2)
            {
                BackgroundColor = new(_backgroundColor.R, _backgroundColor.G, _backgroundColor.B, _backgroundColor.A),
                Fps = _fps,
                Speed = _speed,
                KeepLast = _keepLast,
                Format = _format,
                Loop = _loop,
                Quality = _quality,
                Lossless = _lossless,
                PredMethod = _predMethod,
                Crf = _crf,
                Profile = _profile,
            };
        }

        protected override string GetOutputName(string? prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                prefix = "video";
            return $"{prefix}_{_timestamp}_{Guid.NewGuid().ToString()[..6]}_{_fps}{FormatSuffix}";
        }
    }
}
