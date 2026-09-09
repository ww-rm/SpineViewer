using Spine;
using SpineViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public sealed class LocalAssetsRepoViewModel : AssetsRepoViewModel<LocalAssetsItemViewModel>
    {
        private readonly string _localDirectory;
        private readonly string _defaultName;

        public LocalAssetsRepoViewModel(string localDirectory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(localDirectory);

            _localDirectory = Path.GetFullPath(localDirectory);
            _defaultName = Path.GetFileName(_localDirectory);
        }

        public LocalAssetsRepoViewModel(LocalAssetsRepoModel m) : this(m.LocalDirectory)
        {
            Name = m.Name;
        }

        /// <summary>
        /// 获取模型对象
        /// </summary>
        public LocalAssetsRepoModel Model
        {
            get => new() { LocalDirectory = _localDirectory, Name = Name };
            set => Name = value.Name;
        }

        public override string LocalDirectory => _localDirectory;

        public override string DefaultName => _defaultName;

        public override IReadOnlyList<LocalAssetsItemViewModel> Items => _items;
        private List<LocalAssetsItemViewModel> _items = [];

        public override bool IsItemsLoaded => _isItemsLoaded;
        private bool _isItemsLoaded = false;

        public override bool IsItemsRefreshing => _isItemsRefreshing;
        private bool _isItemsRefreshing = false;

        protected override Task CreateRefreshItemsTask() => Task.Run(RefreshItemsTask);

        private void RefreshItemsTask()
        {
            // 清空列表并设置状态属性
            SetProperty(ref _isItemsRefreshing, true, nameof(IsItemsRefreshing));
            SetProperty(ref _isItemsLoaded, false, nameof(IsItemsLoaded));
            SetProperty(ref _items, [], nameof(Items));

            List<LocalAssetsItemViewModel> items = [];

            if (!Directory.Exists(_localDirectory))
            {
                _logger.Error("Directory '{0}' is not existed.", _localDirectory);
            }
            else
            {
                try
                {
                    foreach (var path in Directory.EnumerateFiles(_localDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        var lowerPath = path.ToLowerInvariant();

                        if (SpineObject.PossibleSuffixMapping.Keys.Any(lowerPath.EndsWith))
                        {
                            var relativePath = Path.GetRelativePath(_localDirectory, path);
                            items.Add(new(this, relativePath));
                        }
                    }
                    SetProperty(ref _isItemsLoaded, true, nameof(IsItemsLoaded));
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to enumerate files in dir: {0}, {1}", _localDirectory, ex.Message);
                }
            }

            SetProperty(ref _items, items, nameof(Items));
            SetProperty(ref _isItemsRefreshing, false, nameof(IsItemsRefreshing));
        }

        public sealed override string ToString() => $"LocalRepo[{_localDirectory}]";

        public sealed override bool Equals(object? obj) => obj is LocalAssetsRepoViewModel other && _localDirectory == other._localDirectory;

        public sealed override int GetHashCode() => HashCode.Combine(GetType(), _localDirectory);
    }
}
