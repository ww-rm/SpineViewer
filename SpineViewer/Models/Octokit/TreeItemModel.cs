using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Models.Octokit
{
    public class TreeItemModel
    {
        public TreeItemModel() { }

        public TreeItemModel(TreeItem treeItem)
        {
            Path = treeItem.Path;
            Mode = treeItem.Mode;
            Type = treeItem.Type.Value;
            Size = treeItem.Size;
            Sha = treeItem.Sha;
            Url = treeItem.Url;
        }

        public string? Path { get; set; }

        public string? Mode { get; set; }

        public TreeType Type { get; set; }

        public int Size { get; set; }

        public string? Sha { get; set; }

        public string? Url { get; set; }
    }
}
