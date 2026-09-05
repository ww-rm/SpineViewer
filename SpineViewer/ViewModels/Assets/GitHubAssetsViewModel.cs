using SpineViewer.Models;
using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public class GitHubAssetsViewModel : AssetsViewModel<GitHubAssetsRepoViewModel, GitHubAssetsItemViewModel>
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        public static readonly string GitHubAssetsFilePath = Path.Combine(App.DataDirectory, "githubassets.json");

        public static readonly string GitHubAssetsCacheDirectory = Path.Combine(AssetsCacheDirectory, "github");

        public GitHubAssetsViewModel(MainWindowViewModel vmMain) : base(vmMain) { }

        public override void LoadAssetsRepos()
        {
            _assetsRepos.Clear();
            if (JsonHelper.Deserialize<GitHubAssetsModel>(GitHubAssetsFilePath, out var assets, true))
            {
                foreach (var m in assets.GitHubAssetsRepos)
                {
                    _assetsRepos.Add(new(m));
                }
            }
        }

        public override void SaveAssetsRepos()
        {
            var m = new GitHubAssetsModel();

            foreach (var repo in _assetsRepos)
            {
                m.GitHubAssetsRepos.Add(repo.Model);
            }

            JsonHelper.Serialize(m, GitHubAssetsFilePath);
        }

        protected override IReadOnlyList<GitHubAssetsRepoViewModel> AddAssetsRepos()
        {
            // TODO: 多行文本解析
            // 挂 ProgressDialog 前台加载, 仓库提交信息获取完整后才视作有效仓库
            _logger.Warn("NotImplemented");
            return null;
        }

        protected override bool EditAssetsRepo(GitHubAssetsRepoViewModel repo)
        {
            // 编辑名字
            _logger.Warn("NotImplemented");
            return false;
        }
    }
}
