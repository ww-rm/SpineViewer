using CommunityToolkit.Mvvm.Input;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shell;

namespace SpineViewer.ViewModels.Assets
{
    public partial class GitHubAssetsViewModel : AssetsViewModel<GitHubAssetsRepoViewModel, GitHubAssetsItemViewModel>
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
            var lines = "ww-rm/azurlane_spinepainting@d37b5bd58b1140c2395bb2d22cf9bc80fda504d5\nww-rm/azurlane_char\n";
            lines = string.Concat(Enumerable.Repeat(lines, 20));

            var records = GetGitHubRepositoryRecords(lines);
            if (records.Count <= 0)
                return [];

            // 使用进度对话框前台添加, 仓库提交信息获取完整后才视作有效仓库
            var result = ProgressService.RunAsync(
                (pr, ct) => GetAssetsRepoTask(records, pr, ct).Result,
                AppResource.Str_AddGitHubAssetsReposTitle
            );
            return result ?? [];
        }

        private async Task<List<GitHubAssetsRepoViewModel>> GetAssetsRepoTask(List<GitHubRepositoryRecord> records, IProgressReporter reporter, CancellationToken ct)
        {
            int totalCount = records.Count;
            int success = 0;
            int error = 0;

            _vmMain.ProgressState = TaskbarItemProgressState.Normal;
            _vmMain.ProgressValue = 0;

            reporter.Total = totalCount;
            reporter.Done = 0;
            reporter.ProgressText = $"[0/{totalCount}]";

            var client = GitHubService.GetClient();
            List<GitHubAssetsRepoViewModel> repos = [];

            for (int i = 0; i < totalCount; i++)
            {
                if (ct.IsCancellationRequested) break;

                var r = records[i];
                reporter.ProgressText = $"[{i}/{totalCount}] {r}";

                try
                {
                    var @ref = r.Ref;
                    if (string.IsNullOrWhiteSpace(@ref))
                    {
                        var repoInfo = await client.Repository.Get(r.Owner, r.Repository);
                        @ref = repoInfo.DefaultBranch;
                    }

                    var commitInfo = await client.Repository.Commit.Get(r.Owner, r.Repository, @ref);
                    var sha = commitInfo.Sha;

                    repos.Add(new(r.Owner, r.Repository, sha));

                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to get repository info '{0}', {1}", r, ex.Message);
                    error++;
                }

                reporter.Done = i + 1;
                reporter.ProgressText = $"[{i + 1}/{totalCount}] {r}";
                _vmMain.ProgressValue = (i + 1f) / totalCount;
            }
            _vmMain.ProgressState = TaskbarItemProgressState.None;

            if (error > 0)
                _logger.Warn("Get GitHub repos {0} successfully, {1} failed", success, error);
            else
                _logger.Info("Get GitHub repos {0} successfully", success);

            client.LogRateLimit();

            return repos;
        }

        [GeneratedRegex(@"^(?<owner>[\w.-]+)/(?<repository>[\w.-]+)(?:@(?<ref>\S+))?$")]
        private static partial Regex GitHubRepositoryRecordRegex();

        private record GitHubRepositoryRecord(string Owner, string Repository, string? Ref)
        {
            public override string ToString() => string.IsNullOrWhiteSpace(Ref) ? $"{Owner}/{Repository}" : $"{Owner}/{Repository}@{Ref}";
        }

        private static List<GitHubRepositoryRecord> GetGitHubRepositoryRecords(string lines)
        {
            List<GitHubRepositoryRecord> records = [];

            foreach (var line in lines.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()))
            {
                var match = GitHubRepositoryRecordRegex().Match(line);

                if (!match.Success)
                {
                    _logger.Info("Ignore input line: '{0}'", JsonSerializer.Serialize(line));
                    continue;
                }

                records.Add(new(
                    match.Groups["owner"].Value,
                    match.Groups["repository"].Value,
                    match.Groups["ref"].Success ? match.Groups["ref"].Value : null
                ));
            }

            return records;
        }

        protected override bool EditAssetsRepo(GitHubAssetsRepoViewModel repo)
        {
            // 编辑名字
            _logger.Warn("NotImplemented");
            return false;
        }

        // TODO: 下载资源
    }
}
