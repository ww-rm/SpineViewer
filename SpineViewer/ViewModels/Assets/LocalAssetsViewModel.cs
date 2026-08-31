using NLog;
using SpineViewer.Models;
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
    public sealed class LocalAssetsViewModel : AssetsViewModel<LocalAssetsRepoViewModel, LocalAssetsItemViewModel>
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        public static readonly string LocalAssetsFilePath = Path.Combine(App.ProcessDataDirectory, "localassets.json");

        public LocalAssetsViewModel(MainWindowViewModel vmMain) : base(vmMain)
        {

        }

        protected override LocalAssetsRepoViewModel? AddAssetsRepo()
        {
            if (!DialogService.ShowOpenFolderDialog(out var selectedPath))
                return null;
            return new(selectedPath!);
        }

        protected override bool EditAssetsRepo(LocalAssetsRepoViewModel repo)
        {
            var m = repo.Model;
            if (!DialogService.ShowLocalAssetEditDialogDialog(m))
                return false;

            repo.Model = m;
            return true;
        }

        public override void LoadAssetsRepos()
        {
            // 先清空列表
            _selectedAssetsRepo = null;
            RefreshShownItemsAsync().Wait();
            _assetsRepos.Clear();

            if (JsonHelper.Deserialize<LocalAssetsModel>(LocalAssetsFilePath, out var assets, true))
            {
                foreach (var m in assets.LocalAssetsRepos)
                {
                    _assetsRepos.Add(new(m.LocalDirectory) { Model = m });
                }
            }
        }

        public override void SaveAssetsRepos()
        {
            var m = new LocalAssetsModel();

            foreach (var repo in _assetsRepos)
            {
                m.LocalAssetsRepos.Add(repo.Model);
            }

            JsonHelper.Serialize(m, LocalAssetsFilePath);
        }
    }
}
