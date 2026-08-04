using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine;
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
using System.Windows.Media;

namespace SpineViewer.ViewModels.MainWindow
{
    public class LocalAssetsViewModel : ObservableObject
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        public static readonly string LocalAssetsFilePath = Path.Combine(App.ProcessDataDirectory, "localassets.json");

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
