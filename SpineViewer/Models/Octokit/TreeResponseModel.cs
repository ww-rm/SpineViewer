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

        /// <summary>
        /// 获取响应的时间
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <inheritdoc cref="TreeResponse.Sha"/>
        public string? Sha { get; set; }

        /// <inheritdoc cref="TreeResponse.Url"/>
        public string? Url { get; set; }

        /// <inheritdoc cref="TreeResponse.Tree"/>
        public List<TreeItemModel>? Tree { get; set; }

        /// <inheritdoc cref="TreeResponse.Truncated"/>
        public bool Truncated { get; set; }
    }
}
