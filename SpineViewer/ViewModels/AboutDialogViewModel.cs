using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels
{
    public partial class AboutDialogViewModel : ObservableObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string ProgramTagName { get; } = $"v{App.Version}";

        public string ProjectUrl { get; } = $"https://github.com/{App.GithubOwner}/{App.GithubRepo}";

        [ObservableProperty]
        private string _latestReleaseTagName = "";

        [ObservableProperty]
        private string _latestReleaseUrl = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(Cmd_CheckUpdates))]
        public bool _isCheckingUpdates = false;

        /// <summary>
        /// 打开指定网址
        /// </summary>
        public RelayCommand<string?> Cmd_OpenUrl => _cmd_OpenUrl ??= new(url =>
        {
            if (string.IsNullOrEmpty(url))
                return;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        });
        private RelayCommand<string?>? _cmd_OpenUrl;

        /// <summary>
        /// 检查更新
        /// </summary>
        public RelayCommand Cmd_CheckUpdates => _cmd_CheckUpdates ??= new(CheckUpdates_Execute, () => !IsCheckingUpdates);
        private RelayCommand? _cmd_CheckUpdates;

        public async void CheckUpdates_Execute()
        {
            LatestReleaseTagName = "";
            LatestReleaseUrl = "";

            IsCheckingUpdates = true;
            try
            {
                var res = await App.GitHubClient.Repository.Release.GetLatest(App.GithubOwner, App.GithubRepo);
                if (res is not null)
                {
                    LatestReleaseTagName = res.TagName;
                    LatestReleaseUrl = res.HtmlUrl;
                }
                else
                {
                    LatestReleaseTagName = "Failed to get tag name.";
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to check updates, {0}", ex.Message);
                LatestReleaseTagName = ex.Message;
            }
            IsCheckingUpdates = false;
        }
    }
}
