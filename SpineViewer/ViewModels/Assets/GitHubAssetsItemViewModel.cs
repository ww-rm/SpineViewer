using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public class GitHubAssetsItemViewModel : AssetsItemViewModel
    {
        public GitHubAssetsItemViewModel(AssetsRepoViewModel vmRepo, string relativePath) : base(vmRepo, relativePath)
        {
        }
    }
}
