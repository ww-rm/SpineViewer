using SpineViewer.Models;
using SpineViewer.Models.Octokit;
using SpineViewer.Services;
using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public sealed class GitHubAssetsRepoViewModel : AssetsRepoViewModel<GitHubAssetsItemViewModel>
    {
        public GitHubAssetsRepoViewModel(string owner, string repository, string sha)
        {
            _owner = owner;
            _repository = repository;
            _sha = sha;
            _cacheDirectory = Path.Combine(GitHubAssetsViewModel.GitHubAssetsCacheDirectory, _owner, _repository);
            _treeCachePath = Path.Combine(_cacheDirectory, $"{_sha}.json");
        }

        public GitHubAssetsRepoViewModel(GitHubAssetsRepoModel m) : this(m.Owner, m.Repository, m.Sha)
        {
            Name = m.Name;
        }

        /// <summary>
        /// 获取模型对象
        /// </summary>
        public GitHubAssetsRepoModel Model
        {
            get => new()
            {
                Owner = Owner,
                Repository = Repository,
                Sha = Sha,
                Name = Name
            };
            set => Name = value.Name;
        }

        private readonly string _cacheDirectory;
        private readonly string _treeCachePath;

        public string Owner { get => _owner; }
        private readonly string _owner;

        public string Repository { get => _repository; }
        private readonly string _repository;

        public string Sha { get => _sha; }
        private readonly string _sha;

        public override string LocalDirectory => throw new NotImplementedException();

        public override string DefaultName => $"{_owner}/{_repository}@{_sha[..7]}";

        /// <summary>
        /// <c>owner/repo@sha</c> 格式标识字符串
        /// </summary>
        public string RepoKey { get => $"{_owner}/{_repository}@{_sha}"; }

        public override IReadOnlyList<GitHubAssetsItemViewModel> Items { get => _items; }
        private List<GitHubAssetsItemViewModel> _items = [];

        public override bool IsItemsLoaded { get => _isItemsLoaded; }
        private bool _isItemsLoaded = false;

        public override bool IsItemsRefreshing { get => _isItemsRefreshing; }
        private bool _isItemsRefreshing = false;

        protected override async Task CreateRefreshItemsTask()
        {
            // 如果已加载则清除缓存
            if (_isItemsLoaded) 
                DeleteTreeDataCache();

            // 清空列表并设置状态属性
            SetProperty(ref _isItemsRefreshing, true, nameof(IsItemsRefreshing));
            SetProperty(ref _isItemsLoaded, false, nameof(IsItemsLoaded));
            SetProperty(ref _items, [], nameof(Items));

            var tree = await GetTreeDataCacheAsync();
            if (tree is null)
                return;

            List<GitHubAssetsItemViewModel> items = [];

            try
            {
                // TODO: 构造列表
                SetProperty(ref _isItemsLoaded, true, nameof(IsItemsLoaded));
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to refresh repo '{0}', {1}", Name, ex.Message);
            }

            SetProperty(ref _items, items, nameof(Items));
            SetProperty(ref _isItemsRefreshing, false, nameof(IsItemsRefreshing));
        }

        private void DeleteTreeDataCache()
        {
            try
            {
                File.Delete(_treeCachePath);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to delete cache: {0}, {1}", _treeCachePath, ex.Message);
            }
        }

        /// <summary>
        /// 获取 tree 缓存数据, 如果不存在则获取并缓存
        /// </summary>
        private async Task<TreeResponseModel?> GetTreeDataCacheAsync()
        {
            if (JsonHelper.Deserialize(_treeCachePath, out TreeResponseModel? obj, true))
                return obj;

            try
            {
                var client = GitHubService.GetClient();
                var res = await client.Git.Tree.GetRecursive(_owner, _repository, _sha);
                var model = new TreeResponseModel(res);
                JsonHelper.Serialize(model, _treeCachePath);
                return model;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to get tree data for repo '{0}', {1}", Name, ex.Message);
            }
            return null;
        }
    }
}
