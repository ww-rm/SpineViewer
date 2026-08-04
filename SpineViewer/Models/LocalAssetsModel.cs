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

        public class LocalDirectoryModel
        {
            public string FullPath { get; set; } = "";

            public string? Name { get; set; }
        }
    }
}
