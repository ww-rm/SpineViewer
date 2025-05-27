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

        private string FormatSuffix => $".{_format.ToString().ToLower()}";

        public override string? Validate()
        {
            if (base.Validate() is string err) 
                return err;
            if (string.IsNullOrWhiteSpace(_format)) 
                return AppResource.Str_FFmpegFormatRequired;
            return null;
        }

        protected override void Export_Execute(IList? args)
        {
            if (args is null || args.Count <= 0) return;
            if (!ExporterDialogService.ShowCustomFFmpegExporterDialog(this)) return;
            SpineObject[] spines = args.Cast<SpineObjectModel>().Select(m => m.GetSpineObject()).ToArray();
            ProgressService.RunAsync((pr, ct) => ExportTask(spines, pr, ct), AppResource.Str_CustomFFmpegExporterTitle);
            foreach (var sp in spines) sp.Dispose();
        }

        private void ExportTask(SpineObject[] spines, IProgressReporter pr, CancellationToken ct)
        {
            if (spines.Length <= 0) return;

            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            using var exporter = new CustomFFmpegExporter(_renderer.Resolution.X + _margin * 2, _renderer.Resolution.Y + _margin * 2)
            {
                BackgroundColor = new(_backgroundColor.R, _backgroundColor.G, _backgroundColor.B, _backgroundColor.A),
                Duration = _duration,
                Fps = _fps,
                KeepLast = _keepLast,
                Format = _format,
                Codec = _codec,
                PixelFormat = _pixelFormat,
                Bitrate = _bitrate,
                Filter = _filter,
                CustomArgs = _customArgs
            };

            // 非自动分辨率则直接用预览画面的视区参数
            if (!_autoResolution)
            {
                using var view = _renderer.GetView();
                var bounds = view.GetBounds().GetCanvasBounds(_renderer.Resolution, _margin);
                exporter.Size = bounds.Size;
                exporter.Center = bounds.Position + bounds.Size / 2;
                exporter.Rotation = view.Rotation;
            }

            if (_exportSingle)
            {
                var filename = $"ffmpeg_{timestamp}_{Guid.NewGuid().ToString()[..6]}_{_fps}{FormatSuffix}";
                var output = Path.Combine(_outputDir!, filename);

                if (_autoResolution) SetAutoResolutionAnimated(exporter, spines);

                exporter.ProgressReporter = (total, done, text) =>
                {
                    pr.Total = total;
                    pr.Done = done;
                    pr.ProgressText = text;
                    _vmMain.ProgressValue = pr.Done / pr.Total;
                };

                _vmMain.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                _vmMain.ProgressValue = 0;
                try
                {
                    exporter.Export(output, ct, spines);
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to export {0}, {1}", output, ex.Message);
                }
                _vmMain.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }
            else
            {
                // 统计总帧数
                int totalFrameCount = 0;
                if (_duration > 0)
                {
                    exporter.Duration = _duration;
                    totalFrameCount = exporter.GetFrameCount() * spines.Length;
                }
                else
                {
                    foreach (var sp in spines)
                    {
                        exporter.Duration = sp.GetAnimationMaxDuration();
                        totalFrameCount += exporter.GetFrameCount();
                    }
                }

                pr.Total = totalFrameCount;
                pr.Done = 0;

                exporter.ProgressReporter = (total, done, text) =>
                {
                    pr.Done++;
                    pr.ProgressText = text;
                    _vmMain.ProgressValue = pr.Done / pr.Total;
                };

                _vmMain.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                _vmMain.ProgressValue = 0;
                foreach (var sp in spines)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.Info("Export cancelled");
                        break;
                    }

                    if (_autoResolution) SetAutoResolutionAnimated(exporter, sp);
                    if (_duration <= 0) exporter.Duration = sp.GetAnimationMaxDuration();

                    var filename = $"{sp.Name}_{timestamp}_{Guid.NewGuid().ToString()[..6]}_{_fps}{FormatSuffix}";
                    var output = Path.Combine(_outputDir ?? sp.AssetsDir, filename);

                    try
                    {
                        exporter.Export(output, ct, sp);
                    }
                    catch (Exception ex)
                    {
                        _logger.Trace(ex.ToString());
                        _logger.Error("Failed to export {0}, {1}", output, ex.Message);
                    }
                }
                _vmMain.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }
        }
    }
}
