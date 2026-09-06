using CommunityToolkit.Mvvm.Input;
using SpineViewer.Models;
using SpineViewer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public class GitHubAssetsViewModel : AssetsViewModel<GitHubAssetsRepoViewModel, GitHubAssetsItemViewModel>
    {
        public const string GitHubUrlHost = "github.com";
        public const string GitHubRawUrlHost = "raw.githubusercontent.com";

        /// <summary>
        /// 文件保存路径
        /// </summary>
        public static readonly string GitHubAssetsFilePath = Path.Combine(App.DataDirectory, "githubassets.json");

        public static readonly string GitHubAssetsCacheDirectory = Path.Combine(AssetsCacheDirectory, "github");

        public static string GitHubAssetsDownloadDirectory => Path.Combine(AssetsDownloadDirectory, "github");

        public GitHubAssetsViewModel(MainWindowViewModel vmMain) : base(vmMain) { }

        /// <summary>
        /// 在浏览器中打开资源库或资源文件
        /// </summary>
        public RelayCommand<IList?> Cmd_OpenAssetsInBrowser => _cmd_OpenAssetsInBrowser ??= new(OpenAssetsInBrowser_Execute, CommandCanExecute.OnlyOne);
        private RelayCommand<IList?>? _cmd_OpenAssetsInBrowser;

        private void OpenAssetsInBrowser_Execute(IList? args)
        {
            if (!CommandCanExecute.OnlyOne(args)) return;

            var obj = (IBrowserOpenable)args[0]!;
            obj.OpenUrlInBroswer();
        }

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
