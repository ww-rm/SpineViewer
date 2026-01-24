using Spine;
using Spine.Exporters;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.ViewModels.MainWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpineViewer.ViewModels.Exporters
{
    public class PsdExporterViewModel : BaseExporterViewModel
    {
        public PsdExporterViewModel(MainWindowViewModel vmMain) : base(vmMain)
        {
            // PSD 文件背景颜色固定为透明, 且不允许用户在面板修改
            _backgroundColor = Color.FromArgb(0, 0, 0, 0);
        }

        protected override void Export(SpineObjectModel[] models)
        {
            base.Export(models);
            if (!DialogService.ShowPsdExporterDialog(this)) return;
            SpineObject[] spines = models.Select(m => m.GetSpineObject(true)).ToArray();
            ProgressService.RunAsync((pr, ct) => ExportTask(spines, pr, ct), AppResource.Str_PsdExporterTitle);
            foreach (var sp in spines) sp.Dispose();
        }

        private void ExportTask(SpineObject[] spines, IProgressReporter pr, CancellationToken ct)
        {
            if (spines.Length <= 0) return;

            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            using var exporter = new PsdExporter(_renderer.Resolution.X + _margin * 2, _renderer.Resolution.Y + _margin * 2);

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
                var filename = $"layers_{timestamp}_{Guid.NewGuid().ToString()[..6]}.psd";
                var output = Path.Combine(_outputDir!, filename);

                if (_autoResolution) SetAutoResolutionStatic(exporter, spines);

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
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to export {0}, {1}", output, ex.Message);
                }
                _vmMain.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }
            else
            {
                pr.Total = spines.Select(sp => sp.IterDrawCount).Sum();
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

                    var filename = $"{sp.Name}_{timestamp}_{Guid.NewGuid().ToString()[..6]}.psd";
                    var output = Path.Combine(_outputDir ?? sp.AssetsDir, filename);

                    if (_autoResolution) SetAutoResolutionStatic(exporter, sp);

                    try
                    {
                        exporter.Export(output, ct, sp);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex.ToString());
                        _logger.Error("Failed to export {0}, {1}", output, ex.Message);
                    }
                }
                _vmMain.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }
        }
    }
}
