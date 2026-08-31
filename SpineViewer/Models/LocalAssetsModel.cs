using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Models
{
    public class LocalAssetsModel
    {
        public List<LocalAssetsRepoModel> LocalAssetsRepos { get; set; } = [];
    }

    public partial class LocalAssetsRepoModel : ObservableObject
    {
        [ObservableProperty]
        private string _localDirectory = "";

        [ObservableProperty]
        private string? _name;
    }
}
