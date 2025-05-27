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

namespace SpineViewer.ViewModels.Exporters
{
    public class FFmpegVideoExporterViewModel(MainWindowViewModel vmMain) : VideoExporterViewModel(vmMain)
    {
        public ImmutableArray<FFmpegVideoExporter.VideoFormat> VideoFormats { get; } = Enum.GetValues<FFmpegVideoExporter.VideoFormat>().ToImmutableArray();

        public FFmpegVideoExporter.VideoFormat Format { get => _format; set => SetProperty(ref _format, value); }
        protected FFmpegVideoExporter.VideoFormat _format = FFmpegVideoExporter.VideoFormat.Mp4;

        public bool Loop { get => _loop; set => SetProperty(ref _loop, value); }
        protected bool _loop = true;

        public int Quality { get => _quality; set => SetProperty(ref _quality, Math.Clamp(value, 0, 100)); }
        protected int _quality = 75;

        public int Crf { get => _crf; set => SetProperty(ref _crf, Math.Clamp(value, 0, 63)); }
        protected int _crf = 23;

        private string FormatSuffix => $".{_format.ToString().ToLower()}";

        protected override void Export_Execute(IList? args)
        {
            if (args is null || args.Count <= 0) return;
            if (!ExporterDialogService.ShowFFmpegVideoExporterDialog(this)) return;
            SpineObject[] spines = args.Cast<SpineObjectModel>().Select(m => m.GetSpineObject()).ToArray();
            ProgressService.RunAsync((pr, ct) => ExportTask(spines, pr, ct), AppResource.Str_FFmpegVideoExporterTitle);
            foreach (var sp in spines) sp.Dispose();
        }

        private void ExportTask(SpineObject[] spines, IProgressReporter pr, CancellationToken ct)
        {
            if (spines.Length <= 0) return;

            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            using var exporter = new FFmpegVideoExporter(_renderer.Resolution.X + _margin * 2, _renderer.Resolution.Y + _margin * 2)
            {
                BackgroundColor = new(_backgroundColor.R, _backgroundColor.G, _backgroundColor.B, _backgroundColor.A),
                Duration = _duration,
                Fps = _fps,
                KeepLast = _keepLast,
                Format = _format,
                Loop = _loop,
                Quality = _quality,
                Crf = _crf
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
                var filename = $"video_{timestamp}_{Guid.NewGuid().ToString()[..6]}_{_fps}{FormatSuffix}";
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
