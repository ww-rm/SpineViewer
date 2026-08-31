using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.ViewModels.MainWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

namespace SpineViewer.ViewModels.Assets
{
    public class AssetsPreviewViewModel : ObservableObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
       
        private readonly MainWindowViewModel _vmMain;

        public AssetsPreviewViewModel(MainWindowViewModel vmMain)
        {
            _vmMain = vmMain;
        }

        /// <summary>
        /// 当前显示的预览图对象
        /// </summary>
        public ImageSource? PreviewImage { get => _previewImage; set => SetProperty(ref _previewImage, value); }
        private ImageSource? _previewImage;

        /// <summary>
        /// 预览图的保存质量
        /// </summary>
        public int PreviewQuality { get => _previewQuality; set => SetProperty(ref _previewQuality, Math.Clamp(value, 0, 100)); }
        private int _previewQuality = 80;

        /// <summary>
        /// 生成预览图边长的最大分辨率
        /// </summary>
        public uint PreviewMaxResolution { get => _previewMaxResolution; set => SetProperty(ref _previewMaxResolution, Math.Clamp(value, 16, 4096)); }
        private uint _previewMaxResolution = 1024;

        /// <summary>
        /// 生成预览图时是否使用预乘 Alpha
        /// </summary>
        public bool PreviewPma { get => _previewPma; set => SetProperty(ref _previewPma, value); }
        private bool _previewPma = false;

        /// <summary>
        /// 为资源对象生成预览图
        /// </summary>
        public void GeneratePreviews(IReadOnlyList<AssetsItemViewModel> items)
        {
            if (items.Count <= 0)
                return;

            if (items.Count <= 1)
            {
                var m = items[0];
                try
                {
                    using var sp = new SpineObject(m.LocalFullPath) { UsePma = PreviewPma };
                    var bounds = sp.GetCurrentBounds();
                    using var exporter = new FrameExporter()
                    {
                        Format = SkiaSharp.SKEncodedImageFormat.Webp,
                        Quality = PreviewQuality,
                        BackgroundColor = SFML.Graphics.Color.Transparent,
                    };
                    SetAutoResolution(exporter, bounds);
                    exporter.Export(m.PreviewFilePath, sp);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to generate preview: {0}, {1}", m.PreviewFilePath, ex.Message);
                }
                _logger.LogCurrentProcessMemoryUsage();
            }
            else
            {
                ProgressService.RunAsync(
                    (pr, ct) => GeneratePreviewsTask(items, pr, ct),
                    AppResource.Str_GeneratePreviewsTitle
                );
            }
        }

        private void GeneratePreviewsTask(IReadOnlyList<AssetsItemViewModel> items, IProgressReporter reporter, CancellationToken ct)
        {
            int totalCount = items.Count;
            int success = 0;
            int error = 0;

            _vmMain.ProgressState = TaskbarItemProgressState.Normal;
            _vmMain.ProgressValue = 0;

            reporter.Total = totalCount;
            reporter.Done = 0;
            reporter.ProgressText = $"[0/{totalCount}]";

            using var exporter = new FrameExporter()
            {
                Format = SkiaSharp.SKEncodedImageFormat.Webp,
                Quality = PreviewQuality,
                BackgroundColor = SFML.Graphics.Color.Transparent,
            };
            for (int i = 0; i < totalCount; i++)
            {
                if (ct.IsCancellationRequested) break;

                var m = items[i];
                reporter.ProgressText = $"[{i}/{totalCount}] {m.LocalFullPath}";

                try
                {
                    using var sp = new SpineObject(m.LocalFullPath) { UsePma = PreviewPma };
                    var bounds = sp.GetCurrentBounds();
                    SetAutoResolution(exporter, bounds);
                    exporter.Export(m.PreviewFilePath, sp);
                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to generate preview: {0}, {1}", m.PreviewFilePath, ex.Message);
                    error++;
                }

                reporter.Done = i + 1;
                reporter.ProgressText = $"[{i + 1}/{totalCount}] {m}";
                _vmMain.ProgressValue = (i + 1f) / totalCount;
            }
            _vmMain.ProgressState = TaskbarItemProgressState.None;

            if (error > 0)
                _logger.Warn("Preview generation {0} successfully, {1} failed", success, error);
            else
                _logger.Info("{0} previews generated successfully", success);

            _logger.LogCurrentProcessMemoryUsage();
        }

        /// <summary>
        /// 使用提供的包围盒设置自动分辨率, 并留有 1 像素的边距
        /// </summary>
        private void SetAutoResolution(BaseExporter exporter, Rect bounds)
        {
            uint margin = 1;
            uint maxResolution = _previewMaxResolution - margin * 2;
            var resolution = bounds.Size.ToVector2u();
            if (resolution.X >= maxResolution || resolution.Y >= maxResolution)
            {
                // 缩小到最大像素限制
                var scale = Math.Min(maxResolution / bounds.Width, maxResolution / bounds.Height);
                resolution.X = (uint)(bounds.Width * scale);
                resolution.Y = (uint)(bounds.Height * scale);
            }
            exporter.Resolution = new(resolution.X + margin * 2, resolution.Y + margin * 2);

            var viewBounds = bounds.ToFloatRect().GetCanvasBounds(resolution, 2);
            exporter.Size = new(viewBounds.Width, -viewBounds.Height);
            exporter.Center = viewBounds.Position + viewBounds.Size / 2;
            exporter.Rotation = 0;
        }
    }
}
