using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Exporters
{
    public class FrameExporterViewModel(MainWindowViewModel vmMain) : BaseExporterViewModel(vmMain)
    {
        public ImmutableArray<SKEncodedImageFormat> FrameFormats { get; } = Enum.GetValues<SKEncodedImageFormat>().ToImmutableArray();

        public SKEncodedImageFormat Format { get => _format; set => SetProperty(ref _format, value); }
        protected SKEncodedImageFormat _format = SKEncodedImageFormat.Png;

        public int Quality { get => _quality; set => SetProperty(ref _quality, Math.Clamp(value, 0, 100)); }
        protected int _quality = 80;

        private string FormatSuffix
        {
            get
            {
                if (_format == SKEncodedImageFormat.Heif) return ".jpeg";
                else if (_format == SKEncodedImageFormat.Jpegxl) return ".jpeg";
                else return $".{_format.ToString().ToLower()}";
            }
        }

        protected override void Export_Execute(IList? args)
        {
            if (args is null || args.Count <= 0) return;
            if (!ExporterDialogService.ShowFrameExporterDialog(this)) return;
            SpineObject[] spines = args.Cast<SpineObjectModel>().Select(m => m.GetSpineObject(true)).ToArray();
            ProgressService.RunAsync((pr, ct) => ExportTask(spines, pr, ct), AppResource.Str_FrameExporterTitle);
            foreach (var sp in spines) sp.Dispose();
        }

        private void ExportTask(SpineObject[] spines, IProgressReporter pr, CancellationToken ct)
        {
            if (spines.Length <= 0) return;

            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            using var exporter = new FrameExporter(_renderer.Resolution.X + _margin * 2, _renderer.Resolution.Y + _margin * 2)
            {
                BackgroundColor = new(_backgroundColor.R, _backgroundColor.G, _backgroundColor.B, _backgroundColor.A),
                Format = _format,
                Quality = _quality
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
                var filename = $"frame_{timestamp}_{Guid.NewGuid().ToString()[..6]}_{_quality}{FormatSuffix}";
                var output = Path.Combine(_outputDir!, filename);

                if (_autoResolution) SetAutoResolutionStatic(exporter, spines);

                try
                {
                    exporter.Export(output, spines);
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to export {0}, {1}", output, ex.Message);
                }
            }
            else
            {
                int total = spines.Length;
                int done = 1;

                pr.Total = total;

                _vmMain.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                _vmMain.ProgressValue = 0;
                foreach (var sp in spines)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.Info("Export cancelled");
                        break;
                    }

                    var filename = $"{sp.Name}_{timestamp}_{Guid.NewGuid().ToString()[..6]}_{_quality}{FormatSuffix}";
                    var output = Path.Combine(_outputDir ?? sp.AssetsDir, filename);

                    pr.Done = done;
                    pr.ProgressText = $"[{done}/{total}] {output}";
                    _vmMain.ProgressValue = pr.Done / pr.Total;

                    if (_autoResolution) SetAutoResolutionStatic(exporter, sp);

                    try
                    {
                        exporter.Export(output, sp);
                    }
                    catch (Exception ex)
                    {
                        _logger.Trace(ex.ToString());
                        _logger.Error("Failed to export {0}, {1}", output, ex.Message);
                    }
                    done++;
                }
                _vmMain.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }
        }
    }
}
