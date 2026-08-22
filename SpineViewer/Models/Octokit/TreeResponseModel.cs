using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Models.Octokit
{
    public class TreeResponseModel
    {
        public TreeResponseModel() { }

        public TreeResponseModel(TreeResponse treeResponse)
        {
            Sha = treeResponse.Sha;
            Url = treeResponse.Url;
            Tree = treeResponse.Tree.Select(v => new TreeItemModel(v)).ToList();
            Truncated = treeResponse.Truncated;
        }

        public string? Sha { get; set; }

        public string? Url { get; set; }

        public List<TreeItemModel>? Tree { get; set; }

        public bool Truncated { get; set; }
    }
}
