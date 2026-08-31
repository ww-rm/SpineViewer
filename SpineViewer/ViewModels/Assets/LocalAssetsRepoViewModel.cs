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
        public LocalAssetsRepoViewModel(string localDirectory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(localDirectory);

            _localDirectory = Path.GetFullPath(localDirectory);
            _defaultName = Path.GetFileName(_localDirectory);
        }

        /// <summary>
        /// 获取模型对象
        /// </summary>
        public LocalAssetsRepoModel Model
        {
            get => new() { LocalDirectory = LocalDirectory, Name = Name };
            set => Name = value.Name;
        }

        public override string LocalDirectory { get => _localDirectory; }
        private readonly string _localDirectory;

        public override string DefaultName { get => _defaultName; }
        private readonly string _defaultName;

        public override IReadOnlyList<LocalAssetsItemViewModel> Items => _items;
        private List<LocalAssetsItemViewModel> _items = [];

        public override bool IsItemsRefreshing { get => _isItemsRefreshing; }
        private bool _isItemsRefreshing = false;

        private Task? _itemsRefreshingTask;

        public override async Task RefreshItemsAsync()
        {
            if (_itemsRefreshingTask is null || _itemsRefreshingTask.IsCompleted)
            {
                _itemsRefreshingTask = Task.Run(RefreshItemsTask);
            }
            await _itemsRefreshingTask;
        }

        private void RefreshItemsTask()
        {
            _isItemsRefreshing = true;
            OnPropertyChanged(nameof(IsItemsRefreshing));

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
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to enumerate files in dir: {0}, {1}", _localDirectory, ex.Message);
                }
            }

            _items = items;

            _isItemsRefreshing = false;
            OnPropertyChanged(nameof(IsItemsRefreshing));
        }
    }
}
