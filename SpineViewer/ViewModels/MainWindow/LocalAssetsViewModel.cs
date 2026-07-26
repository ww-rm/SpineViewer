using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine;
using SpineViewer.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// 缩略图文件名格式字符串, 需要一个参数
        /// </summary>
        public static string PreviewFileNameFormat => ".{0}.preview.webp";

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly MainWindowViewModel _vmMain;

        private LocalDirectoryViewModel? _selectedDirectory = null;

        public LocalAssetsViewModel(MainWindowViewModel vmMain)
        {
            _vmMain = vmMain;
#if DEBUG
            _localDirectories.Add(new(@"D:\ACGN\AzurLane_Export\AzurLane_SD\docs"));
            _localDirectories.Add(new(@"D:\ACGN\AzurLane_Export\AzurLane_Dynamic\docs"));

            foreach (var a in _localDirectories)
                a.RefreshItems();
#endif
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
        /// 强制刷新列表项命令
        /// </summary>
        public RelayCommand Cmd_RefreshItems => _cmd_RefreshItems ??= new(() => RefreshItems(true));
        private RelayCommand? _cmd_RefreshItems;

        /// <summary>
        /// 刷新目录下的文件项, 可以更新文件夹项缓存
        /// </summary>
        public void RefreshItems(bool forceRefreshCache = false)
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
    }

    /// <summary>
    /// 本地资源目录对象
    /// </summary>
    public class LocalDirectoryViewModel : ObservableObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public LocalDirectoryViewModel(string path)
        {
            if (!Directory.Exists(path))
                throw new FileNotFoundException("Directory not exists", nameof(path));
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
        public string Name { get => _name; set => SetProperty(ref _name, value); }
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
    }

    /// <summary>
    /// 本地资源对象
    /// </summary>
    public class LocalDirectoryItemViewModel : ObservableObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public LocalDirectoryItemViewModel(string path)
        {
            FullPath = Path.GetFullPath(path);
            FileDirectory = Path.GetDirectoryName(FullPath) ?? "";
            FileName = Path.GetFileName(FullPath);
            PreviewFilePath = Path.Combine(FileDirectory, string.Format(LocalAssetsViewModel.PreviewFileNameFormat, FileName));
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
    }
}
