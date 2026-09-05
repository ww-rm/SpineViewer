using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Models
{
    public class GitHubAssetsModel
    {
        public List<GitHubAssetsRepoModel> GitHubAssetsRepos { get; set; } = [];
    }

    public partial class GitHubAssetsRepoModel : ObservableObject
    {
        [ObservableProperty]
        private string _owner = "";

        [ObservableProperty]
        private string _repository = "";

        [ObservableProperty]
        private string _sha = "";

        [ObservableProperty]
        private string? _name;
    }
}
