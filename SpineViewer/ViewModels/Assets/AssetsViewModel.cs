using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.Utils;
using SpineViewer.ViewModels.MainWindow;
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

namespace SpineViewer.ViewModels.Assets
{
    public abstract class AssetsViewModel<TRepo, TItem> : ObservableObject
        where TRepo : AssetsRepoViewModel<TItem>
        where TItem : AssetsItemViewModel
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        protected readonly MainWindowViewModel _vmMain;

        public AssetsViewModel(MainWindowViewModel vmMain)
        {
            _vmMain = vmMain;
        }

        #region 资源库列表管理

        /// <summary>
        /// 资源库列表
        /// </summary>
        public ObservableCollection<TRepo> AssetsRepos { get => _assetsRepos; }
        protected readonly ObservableCollection<TRepo> _assetsRepos = [];

        /// <summary>
        /// 当前选中的资源库
        /// </summary>
        protected TRepo? _selectedAssetsRepo;

        /// <summary>
        /// 资源文件夹选中项发生变化命令
        /// </summary>
        public AsyncRelayCommand<IList?> Cmd_AssetsRepoSelectionChanged => _cmd_AssetsRepoSelectionChanged ??= new(async args =>
        {
            // 选中单个目录时显示该目录下所有文件项
            if (CommandCanExecute.OnlyOne(args))
            {
                _selectedAssetsRepo = (TRepo)args[0]!;
            }
            else
            {
                _selectedAssetsRepo = null;
            }
            await RefreshShownItemsAsync();
        });
        private AsyncRelayCommand<IList?>? _cmd_AssetsRepoSelectionChanged;

        /// <summary>
        /// 添加资源库
        /// </summary>
        public RelayCommand Cmd_AddAssetsRepo => _cmd_AddAssetsRepo ??= new(AddAssetsRepo_Execute);
        private RelayCommand? _cmd_AddAssetsRepo;

        private void AddAssetsRepo_Execute()
        {
            var repo = AddAssetsRepo();
            if (repo is null) return;

            _assetsRepos.Add(repo);
            SaveAssetsRepos();
        }

        /// <summary>
        /// 移除资源库
        /// </summary>
        public RelayCommand<IList?> Cmd_RemoveAssetsRepo => _cmd_RemoveAssetsRepo ??= new(RemoveAssetsRepo_Execute, CommandCanExecute.MoreThanZero);
        private RelayCommand<IList?> _cmd_RemoveAssetsRepo;

        private void RemoveAssetsRepo_Execute(IList? args)
        {
            if (!CommandCanExecute.MoreThanZero(args)) return;

            if (args.Count > 1)
            {
                if (!MessagePopupService.OKCancel(string.Format(AppResource.Str_RemoveItemsQuest, args.Count)))
                    return;
            }

            // NOTE: 这里必须要浅拷贝一次, 不能直接对会被修改的绑定数据 args 进行 foreach 遍历
            foreach (var repo in args.Cast<TRepo>().ToArray())
            {
                _assetsRepos.Remove(repo);
            }

            SaveAssetsRepos();
        }

        /// <summary>
        /// 资源库上移一位
        /// </summary>
        public RelayCommand<IList?> Cmd_MoveUpAssetsRepo => _cmd_MoveUpAssetsRepo ??= new(MoveUpAssetsRepo_Execute, CommandCanExecute.OnlyOne);
        private RelayCommand<IList?>? _cmd_MoveUpAssetsRepo;

        private void MoveUpAssetsRepo_Execute(IList? args)
        {
            if (!CommandCanExecute.OnlyOne(args)) return;

            var repo = (TRepo)args[0]!;
            var idx = _assetsRepos.IndexOf(repo);
            if (idx <= 0) return;
            _assetsRepos.Move(idx, idx - 1);

            SaveAssetsRepos();
        }

        /// <summary>
        /// 资源库下移一位
        /// </summary>
        public RelayCommand<IList?> Cmd_MoveDownAssetsRepo => _cmd_MoveDownAssetsRepo ??= new(MoveDownAssetsRepo_Execute, CommandCanExecute.OnlyOne);
        private RelayCommand<IList?>? _cmd_MoveDownAssetsRepo;

        private void MoveDownAssetsRepo_Execute(IList? args)
        {
            if (!CommandCanExecute.OnlyOne(args)) return;

            var repo = (TRepo)args[0]!;
            var idx = _assetsRepos.IndexOf(repo);
            if (idx < 0 || idx >= _assetsRepos.Count - 1) return;
            _assetsRepos.Move(idx, idx + 1);

            SaveAssetsRepos();
        }

        /// <summary>
        /// 在资源管理器中打开资源
        /// </summary>
        public RelayCommand<IList?> Cmd_OpenAssetsInExplorer => _cmd_OpenAssetsInExplorer ??= new(OpenAssetsInExplorer_Execute, CommandCanExecute.OnlyOne);
        private RelayCommand<IList?>? _cmd_OpenAssetsInExplorer;

        private void OpenAssetsInExplorer_Execute(IList? args)
        {
            if (!CommandCanExecute.OnlyOne(args)) return;

            var obj = (IExplorerOpenable)args[0]!;
            obj.OpenDirectoryInExplorer();
        }

        /// <summary>
        /// 编辑资源库信息
        /// </summary>
        public RelayCommand<IList?> Cmd_EditAssetsRepo => _cmd_EditAssetsRepo ??= new(EditAssetsRepo_Execute, CommandCanExecute.OnlyOne);
        private RelayCommand<IList?>? _cmd_EditAssetsRepo;

        private void EditAssetsRepo_Execute(IList? args)
        {
            if (!CommandCanExecute.OnlyOne(args)) return;

            var repo = (TRepo)args[0]!;

            if (!EditAssetsRepo(repo)) return;

            SaveAssetsRepos();
        }

        /// <summary>
        /// 添加资源库
        /// </summary>
        /// <returns>添加的资源库, 取消或失败返回 null</returns>
        protected abstract TRepo? AddAssetsRepo();

        /// <summary>
        /// 编辑资源库信息
        /// </summary>
        /// <returns>取消或失败返回 false</returns>
        protected abstract bool EditAssetsRepo(TRepo repo);

        /// <summary>
        /// 保存资源库列表
        /// </summary>
        public abstract void SaveAssetsRepos();

        /// <summary>
        /// 加载资源库列表
        /// </summary>
        public abstract void LoadAssetsRepos();

        #endregion

        #region 资源库模型列表管理

        /// <summary>
        /// 当前被显示的模型文件列表
        /// </summary>
        public IReadOnlyList<TItem> ShownItems { get => _shownItems; }
        private List<TItem> _shownItems = [];

        /// <summary>
        /// 模型列表筛选字符串
        /// </summary>
        public string? FilterString
        {
            get => string.IsNullOrWhiteSpace(_filterString) ? null : _filterString;
            set
            {
                if (!SetProperty(ref _filterString, value)) return;
                _ = RefreshShownItemsAsync();
            }
        }
        private string? _filterString;

        /// <summary>
        /// 资源文件选中项发生变化命令
        /// </summary>
        public RelayCommand<IList?> Cmd_AssetsItemSelectionChanged => _cmd_AssetsItemSelectionChanged ??= new(args =>
        {
            // 选中单个目录时显示该目录下所有文件项
            if (!CommandCanExecute.OnlyOne(args))
            {
                _vmMain.PreviewImage = null;
                return;
            }

            var item = (TItem)args[0]!;
            _vmMain.PreviewImage = item.PreviewImage;
        });
        private RelayCommand<IList?>? _cmd_AssetsItemSelectionChanged;

        /// <summary>
        /// 强制刷新列表项命令
        /// </summary>
        public AsyncRelayCommand<IList?> Cmd_RefreshShownItems => _cmd_RefreshShownItems ??= new(
            async args =>
            {
                if (!CommandCanExecute.OnlyOne(args)) return;
                await RefreshShownItemsAsync(true);
            },
            CommandCanExecute.OnlyOne
        );
        private AsyncRelayCommand<IList?>? _cmd_RefreshShownItems;

        private bool _isRefreshing = false;

        /// <summary>
        /// 刷新 <see cref="ShownItems"/>
        /// </summary>
        protected async Task RefreshShownItemsAsync(bool refreshRepoItems = false, CancellationToken ct = default)
        {
            if (_isRefreshing) return;
            _isRefreshing= true;

            _shownItems = [];
            if (_selectedAssetsRepo is not null)
            {
                if (_selectedAssetsRepo.Items.Count <= 0 || refreshRepoItems)
                {
                    await _selectedAssetsRepo.RefreshItemsAsync(ct);
                }

                if (string.IsNullOrWhiteSpace(_filterString))
                {
                    _shownItems.AddRange(_selectedAssetsRepo.Items);
                }
                else
                {
                    _shownItems.AddRange(_selectedAssetsRepo.Items.Where(it => it.FileName.Contains(_filterString, StringComparison.OrdinalIgnoreCase)));
                }
            }
            OnPropertyChanged(nameof(ShownItems));

            _isRefreshing = false;
        }

        #endregion
    }


}
