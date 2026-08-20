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
        [NotifyCanExecuteChangedFor(nameof(Cmd_CheckUpdates))]
        public bool _isCheckingUpdates = false;

        /// <summary>
        /// 打开项目地址
        /// </summary>
        public RelayCommand Cmd_OpenProjectUrl => _cmd_OpenProjectUrl ??= new(() =>
        {
            Process.Start(new ProcessStartInfo(ProjectUrl) { UseShellExecute = true });
        });
        private RelayCommand? _cmd_OpenProjectUrl;

        /// <summary>
        /// 检查更新
        /// </summary>
        public RelayCommand Cmd_CheckUpdates => _cmd_CheckUpdates ??= new(CheckUpdates_Execute, () => !IsCheckingUpdates);
        private RelayCommand? _cmd_CheckUpdates;

        public async void CheckUpdates_Execute()
        {
            LatestReleaseTagName = "";

            IsCheckingUpdates = true;
            try
            {
                var res = await App.GitHubClient.Repository.Release.GetLatest(App.GithubOwner, App.GithubRepo);
                if (res?.TagName is string tagName)
                    LatestReleaseTagName = tagName;
                else
                    LatestReleaseTagName = "Failed to get tag name.";
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
