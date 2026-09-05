using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
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

namespace SpineViewer.ViewModels.Assets
{
    public abstract class AssetsViewModel : ObservableObject
    {
        /// <summary>
        /// 资源相关信息的缓存目录
        /// </summary>
        public static readonly string CacheDirectory = Path.Combine(App.CacheDirectory, "assets");

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
        public abstract IReadOnlyList<AssetsRepoViewModel> AssetsRepos { get; }

        /// <summary>
        /// 资源文件夹选中项发生变化命令
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_AssetsRepoSelectionChanged { get; }

        /// <summary>
        /// 添加资源库
        /// </summary>
        public abstract RelayCommand Cmd_AddAssetsRepo { get; }

        /// <summary>
        /// 移除资源库
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_RemoveAssetsRepo { get; }

        /// <summary>
        /// 资源库上移一位
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_MoveUpAssetsRepo { get; }

        /// <summary>
        /// 资源库下移一位
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_MoveDownAssetsRepo { get; }

        /// <summary>
        /// 在资源管理器中打开资源
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_OpenAssetsInExplorer { get; }

        /// <summary>
        /// 编辑资源库信息
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_EditAssetsRepo { get; }

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
        public abstract IReadOnlyList<AssetsItemViewModel> ShownItems { get; }

        /// <summary>
        /// 模型列表筛选字符串
        /// </summary>
        public abstract string? FilterString { get; set; }

        /// <summary>
        /// 资源文件选中项发生变化命令
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_AssetsItemSelectionChanged { get; }

        /// <summary>
        /// 强制刷新列表项命令
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_RefreshRepoItems { get; }

        /// <summary>
        /// 导入选中的模型文件或者资源库
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_ImportSelectedAssets { get; }

        #endregion

        #region 预览图管理

        /// <summary>
        /// 为选中的资源库/文件项生成预览图
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_GeneratePreviews { get; }

        /// <summary>
        /// 为选中的目录/文件项删除预览图
        /// </summary>
        public abstract RelayCommand<IList?> Cmd_DeletePreviews { get; }

        #endregion
    }

    public abstract class AssetsViewModel<TRepo, TItem> : AssetsViewModel
        where TRepo : AssetsRepoViewModel<TItem>
        where TItem : AssetsItemViewModel
    {
        public AssetsViewModel(MainWindowViewModel vmMain) : base(vmMain) { }

        #region 资源库列表管理

        /// <summary>
        /// 当前选中的资源库
        /// </summary>
        private TRepo? _selectedAssetsRepo;

        public override IReadOnlyList<TRepo> AssetsRepos { get => _assetsRepos; }
        protected readonly ObservableCollection<TRepo> _assetsRepos = [];

        public override RelayCommand<IList?> Cmd_AssetsRepoSelectionChanged => _cmd_AssetsRepoSelectionChanged ??= new(args =>
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
            _ = UpdateShownItemsAsync();
        });
        private RelayCommand<IList?>? _cmd_AssetsRepoSelectionChanged;

        public override RelayCommand Cmd_AddAssetsRepo => _cmd_AddAssetsRepo ??= new(AddAssetsRepo_Execute);
        private RelayCommand? _cmd_AddAssetsRepo;

        private void AddAssetsRepo_Execute()
        {
            var repo = AddAssetsRepo();
            if (repo is null) return;

            _assetsRepos.Add(repo);
            SaveAssetsRepos();
        }

        public override RelayCommand<IList?> Cmd_RemoveAssetsRepo => _cmd_RemoveAssetsRepo ??= new(RemoveAssetsRepo_Execute, CommandCanExecute.AtLeastOne);
        private RelayCommand<IList?>? _cmd_RemoveAssetsRepo;

        private void RemoveAssetsRepo_Execute(IList? args)
        {
            if (!CommandCanExecute.AtLeastOne(args)) return;

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

        public override RelayCommand<IList?> Cmd_MoveUpAssetsRepo => _cmd_MoveUpAssetsRepo ??= new(MoveUpAssetsRepo_Execute, CommandCanExecute.OnlyOne);
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

        public override RelayCommand<IList?> Cmd_MoveDownAssetsRepo => _cmd_MoveDownAssetsRepo ??= new(MoveDownAssetsRepo_Execute, CommandCanExecute.OnlyOne);
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

        public override RelayCommand<IList?> Cmd_OpenAssetsInExplorer => _cmd_OpenAssetsInExplorer ??= new(OpenAssetsInExplorer_Execute, CommandCanExecute.OnlyOne);
        private RelayCommand<IList?>? _cmd_OpenAssetsInExplorer;

        private void OpenAssetsInExplorer_Execute(IList? args)
        {
            if (!CommandCanExecute.OnlyOne(args)) return;

            var obj = (IExplorerOpenable)args[0]!;
            obj.OpenDirectoryInExplorer();
        }

        public override RelayCommand<IList?> Cmd_EditAssetsRepo => _cmd_EditAssetsRepo ??= new(EditAssetsRepo_Execute, CommandCanExecute.OnlyOne);
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

        #endregion

        #region 资源库模型列表管理

        public override IReadOnlyList<TItem> ShownItems { get => _shownItems; }
        private List<TItem> _shownItems = [];

        public override string? FilterString
        {
            get => string.IsNullOrWhiteSpace(_filterString) ? null : _filterString;
            set
            {
                if (!SetProperty(ref _filterString, value)) return;
                _ = UpdateShownItemsAsync();
            }
        }
        private string? _filterString;

        public override RelayCommand<IList?> Cmd_AssetsItemSelectionChanged => _cmd_AssetsItemSelectionChanged ??= new(args =>
        {
            // 选中单个目录时显示该目录下所有文件项
            if (!CommandCanExecute.OnlyOne(args))
            {
                _vmMain.AssetsPreviewViewModel.PreviewImage = null;
                return;
            }

            var item = (TItem)args[0]!;
            _vmMain.AssetsPreviewViewModel.PreviewImage = item.PreviewImage;
        });
        private RelayCommand<IList?>? _cmd_AssetsItemSelectionChanged;

        public override RelayCommand<IList?> Cmd_RefreshRepoItems => _cmd_RefreshRepoItems ??= new(
            args =>
            {
                if (!CommandCanExecute.OnlyOne(args)) return;
                _ = UpdateShownItemsAsync(true);
            },
            CommandCanExecute.OnlyOne
        );
        private RelayCommand<IList?>? _cmd_RefreshRepoItems;

        public override RelayCommand<IList?> Cmd_ImportSelectedAssets => _cmd_ImportSelectedAssets ??= new(ImportSelectedAssets_Execute, CommandCanExecute.AtLeastOne);
        private RelayCommand<IList?>? _cmd_ImportSelectedAssets;

        private void ImportSelectedAssets_Execute(IList? args)
        {
            if (!CommandCanExecute.AtLeastOne(args))
                return;

            var items = GetItems(args);

            _vmMain.SpineObjectListViewModel.AddSpineObjectFromFileList(items.Select(m => m.LocalFullPath));
        }

        /// <summary>
        /// <see cref="UpdateShownItemsAsync(bool)"/> 异步任务计数器, 用于区分执行先后顺序
        /// </summary>
        private long _updateShownItemsAsyncCounter = 0;

        /// <summary>
        /// 更新 <see cref="ShownItems"/>
        /// </summary>
        private async Task UpdateShownItemsAsync(bool refreshRepoItems = false)
        {
            // 先清空显示
            SetProperty(ref _shownItems, [], nameof(ShownItems));

            List<TItem> shownItems = [];
            var repo = _selectedAssetsRepo;
            var filter = _filterString;

            // 保存进入时的计数器
            var counter1 = Interlocked.Increment(ref _updateShownItemsAsyncCounter);

            if (repo is not null)
            {
                if (!repo.IsItemsLoaded || refreshRepoItems)
                {
                    await repo.RefreshItemsAsync();
                }

                if (string.IsNullOrWhiteSpace(filter))
                {
                    shownItems.AddRange(repo.Items);
                }
                else
                {
                    shownItems.AddRange(repo.Items.Where(it => it.FileName.Contains(filter, StringComparison.OrdinalIgnoreCase)));
                }
            }

            var counter2 = Interlocked.Read(ref _updateShownItemsAsyncCounter);

            // 如果此次运行是最新的, 则按这个结果更新
            if (counter1 >= counter2)
            {
                SetProperty(ref _shownItems, shownItems, nameof(ShownItems));
            }
        }

        #endregion

        #region 预览图管理

        public override RelayCommand<IList?> Cmd_GeneratePreviews => _cmd_GeneratePreviews ??= new(GeneratePreviews_Execute, CommandCanExecute.AtLeastOne);
        private RelayCommand<IList?>? _cmd_GeneratePreviews;

        private void GeneratePreviews_Execute(IList? args)
        {
            if (!CommandCanExecute.AtLeastOne(args))
                return;

            if (!DialogService.ShowGeneratePreviewsDialog(_vmMain.AssetsPreviewViewModel))
                return;

            var items = GetItems(args);
            _vmMain.AssetsPreviewViewModel.GeneratePreviews(items);
        }

        public override RelayCommand<IList?> Cmd_DeletePreviews => _cmd_DeletePreviews ??= new(DeletePreviews_Execute, CommandCanExecute.AtLeastOne);
        private RelayCommand<IList?>? _cmd_DeletePreviews;

        private void DeletePreviews_Execute(IList? args)
        {
            if (!CommandCanExecute.AtLeastOne(args))
                return;

            var items = GetItems(args);

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

        private void DeletePreviewsTask(List<TItem> items, IProgressReporter reporter, CancellationToken ct)
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
                reporter.ProgressText = $"[{i}/{totalCount}] {it.LocalFullPath}";

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

        private static List<TItem> GetItems(IList args)
        {
            List<TItem> items = [];
            foreach (var it in args!)
            {
                switch (it)
                {
                    case TRepo repo:
                        items.AddRange(repo.Items);
                        break;
                    case TItem item:
                        items.Add(item);
                        break;
                    default:
                        _logger.Warn("Invalid type {0}, skip it", it.GetType().Name);
                        break;
                }
            }
            return items;
        }

        #endregion
    }
}
