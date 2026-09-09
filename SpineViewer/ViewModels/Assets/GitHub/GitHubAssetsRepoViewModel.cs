using Spine;
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

namespace SpineViewer.ViewModels.Assets.GitHub
{
    public sealed class GitHubAssetsRepoViewModel : AssetsRepoViewModel<GitHubAssetsItemViewModel>, IBrowserOpenable
    {
        private readonly string _owner;
        private readonly string _repository;
        private readonly string _sha;
        private readonly string _repoKey;
        private readonly string _githubUrl;
        private readonly string _defaultName;
        private readonly string _cacheDirectory;
        private readonly string _treeCachePath;

        public GitHubAssetsRepoViewModel(string owner, string repository, string sha)
        {
            _owner = owner;
            _repository = repository;
            _sha = sha;
            _repoKey = $"{_owner}/{_repository}@{_sha}";
            _githubUrl = $"https://{GitHubAssetsViewModel.GitHubUrlHost}/{_owner}/{_repository}/tree/{_sha}";
            _defaultName = $"{_owner}/{_repository}@{_sha[..7]}";
            _cacheDirectory = Path.Combine(GitHubAssetsViewModel.GitHubAssetsCacheDirectory, _owner, _repository);
            _treeCachePath = Path.Combine(_cacheDirectory, $"{_sha}.json");
        }

        public GitHubAssetsRepoViewModel(GitHubAssetsRepoModel m) : this(m.Owner, m.Repository, m.Sha)
        {
            Name = m.Name;
        }

        /// <summary>
        /// 仓库所有者
        /// </summary>
        public string Owner => _owner;

        /// <summary>
        /// 仓库名
        /// </summary>
        public string Repository => _repository;

        /// <summary>
        /// 仓库 SHA 值
        /// </summary>
        public string Sha => _sha;

        /// <summary>
        /// <c>&lt;owner&gt;/&lt;repo&gt;@&lt;sha&gt;</c> 格式标识字符串
        /// </summary>
        public string RepoKey => _repoKey;

        /// <summary>
        /// GitHub 浏览器访问链接
        /// </summary>
        public string GitHubUrl => _githubUrl;

        public override string LocalDirectory => Path.Combine(GitHubAssetsViewModel.GitHubAssetsDownloadDirectory, _owner, _repository, _sha);

        public override string DefaultName => _defaultName;

        /// <summary>
        /// 获取模型对象
        /// </summary>
        public GitHubAssetsRepoModel Model
        {
            get => new()
            {
                Owner = _owner,
                Repository = _repository,
                Sha = _sha,
                Name = Name
            };
            set => Name = value.Name;
        }

        public override IReadOnlyList<GitHubAssetsItemViewModel> Items => _items;
        private List<GitHubAssetsItemViewModel> _items = [];

        /// <summary>
        /// 仓库内所有文件, 键是 <see cref="AssetsItemViewModel.RelativePath"/>
        /// </summary>
        public IReadOnlyDictionary<string, GitHubAssetsItemViewModel> AllItems => _allItems;
        private Dictionary<string, GitHubAssetsItemViewModel> _allItems = [];

        public override bool IsItemsLoaded => _isItemsLoaded;
        private bool _isItemsLoaded = false;

        public override bool IsItemsRefreshing => _isItemsRefreshing;
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
            SetProperty(ref _allItems, [], nameof(AllItems));

            var tree = await GetTreeDataCacheAsync();
            if (tree is null)
                return;

            List<GitHubAssetsItemViewModel> items = [];
            Dictionary<string, GitHubAssetsItemViewModel> allItems = [];

            try
            {
                foreach (var tim in tree.Tree.Where(v => v.Type == Octokit.TreeType.Blob))
                {
                    var item = new GitHubAssetsItemViewModel(this, tim.Path!);
                    var relativePath = item.RelativePath;
                    allItems[relativePath] = item;

                    // 此处严格区分大小写, 因为 GitHub 上文件名是区分大小写的
                    if (SpineObject.PossibleSuffixMapping.Keys.Any(relativePath.EndsWith))
                        items.Add(item);
                }

                SetProperty(ref _isItemsLoaded, true, nameof(IsItemsLoaded));
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to refresh repo '{0}', {1}", Name, ex.Message);
            }

            SetProperty(ref _allItems, allItems, nameof(AllItems));
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

        public sealed override string ToString() => $"GitHubRepo[{_repoKey}]";

        public sealed override bool Equals(object? obj) => obj is GitHubAssetsRepoViewModel other && _repoKey == other._repoKey;

        public sealed override int GetHashCode() => HashCode.Combine(GetType(), _repoKey);

        #region IBrowserOpenable

        string IBrowserOpenable.OpenInBrowserUrl => GitHubUrl;

        #endregion
    }
}
