using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using SFML.Audio;
using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace SpineViewer.ViewModels
{
    public class ExplorerListViewModel : ObservableObject
    {
        /// <summary>
        /// 预览图的保存质量
        /// </summary>
        public static int PreviewQuality { get; set; } = 80;

        /// <summary>
        /// 缩略图文件名格式字符串, 需要一个参数
        /// </summary>
        public static string PreviewFileNameFormat => ".{0}.preview.webp";

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly MainWindowViewModel _vmMain;

        /// <summary>
        /// 当前目录路径
        /// </summary>
        private string? _currentDirectory;

        /// <summary>
        /// 当前目录下文件项缓存
        /// </summary>
        private readonly List<ExplorerItemViewModel> _items = [];

        public ExplorerListViewModel(MainWindowViewModel vmMain)
        {
            _vmMain = vmMain;
        }

        /// <summary>
        /// 筛选字符串
        /// </summary>
        public string? FilterString
        {
            get => _filterString;
            set
            {
                if (!SetProperty(ref _filterString, value)) return;
                if (string.IsNullOrWhiteSpace(_filterString))
                {
                    _shownItems = _items.ToList();
                }
                else
                {
                    _shownItems = [];
                    _shownItems.AddRange(_items.Where(it => it.FileName.Contains(_filterString)));
                }
                OnPropertyChanged(nameof(ShownItems));
            }
        }
        private string? _filterString;

        /// <summary>
        /// 当前目录下的所有子项文件, 含递归目录
        /// </summary>
        public List<ExplorerItemViewModel> ShownItems => _shownItems;
        private List<ExplorerItemViewModel> _shownItems = [];

        /// <summary>
        /// 选择项, 显示某一项的具体信息和预览图
        /// </summary>
        public ExplorerItemViewModel? SelectedItem => _selectedItem;
        private ExplorerItemViewModel? _selectedItem;

        /// <summary>
        /// 选择文件夹命令
        /// </summary>
        public RelayCommand Cmd_ChangeCurrentDirectory => _cmd_ChangeCurrentDirectory ??= new(() =>
        {
            if (OpenFolderService.OpenFolder(out var selectedPath))
            {
                _currentDirectory = selectedPath;
                RefreshItems();
            }
        });
        private RelayCommand? _cmd_ChangeCurrentDirectory;

        public RelayCommand Cmd_RefreshItems => _cmd_RefreshItems ??= new(RefreshItems);
        private RelayCommand? _cmd_RefreshItems;

        /// <summary>
        /// 选中项发生变化命令
        /// </summary>
        public RelayCommand<IList?> Cmd_SelectionChanged => _cmd_SelectionChanged ??= new(args =>
        {
            if (args is null || args.Count != 1)
            {
                SetProperty(ref _selectedItem, null, nameof(SelectedItem));
            }
            else
            {
                SetProperty(ref _selectedItem, args[0] as ExplorerItemViewModel, nameof(SelectedItem));
            }
        });
        private RelayCommand<IList?>? _cmd_SelectionChanged;

        /// <summary>
        /// 右键菜单, 添加到模型列表
        /// </summary>
        public RelayCommand<IList?> Cmd_AddSelectedItems => _cmd_AddSelectedItems ??= new(AddSelectedItems_Execute, args => args is not null && args.Count > 0);
        private RelayCommand<IList?>? _cmd_AddSelectedItems;

        private void AddSelectedItems_Execute(IList? args)
        {
            if (args is null || args.Count <= 0) return;
            _vmMain.SpineObjectListViewModel.AddSpineObjectFromFileList(args.Cast<ExplorerItemViewModel>().Select(m => m.FullPath).ToArray());
        }

        /// <summary>
        /// 对参数项生成预览图
        /// </summary>
        public RelayCommand<IList?> Cmd_GeneratePreviews => _cmd_GeneratePreviews ??= new(GeneratePreview_Execute, args => args is not null && args.Count > 0);
        private RelayCommand<IList?>? _cmd_GeneratePreviews;

        private void GeneratePreview_Execute(IList? args)
        {
            if (args is null || args.Count <= 0) return;

            if (args.Count <= 1)
            {
                var m = (ExplorerItemViewModel)args[0];
                try
                {
                    using var sp = new SpineObject(m.FullPath);
                    sp.Skeleton.GetBounds(out var x, out var y, out var w, out var h);
                    var bounds = new SFML.Graphics.FloatRect(x, y, w, h).GetCanvasBounds(new(510, 510), 2);
                    using var exporter = new FrameExporter(512, 512)
                    {
                        Center = bounds.Position + bounds.Size / 2,
                        Size = new(bounds.Width, -bounds.Height),
                        Format = SkiaSharp.SKEncodedImageFormat.Webp,
                        Quality = PreviewQuality,
                    };
                    exporter.Export(m.PreviewFilePath, sp);
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to generate preview: {0}, {1}", m.PreviewFilePath, ex.Message);
                }
                _logger.LogCurrentProcessMemoryUsage();
            }
            else
            {
                ProgressService.RunAsync((pr, ct) => GeneratePreviewTask(
                    args.Cast<ExplorerItemViewModel>().ToArray(), pr, ct), 
                    AppResource.Str_GeneratePreviewsTitle
                );
            }
        }

        private void GeneratePreviewTask(ExplorerItemViewModel[] models, IProgressReporter reporter, CancellationToken ct)
        {
            int totalCount = models.Length;
            int success = 0;
            int error = 0;

            _vmMain.ProgressState = TaskbarItemProgressState.Normal;
            _vmMain.ProgressValue = 0;

            reporter.Total = totalCount;
            reporter.Done = 0;
            reporter.ProgressText = $"[0/{totalCount}]";

            using var exporter = new FrameExporter(512, 512)
            {
                Format = SkiaSharp.SKEncodedImageFormat.Webp,
                Quality = PreviewQuality,
            };
            for (int i = 0; i < totalCount; i++)
            {
                if (ct.IsCancellationRequested) break;

                var m = models[i];
                reporter.ProgressText = $"[{i}/{totalCount}] {m.FullPath}";

                try
                {
                    using var sp = new SpineObject(m.FullPath);
                    sp.Skeleton.GetBounds(out var x, out var y, out var w, out var h);
                    var bounds = new SFML.Graphics.FloatRect(x, y, w, h).GetCanvasBounds(new(510, 510), 2);
                    exporter.Center = bounds.Position + bounds.Size / 2;
                    exporter.Size = new(bounds.Width, -bounds.Height);
                    exporter.Export(m.PreviewFilePath, sp);
                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
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
        /// 删除参数项的预览图
        /// </summary>
        public RelayCommand<IList?> Cmd_DeletePreviews => _cmd_DeletePreviews ??= new(DeletePreview_Execute, args => args is not null && args.Count > 0);
        private RelayCommand<IList?>? _cmd_DeletePreviews;

        private void DeletePreview_Execute(IList? args)
        {
            if (args is null || args.Count <= 0) return;
            if (!MessagePopupService.Quest(string.Format(AppResource.Str_DeleteItemsQuest, args.Count))) return;

            if (args.Count <= 10)
            {
                foreach (var m in args.Cast<ExplorerItemViewModel>())
                {
                    try
                    {
                        File.Delete(m.PreviewFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Trace(ex.ToString());
                        _logger.Error("Failed to delete preview: {0}, {1}", m.PreviewFilePath, ex.Message);
                    }
                }
                _logger.LogCurrentProcessMemoryUsage();
            }
            else
            {
                ProgressService.RunAsync((pr, ct) => DeletePreviewTask(
                    args.Cast<ExplorerItemViewModel>().ToArray(), pr, ct),
                    AppResource.Str_DeletePreviewsTitle
                );
            }
        }

        private void DeletePreviewTask(ExplorerItemViewModel[] models, IProgressReporter reporter, CancellationToken ct)
        {
            int totalCount = models.Length;
            int success = 0;
            int error = 0;

            _vmMain.ProgressState = TaskbarItemProgressState.Normal;
            _vmMain.ProgressValue = 0;

            reporter.Total = totalCount;
            reporter.Done = 0;
            reporter.ProgressText = $"[0/{totalCount}]";
            for (int i = 0; i < totalCount; i++)
            {
                if (ct.IsCancellationRequested) break;

                var m = models[i];
                reporter.ProgressText = $"[{i}/{totalCount}] {m.FullPath}";

                try
                {
                    File.Delete(m.PreviewFilePath);
                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to delete preview: {0}, {1}", m.PreviewFilePath, ex.Message);
                    error++;
                }

                reporter.Done = i + 1;
                reporter.ProgressText = $"[{i + 1}/{totalCount}] {m}";
                _vmMain.ProgressValue = (i + 1f) / totalCount;
            }
            _vmMain.ProgressState = TaskbarItemProgressState.None;

            if (error > 0)
                _logger.Warn("Preview deletion {0} successfully, {1} failed", success, error);
            else
                _logger.Info("{0} previews deleted successfully", success);

            _logger.LogCurrentProcessMemoryUsage();
        }

        /// <summary>
        /// 刷新显示, 可以更新文件夹项缓存
        /// </summary>
        public void RefreshItems()
        {
            _items.Clear();
            if (Directory.Exists(_currentDirectory))
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(_currentDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        var lowerPath = file.ToLower();
                        if (SpineObject.PossibleSuffixMapping.Keys.Any(lowerPath.EndsWith))
                            _items.Add(new(file));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to enumerate files in dir: {0}, {1}", _currentDirectory, ex.Message);
                }
            }

            _shownItems = [];
            if (string.IsNullOrWhiteSpace(_filterString))
            {
                _shownItems = _items.ToList();
            }
            else
            {
                _shownItems = [];
                _shownItems.AddRange(_items.Where(it => it.FileName.Contains(_filterString)));
            }
            OnPropertyChanged(nameof(ShownItems));
        }
    }

    public class ExplorerItemViewModel : ObservableObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ExplorerItemViewModel(string path)
        {
            FullPath = Path.GetFullPath(path);
            FileDirectory = Path.GetDirectoryName(FullPath) ?? "";
            FileName = Path.GetFileName(FullPath);
            PreviewFilePath = Path.Combine(FileDirectory, string.Format(ExplorerListViewModel.PreviewFileNameFormat, FileName));
        }

        /// <summary>
        /// 完整路径
        /// </summary
        public string FullPath { get; }

        /// <summary>
        /// 文件所处目录
        /// </summary>
        public string FileDirectory { get; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// 预览图路径
        /// </summary>
        public string PreviewFilePath { get; }

        /// <summary>
        /// 预览图
        /// </summary>
        public ImageSource? PreviewImage 
        { 
            get
            {
                try
                {
                    return WpfExtension.LoadWebpWithAlpha(PreviewFilePath);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Warn("Failed to load preview image for {0}, {1}", FullPath, ex.Message);
                    return null;
                }
            }
        }
    }
}
