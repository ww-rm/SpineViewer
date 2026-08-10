using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

namespace SpineViewer.ViewModels.MainWindow
{
    public class LocalAssetsViewModel : ObservableObject
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        public static readonly string LocalAssetsFilePath = Path.Combine(App.ProcessDataDirectory, "localassets.json");

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 辅助函数, 从 Command 的传参里获取所有的文件项
        /// </summary>
        /// <param name="args">元素类型可以是 <see cref="LocalDirectoryViewModel"/> 或者 <see cref="LocalDirectoryItemViewModel"/></param>
        private static List<LocalDirectoryItemViewModel> GetDirectoryItems(IList args)
        {
            List<LocalDirectoryItemViewModel> items = [];
            foreach (var it in args!)
            {
                switch (it)
                {
                    case LocalDirectoryViewModel dvm:
                        dvm.RefreshItems(); // 需要强制刷新一次文件列表缓存
                        items.AddRange(dvm.Items);
                        break;
                    case LocalDirectoryItemViewModel itm:
                        items.Add(itm);
                        break;
                    default:
                        _logger.Warn("Invalid type {0}, skip delete it", it.GetType().Name);
                        break;
                }
            }
            return items;
        }

        private readonly MainWindowViewModel _vmMain;

        private LocalDirectoryViewModel? _selectedDirectory = null;

        public LocalAssetsViewModel(MainWindowViewModel vmMain)
        {
            _vmMain = vmMain;
        }

        /// <summary>
        /// 本地资源目录集合
        /// </summary>
        public ObservableCollection<LocalDirectoryViewModel> LocalDirectories => _localDirectories;
        private readonly ObservableCollection<LocalDirectoryViewModel> _localDirectories = [];

        /// <summary>
        /// 当前选中目录下的所有子项文件, 含递归目录
        /// </summary>
        public List<LocalDirectoryItemViewModel> ShownItems => _shownItems;
        private List<LocalDirectoryItemViewModel> _shownItems = [];

        /// <summary>
        /// 筛选字符串
        /// </summary>
        public string? FilterString
        {
            get => string.IsNullOrWhiteSpace(_filterString) ? null : _filterString;
            set
            {
                if (!SetProperty(ref _filterString, value)) 
                    return;
                RefreshItems();
            }
        }
        private string? _filterString;

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
        /// 选中项发生变化命令
        /// </summary>
        public RelayCommand<IList?> Cmd_SelectionChanged => _cmd_SelectionChanged ??= new(args =>
        {
            // 选中单个目录时显示该目录下所有文件项
            if (args is null || args.Count != 1)
            {
                _selectedDirectory = null;
            }
            else
            {
                _selectedDirectory = (LocalDirectoryViewModel)args[0]!;
            }
            RefreshItems();
        });
        private RelayCommand<IList?>? _cmd_SelectionChanged;

        /// <summary>
        /// 添加本地资源目录
        /// </summary>
        public RelayCommand<IList?> Cmd_AddLocalAsset => _cmd_AddLocalAsset ??= new(AddLocalAsset_Execute);
        private RelayCommand<IList?>? _cmd_AddLocalAsset;

        private void AddLocalAsset_Execute(IList? args)
        {
            if (!DialogService.ShowOpenFolderDialog(out var selectedPath))
                return;

            _localDirectories.Add(new(selectedPath!));
            SaveLocalAssets();
        }

        /// <summary>
        /// 移除本地资源目录
        /// </summary>
        public RelayCommand<IList?> Cmd_RemoveLocalAsset => _cmd_RemoveLocalAsset ??= new(RemoveLocalAsset_Execute, RemoveLocalAsset_CanExecute);
        private RelayCommand<IList?> _cmd_RemoveLocalAsset;

        private void RemoveLocalAsset_Execute(IList? args)
        {
            if (!RemoveLocalAsset_CanExecute(args)) return;

            if (args!.Count > 1)
            {
                if (!MessagePopupService.OKCancel(string.Format(AppResource.Str_RemoveItemsQuest, args.Count)))
                    return;
            }

            // NOTE: 这里必须要浅拷贝一次, 不能直接对会被修改的绑定数据 args 进行 foreach 遍历
            foreach (var dvm in args.Cast<LocalDirectoryViewModel>().ToArray())
            {
                _localDirectories.Remove(dvm);
            }

            SaveLocalAssets();
        }

        private bool RemoveLocalAsset_CanExecute(IList? args)
        {
            if (args is null) return false;
            if (args.Count <= 0) return false;
            return true;
        }

        /// <summary>
        /// 目录上移一位
        /// </summary>
        public RelayCommand<IList?> Cmd_MoveUpLocalAsset => _cmd_MoveUpLocalAsset ??= new(MoveUpLocalAsset_Execute, MoveUpLocalAsset_CanExecute);
        private RelayCommand<IList?>? _cmd_MoveUpLocalAsset;

        private void MoveUpLocalAsset_Execute(IList? args)
        {
            if (!MoveUpLocalAsset_CanExecute(args)) return;
            var dvm = (LocalDirectoryViewModel)args![0]!;
            var idx = _localDirectories.IndexOf(dvm);
            if (idx <= 0) return;
            _localDirectories.Move(idx, idx - 1);

            SaveLocalAssets();
        }

        private bool MoveUpLocalAsset_CanExecute(IList? args)
        {
            if (args is null) return false;
            if (args.Count != 1) return false;
            return true;
        }

        /// <summary>
        /// 目录下移一位
        /// </summary>
        public RelayCommand<IList?> Cmd_MoveDownLocalAsset => _cmd_MoveDownLocalAsset ??= new(MoveDownLocalAsset_Execute, MoveDownLocalAsset_CanExecute);
        private RelayCommand<IList?>? _cmd_MoveDownLocalAsset;

        private void MoveDownLocalAsset_Execute(IList? args)
        {
            if (!MoveDownLocalAsset_CanExecute(args)) return;
            var dvm = (LocalDirectoryViewModel)args![0]!;
            var idx = _localDirectories.IndexOf(dvm);
            if (idx < 0 || idx >= _localDirectories.Count - 1) return;
            _localDirectories.Move(idx, idx + 1);

            SaveLocalAssets();
        }

        private bool MoveDownLocalAsset_CanExecute(IList? args)
        {
            if (args is null) return false;
            if (args.Count != 1) return false;
            return true;
        }

        /// <summary>
        /// 在资源管理中打开本地资源目录
        /// </summary>
        public RelayCommand<IList?> Cmd_OpenLocalAssetInExplorer => _cmd_OpenLocalAssetInExplorer ??= new(OpenLocalAssetInExplorer_Execute, OpenLocalAssetInExplorer_CanExecute);
        private RelayCommand<IList?>? _cmd_OpenLocalAssetInExplorer;

        private void OpenLocalAssetInExplorer_Execute(IList? args)
        {
            if (!OpenLocalAssetInExplorer_CanExecute(args)) return;
            var dvm = (LocalDirectoryViewModel)args![0]!;
            dvm.OpenInExplorer();
        }

        private bool OpenLocalAssetInExplorer_CanExecute(IList? args)
        {
            if (args is null) return false;
            if (args.Count != 1) return false;
            return true;
        }

        /// <summary>
        /// 强制刷新列表项命令
        /// </summary>
        public RelayCommand<IList?> Cmd_RefreshItems => _cmd_RefreshItems ??= new(
            args =>
            {
                if (args is null) return;
                if (args.Count != 1) return;
                RefreshItems(true);
            },
            args =>
            {
                if (args is null) return false;
                if (args.Count != 1) return false;
                return true;
            }
        );
        private RelayCommand<IList?>? _cmd_RefreshItems;

        /// <summary>
        /// 刷新目录下的文件项, 可以更新文件夹项缓存
        /// </summary>
        private void RefreshItems(bool forceRefreshCache = false)
        {
            _shownItems = [];
            if (_selectedDirectory is not null)
            {
                if (_selectedDirectory.Items.Count <= 0 || forceRefreshCache)
                {
                    _selectedDirectory.RefreshItems();
                }

                if (string.IsNullOrWhiteSpace(_filterString))
                {
                    _shownItems.AddRange(_selectedDirectory.Items);
                }
                else
                {
                    _shownItems.AddRange(_selectedDirectory.Items.Where(it => it.FileName.Contains(_filterString, StringComparison.OrdinalIgnoreCase)));
                }
            }
            OnPropertyChanged(nameof(ShownItems));
        }

        /// <summary>
        /// 编辑资源目录信息
        /// </summary>
        public RelayCommand<IList?> Cmd_EditLocalAsset => _cmd_EditLocalAsset ??= new(EditLocalAsset_Execute, EditLocalAsset_CanExecute);
        private RelayCommand<IList?>? _cmd_EditLocalAsset;

        private void EditLocalAsset_Execute(IList? args)
        {
            if (!EditLocalAsset_CanExecute(args)) return;
            var dvm = (LocalDirectoryViewModel)args![0]!;

            _logger.Warn("TODO: EditLocalAsset");
        }

        private bool EditLocalAsset_CanExecute(IList? args)
        {
            if (args is null) return false;
            if (args.Count != 1) return false;
            return true;
        }

        /// <summary>
        /// 为选中的目录/文件项生成预览图
        /// </summary>
        public RelayCommand<IList?> Cmd_GeneratePreviews => _cmd_GeneratePreviews ??= new(GeneratePreviews_Execute, GeneratePreviews_CanExecute);
        private RelayCommand<IList?>? _cmd_GeneratePreviews;

        private void GeneratePreviews_Execute(IList? args)
        {
            if (!GeneratePreviews_CanExecute(args))
                return;

            // TODO: 弹出预览图参数对话框并判断用户选择

            var items = GetDirectoryItems(args!);
            GeneratePreviews(items);
        }

        private bool GeneratePreviews_CanExecute(IList? args)
        {
            if (args is null) return false;
            if (args.Count <= 0) return false;
            return true;
        }

        private void GeneratePreviews(List<LocalDirectoryItemViewModel> items)
        {
            if (items.Count <= 0)
                return;

            if (items.Count <= 1)
            {
                var m = items[0];
                try
                {
                    using var sp = new SpineObject(m.FullPath) { UsePma = PreviewPma };
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

        private void GeneratePreviewsTask(List<LocalDirectoryItemViewModel> items, IProgressReporter reporter, CancellationToken ct)
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
                reporter.ProgressText = $"[{i}/{totalCount}] {m.FullPath}";

                try
                {
                    using var sp = new SpineObject(m.FullPath);
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

        /// <summary>
        /// 为选中的目录/文件项删除预览图
        /// </summary>
        public RelayCommand<IList?> Cmd_DeletePreviews => _cmd_DeletePreviews ??= new(DeletePreviews_Execute, DeletePreviews_CanExecute);
        private RelayCommand<IList?>? _cmd_DeletePreviews;

        private void DeletePreviews_Execute(IList? args)
        {
            if (!DeletePreviews_CanExecute(args))
                return;

            var items = GetDirectoryItems(args!);

            if (items.Count <= 0)
                return;

            if (!MessagePopupService.OKCancel(string.Format(AppResource.Str_DeleteItemsQuest, items.Count))) 
                return;

            if (args.Count <= 10)
            {
                foreach (var it in items)
                {
                    try
                    {
                        File.Delete(it.PreviewFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex.ToString());
                        _logger.Error("Failed to delete preview: {0}, {1}", it.PreviewFilePath, ex.Message);
                    }
                }
            }
            else
            {
                ProgressService.RunAsync(
                    (pr, ct) => DeletePreviewsTask(items, pr, ct),
                    AppResource.Str_DeletePreviewsTitle
                );
            }
        }

        private bool DeletePreviews_CanExecute(IList? args)
        {
            if (args is null) return false;
            if (args.Count <= 0) return false;
            return true;
        }

        private void DeletePreviewsTask(List<LocalDirectoryItemViewModel> items, IProgressReporter reporter, CancellationToken ct)
        {
            int totalCount = items.Count;
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

                var it = items[i];
                reporter.ProgressText = $"[{i}/{totalCount}] {it.FullPath}";

                try
                {
                    File.Delete(it.PreviewFilePath);
                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to delete preview: {0}, {1}", it.PreviewFilePath, ex.Message);
                    error++;
                }

                reporter.Done = i + 1;
                reporter.ProgressText = $"[{i + 1}/{totalCount}] {it}";
                _vmMain.ProgressValue = (i + 1f) / totalCount;
            }
            _vmMain.ProgressState = TaskbarItemProgressState.None;

            if (error > 0)
                _logger.Warn("Preview deletion {0} successfully, {1} failed", success, error);
            else
                _logger.Info("{0} previews deleted successfully", success);
        }

        /// <summary>
        /// 导入选中的目录/文件项
        /// </summary>
        public RelayCommand<IList?> Cmd_ImportSelectedItems => _cmd_ImportSelectedItems ??= new(ImportSelectedItems_Execute, ImportSelectedItems_CanExecute);
        private RelayCommand<IList?>? _cmd_ImportSelectedItems;

        private void ImportSelectedItems_Execute(IList? args)
        {
            if (!ImportSelectedItems_CanExecute(args))
                return;

            var items = GetDirectoryItems(args!);

            if (items.Count <= 0)
                return;

            _vmMain.SpineObjectListViewModel.AddSpineObjectFromFileList(items.Select(m => m.FullPath).ToArray());
        }

        private bool ImportSelectedItems_CanExecute(IList? args)
        {
            if (args is null) return false;
            if (args.Count <= 0) return false;
            return true;
        }

        /// <summary>
        /// 从本地加载资源列表
        /// </summary>
        public void LoadLocalAssets()
        {
            // 先清空列表
            _selectedDirectory = null;
            RefreshItems();
            _localDirectories.Clear();

            if (JsonHelper.Deserialize<LocalAssetsModel>(LocalAssetsFilePath, out var m, true))
            {
                foreach (var it in m.LocalDirectories)
                {
                    _localDirectories.Add(new(it.FullPath) { Name = it.Name });
                }
            }
        }

        /// <summary>
        /// 保存资源列表至本地
        /// </summary>
        public void SaveLocalAssets()
        {
            var m = new LocalAssetsModel();

            foreach (var dvm in _localDirectories)
            {
                m.LocalDirectories.Add(new() { FullPath = dvm.FullPath, Name = dvm.Name });
            }

            JsonHelper.Serialize(m, LocalAssetsFilePath);
        }
    }

    /// <summary>
    /// 本地资源目录对象
    /// </summary>
    public sealed class LocalDirectoryViewModel : ObservableObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public LocalDirectoryViewModel(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            FullPath = Path.GetFullPath(path);
            _name = Path.GetFileName(FullPath);
        }

        /// <summary>
        /// 目录完整路径
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// 备注名称
        /// </summary>
        public string Name 
        { 
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = Path.GetFileName(FullPath);
                SetProperty(ref _name, value);
            }
        }
        private string _name;

        /// <summary>
        /// 该目录下所有的模型文件路径对象缓存, 含递归目录
        /// </summary>
        public List<LocalDirectoryItemViewModel> Items => _items;
        private readonly List<LocalDirectoryItemViewModel> _items = [];

        /// <summary>
        /// 刷新该目录下所有可能的模型文件路径缓存
        /// </summary>
        public void RefreshItems()
        {
            _items.Clear();

            if (!Directory.Exists(FullPath))
            {
                _logger.Error("Directory '{0}' is not existed.", FullPath);
                return;
            }

            try
            {
                foreach (var file in Directory.EnumerateFiles(FullPath, "*.*", SearchOption.AllDirectories))
                {
                    var lowerPath = file.ToLowerInvariant();
                    if (SpineObject.PossibleSuffixMapping.Keys.Any(lowerPath.EndsWith))
                        _items.Add(new(file));
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to enumerate files in dir: {0}, {1}", FullPath, ex.Message);
            }
        }

        /// <summary>
        /// 在资源管理器中打开目录
        /// </summary>
        public void OpenInExplorer()
        {
            if (!Directory.Exists(FullPath))
            {
                _logger.Error("Directory '{0}' is not existed.", FullPath);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{FullPath}\"",
                UseShellExecute = true,
            });
        }

        public override bool Equals(object? obj)
        {
            if (obj is LocalDirectoryViewModel vm)
                return vm.FullPath.Equals(FullPath);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }
    }

    /// <summary>
    /// 本地资源对象
    /// </summary>
    public sealed class LocalDirectoryItemViewModel : ObservableObject
    {
        /// <summary>
        /// 缩略图文件名格式字符串, 需要一个参数
        /// </summary>
        public static string PreviewFileNameFormat => ".{0}.preview.webp";

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public LocalDirectoryItemViewModel(string path)
        {
            FullPath = Path.GetFullPath(path);
            FileDirectory = Path.GetDirectoryName(FullPath) ?? "";
            FileName = Path.GetFileName(FullPath);
            PreviewFilePath = Path.Combine(FileDirectory, string.Format(PreviewFileNameFormat, FileName));
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
                    _logger.Debug(ex.ToString());
                    _logger.Warn("Failed to load preview image for {0}, {1}", FullPath, ex.Message);
                    return null;
                }
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is LocalDirectoryItemViewModel vm) 
                return vm.FullPath.Equals(FullPath);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }
    }
}
