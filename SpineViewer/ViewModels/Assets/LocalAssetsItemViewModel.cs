using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public sealed class LocalAssetsItemViewModel : AssetsItemViewModel
    {
        private readonly LocalAssetsRepoViewModel _vmRepo;

        public LocalAssetsItemViewModel(LocalAssetsRepoViewModel vmRepo, string relativePath) : base(vmRepo, relativePath)
        {
            _vmRepo = vmRepo;
        }
    }
}
