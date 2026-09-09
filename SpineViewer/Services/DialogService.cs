using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using SpineViewer.Models;
using SpineViewer.ViewModels.Assets;
using SpineViewer.ViewModels.Assets.GitHub;
using SpineViewer.ViewModels.Exporters;
using SpineViewer.Views;
using SpineViewer.Views.AssetsDialogs;
using SpineViewer.Views.ExporterDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Services
{
    /// <summary>
    /// 用于弹出各种对话框的服务
    /// </summary>
    public static class DialogService
    {
        private static bool ShowDialog<TDialog>(object? dc = null) 
            where TDialog : Window, new() 
        {
            var dialog = new TDialog() { Owner = App.Current.MainWindow };
            if (dc is not null) dialog.DataContext = dc;
            return dialog.ShowDialog() ?? false;
        }

        public static bool ShowSystemInfoDialog() => ShowDialog<SystemInfoDialog>();

        public static bool ShowAboutDialog() => ShowDialog<AboutDialog>();

        public static bool ShowGeneratePreviewsDialog(AssetsPreviewViewModel vm) => ShowDialog<GeneratePreviewsDialog>(vm);

        public static bool ShowLocalsAssetEditDialogDialog(LocalAssetsRepoModel vm) => ShowDialog<LocalAssetsRepoEditDialog>(vm);

        public static bool ShowGitHubAddAssetsReposDialog(GitHubAssetsViewModel vm) => ShowDialog<GitHubAddAssetsReposDialog>(vm);

        public static bool ShowFrameExporterDialog(FrameExporterViewModel vm) => ShowDialog<FrameExporterDialog>(vm);

        public static bool ShowPsdExporterDialog(PsdExporterViewModel vm) => ShowDialog<PsdExporterDialog>(vm);

        public static bool ShowFrameSequenceExporterDialog(FrameSequenceExporterViewModel vm) => ShowDialog<FrameSequenceExporterDialog>(vm);

        public static bool ShowFFmpegVideoExporterDialog(FFmpegVideoExporterViewModel vm) => ShowDialog<FFmpegVideoExporterDialog>(vm);

        public static bool ShowCustomFFmpegExporterDialog(CustomFFmpegExporterViewModel vm) => ShowDialog<CustomFFmpegExporterDialog>(vm);

        public static bool ShowPreferenceDialog(PreferenceModel m) => ShowDialog<PreferenceDialog>(m);

        /// <summary>
        /// 获取用户选择的文件
        /// </summary>
        /// <returns>是否确认了选择</returns>
        public static bool ShowOpenFileDialog(out string? fileName, string title = null, string filter = "")
        {
            var dialog = new OpenFileDialog() { Title = title, Filter = filter };
            if (dialog.ShowDialog() is true)
            {
                fileName = dialog.FileName;
                return true;
            }
            fileName = null;
            return false;
        }

        /// <summary>
        /// 获取用户选择的文件夹
        /// </summary>
        /// <returns>是否确认了选择</returns>
        public static bool ShowOpenFolderDialog(out string? folderName)
        {
            var dialog = new OpenFolderDialog() { Multiselect = false };
            if (dialog.ShowDialog() is true)
            {
                folderName = dialog.FolderName;
                return true;
            }
            folderName = null;
            return false;
        }

        public static bool ShowOpenSFMLImageDialog(out string? fileName, string initialDirectory = "")
        {
            var dialog = new OpenFileDialog()
            {
                InitialDirectory = initialDirectory,
                Filter = "SFML Image|*.png;*.jpg;*.jpeg;*.bmp;*.tga|All|*.*"
            };
            if (dialog.ShowDialog() is true)
            {
                fileName = dialog.FileName;
                return true;
            }
            fileName = null;
            return false;
        }

        public static bool ShowOpenJsonDialog(out string? fileName, string initialDirectory = "")
        {
            var dialog = new OpenFileDialog()
            {
                InitialDirectory = initialDirectory,
                Filter = "Json|*.jcfg;*.json|All|*.*"
            };
            if (dialog.ShowDialog() is true)
            {
                fileName = dialog.FileName;
                return true;
            }
            fileName = null;
            return false;
        }

        public static bool ShowSaveJsonDialog(ref string? fileName, string initialDirectory = "")
        {
            var dialog = new SaveFileDialog()
            {
                FileName = fileName,
                InitialDirectory = initialDirectory,
                DefaultExt = ".jcfg",
                Filter = "Json|*.jcfg;*.json|All|*.*",
            };
            if (dialog.ShowDialog() is true)
            {
                fileName = dialog.FileName;
                return true;
            }
            fileName = null;
            return false;
        }
    }
}
