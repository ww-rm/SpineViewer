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

        /// <inheritdoc cref="TreeItem.Path"/>
        public string? Path { get; set; }

        /// <inheritdoc cref="TreeItem.Mode"/>
        public string? Mode { get; set; }

        /// <inheritdoc cref="TreeItem.Type"/>
        public TreeType Type { get; set; }

        /// <inheritdoc cref="TreeItem.Size"/>
        public int Size { get; set; }

        /// <inheritdoc cref="TreeItem.Sha"/>
        public string? Sha { get; set; }

        /// <inheritdoc cref="TreeItem.Url"/>
        public string? Url { get; set; }
    }
}
