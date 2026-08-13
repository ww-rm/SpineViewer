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
        public List<LocalDirectoryModel> LocalDirectories { get; set; } = [];
    }

    public partial class LocalDirectoryModel : ObservableObject
    {
        [ObservableProperty]
        private string _fullPath = "";

        [ObservableProperty]
        private string? _name;
    }
}
