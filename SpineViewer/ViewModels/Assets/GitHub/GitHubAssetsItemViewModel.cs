using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets.GitHub
{
    public class GitHubAssetsItemViewModel : AssetsItemViewModel, IBrowserOpenable
    {
        private readonly GitHubAssetsRepoViewModel _vmRepo;

        public GitHubAssetsItemViewModel(GitHubAssetsRepoViewModel vmRepo, string relativePath) : base(vmRepo, relativePath)
        {
            _vmRepo = vmRepo;
        }

        /// <summary>
        /// 原始数据下载地址
        /// </summary>
        public string GitHubRawUrl => string.Format(
            "https://{0}/{1}/{2}/{3}/{4}",
            GitHubAssetsViewModel.GitHubRawUrlHost,
            _vmRepo.Owner,
            _vmRepo.Repository,
            _vmRepo.Sha,
            _relativePath.Replace("\\", "/")
        );

        public string GitHubUrl => string.Format(
            "https://{0}/{1}/{2}/blob/{3}/{4}",
            GitHubAssetsViewModel.GitHubUrlHost,
            _vmRepo.Owner,
            _vmRepo.Repository,
            _vmRepo.Sha,
            _relativePath.Replace("\\", "/")
        );

        #region IBrowserOpenable

        string IBrowserOpenable.OpenInBrowserUrl => GitHubUrl;

        #endregion
    }
}
