using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public class GitHubAssetsViewModel : AssetsViewModel<GitHubAssetsRepoViewModel, GitHubAssetsItemViewModel>
    {
        public static new readonly string CacheDirectory = Path.Combine(AssetsViewModel.CacheDirectory, "github");

        public GitHubAssetsViewModel(MainWindowViewModel vmMain) : base(vmMain)
        {
        }

        public override void LoadAssetsRepos()
        {
            _assetsRepos.Clear();
            _assetsRepos.Add(new("ww-rm", "azurlane_char", "82bfb06b815815ef17e5ef21d267d461d1b6d0b7"));
            _assetsRepos.Add(new("ww-rm", "azurlane_spinepainting", "d37b5bd58b1140c2395bb2d22cf9bc80fda504d5"));
            _logger.Warn("NotImplemented");
        }

        public override void SaveAssetsRepos()
        {
            _logger.Warn("NotImplemented");
        }

        protected override GitHubAssetsRepoViewModel? AddAssetsRepo()
        {
            _logger.Warn("NotImplemented");
            return null;
        }

        protected override bool EditAssetsRepo(GitHubAssetsRepoViewModel repo)
        {
            _logger.Warn("NotImplemented");
            return false;
        }
    }
}
