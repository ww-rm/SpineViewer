using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.ViewModels.MainWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Exporters
{
    public abstract class VideoExporterViewModel(MainWindowViewModel vmMain) : BaseExporterViewModel(vmMain)
    {
        public float Duration { get => _duration; set => SetProperty(ref _duration, value); }
        protected float _duration = -1;

        public uint Fps { get => _fps; set => SetProperty(ref _fps, Math.Max(1, value)); }
        protected uint _fps = 30;

        public float  Speed { get => _speed; set => SetProperty(ref _speed, Math.Clamp(value, 0.001f, 1000f)); }
        protected float _speed = 1f;

        public bool KeepLast { get => _keepLast; set => SetProperty(ref _keepLast, value); }
        protected bool _keepLast = true;

        /// <summary>
        /// 子类实现获取导出器, 由调用方管理生命周期
        /// </summary>
        protected abstract VideoExporter GetExporter();

        /// <summary>
        /// 子类实现获取输出名
        /// </summary>
        protected abstract string GetOutputName(string? prefix);

        /// <summary>
        /// 导出任务, 用于子类后台执行
        /// </summary>
        protected void ExportTask(SpineObject[] spines, IProgressReporter pr, CancellationToken ct)
        {
            if (spines.Length <= 0) return;

            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            using VideoExporter exporter = GetExporter();

            // 非自动分辨率则直接用预览画面的视区参数
            if (!_autoResolution)
            {
                using var view = _renderer.GetView();
                var bounds = view.GetBounds().GetCanvasBounds(_renderer.Resolution, _margin);
                exporter.Size = bounds.Size;
                exporter.Center = bounds.Position + bounds.Size / 2;
                exporter.Rotation = view.Rotation;
            }

            // BUG: FFmpeg 导出时对 RenderTexture 的频繁资源申请释放似乎使 SFML 库内部出现问题, 会卡死所有使用 SFML 的地方, 包括渲染线程
            // 所以临时把渲染线程停掉, 只让此处使用 SFML 资源, 这个问题或许和多个线程同时使用渲染资源有关
            _vmMain.SFMLRendererViewModel.StopRender();

            if (_exportSingle)
            {
                var outputName = GetOutputName(null);
                var output = Path.Combine(_outputDir!, outputName);

                if (_autoResolution) SetAutoResolutionAnimated(exporter, spines);

                // 如果时长是一个负数值则使用所有动画时长的最大值
                exporter.Duration = _duration < 0 ? spines.Select(sp => sp.GetAnimationMaxDuration()).DefaultIfEmpty(0).Max() : _duration;

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
                // 统计总帧数
                int totalFrameCount = 0;
                if (_duration < 0)
                {
                    foreach (var sp in spines)
                    {
                        exporter.Duration = sp.GetAnimationMaxDuration();
                        totalFrameCount += exporter.GetFrameCount();
                    }
                }
                else
                {
                    exporter.Duration = _duration;
                    totalFrameCount = exporter.GetFrameCount() * spines.Length;
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

                    // 如果时长是负数则需要每次都设置成动画的时长值, 否则前面统计帧数时已经设置过时长值
                    if (_duration < 0) exporter.Duration = sp.GetAnimationMaxDuration();

                    var outputName = GetOutputName(sp.Name);
                    var output = Path.Combine(_outputDir ?? sp.AssetsDir, outputName);

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

            _vmMain.SFMLRendererViewModel.StartRender();
        }
    }
}
