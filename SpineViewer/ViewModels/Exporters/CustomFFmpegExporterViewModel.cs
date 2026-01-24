using Spine.Exporters;
using Spine;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SpineViewer.ViewModels.MainWindow;

namespace SpineViewer.ViewModels.Exporters
{
    public class CustomFFmpegExporterViewModel(MainWindowViewModel vmMain) : VideoExporterViewModel(vmMain)
    {
        public string Format { get => _format; set => SetProperty(ref _format, value); }
        protected string _format = "mp4";

        public string? Codec { get => _codec; set => SetProperty(ref _codec, value); }
        protected string? _codec;

        public string? PixelFormat { get => _pixelFormat; set => SetProperty(ref _pixelFormat, value); }
        protected string? _pixelFormat;

        public string? Bitrate { get => _bitrate; set => SetProperty(ref _bitrate, value); }
        protected string? _bitrate;

        public string? Filter { get => _filter; set => SetProperty(ref _filter, value); }
        protected string? _filter;

        public string? CustomArgs { get => _customArgs; set => SetProperty(ref _customArgs, value); }
        protected string? _customArgs;

        private string FormatSuffix => $".{_format.ToString().ToLowerInvariant()}";

        public override string? Validate()
        {
            if (base.Validate() is string err) 
                return err;
            if (string.IsNullOrWhiteSpace(_format)) 
                return AppResource.Str_FFmpegFormatRequired;
            return null;
        }

        protected override void Export(SpineObjectModel[] models)
        {
            base.Export(models);
            if (!DialogService.ShowCustomFFmpegExporterDialog(this)) return;
            SpineObject[] spines = models.Select(m => m.GetSpineObject()).ToArray();
            ProgressService.RunAsync((pr, ct) => ExportTask(spines, pr, ct), AppResource.Str_CustomFFmpegExporterTitle);
            foreach (var sp in spines) sp.Dispose();
        }

        protected override VideoExporter GetExporter()
        {
            return new CustomFFmpegExporter(_renderer.Resolution.X + _margin * 2, _renderer.Resolution.Y + _margin * 2)
            {
                BackgroundColor = new(_backgroundColor.R, _backgroundColor.G, _backgroundColor.B, _backgroundColor.A),
                Fps = _fps,
                Speed = _speed,
                KeepLast = _keepLast,
                Format = _format,
                Codec = _codec,
                PixelFormat = _pixelFormat,
                Bitrate = _bitrate,
                Filter = _filter,
                CustomArgs = _customArgs
            };
        }

        protected override string GetOutputName(string? prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                prefix = "ffmpeg";
            return $"{prefix}_{_timestamp}_{Guid.NewGuid().ToString()[..6]}_{_fps}{FormatSuffix}";
        }
    }
}
