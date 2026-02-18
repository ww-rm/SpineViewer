using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.ViewModels.MainWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Exporters
{
    public class FrameSequenceExporterViewModel(MainWindowViewModel vmMain) : VideoExporterViewModel(vmMain)
    {
        public int PngQuality { get => _pngQuality; set => SetProperty(ref _pngQuality, Math.Clamp(value, 0, 100)); }
        protected int _pngQuality = 85;

        protected override void Export(SpineObjectModel[] models)
        {
            base.Export(models);
            if (!DialogService.ShowFrameSequenceExporterDialog(this)) return;
            SpineObject[] spines = models.Select(m => m.GetSpineObject()).ToArray();
            ProgressService.RunAsync((pr, ct) => ExportTask(spines, pr, ct), AppResource.Str_FrameSequenceExporterTitle);
            foreach (var sp in spines) sp.Dispose();
        }

        protected override VideoExporter GetExporter()
        {
            return new FrameSequenceExporter(_renderer.Resolution.X + _margin * 2, _renderer.Resolution.Y + _margin * 2)
            {
                BackgroundColor = new(_backgroundColor.R, _backgroundColor.G, _backgroundColor.B, _backgroundColor.A),
                Fps = _fps,
                Speed = _speed,
                KeepLast = _keepLast,
                PngQuality = _pngQuality,
            };
        }

        protected override string GetOutputName(string? prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                prefix = "frames";
            return $"{prefix}_{_timestamp}_{Guid.NewGuid().ToString()[..6]}_{_fps}";
        }
    }
}
