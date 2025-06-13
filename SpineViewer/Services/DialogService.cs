using Microsoft.Win32;
using SpineViewer.Models;
using SpineViewer.ViewModels.Exporters;
using SpineViewer.Views;
using SpineViewer.Views.ExporterDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Services
{
    /// <summary>
    /// 用于弹出各种对话框的服务
    /// </summary>
    public static class DialogService
    {
        public static bool ShowDiagnosticsDialog()
        {
            var dialog = new DiagnosticsDialog() { Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }

        public static bool ShowAboutDialog()
        {
            var dialog = new AboutDialog() { Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }

        public static bool ShowFrameExporterDialog(FrameExporterViewModel vm)
        {
            var dialog = new FrameExporterDialog() { DataContext = vm, Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }

        public static bool ShowFrameSequenceExporterDialog(FrameSequenceExporterViewModel vm)
        {
            var dialog = new FrameSequenceExporterDialog() { DataContext = vm, Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }

        public static bool ShowFFmpegVideoExporterDialog(FFmpegVideoExporterViewModel vm)
        {
            var dialog = new FFmpegVideoExporterDialog() { DataContext = vm, Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }

        public static bool ShowCustomFFmpegExporterDialog(CustomFFmpegExporterViewModel vm)
        {
            var dialog = new CustomFFmpegExporterDialog() { DataContext = vm, Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }

        /// <summary>
        /// 将给定的首选项参数在对话框上进行显示, 返回值表示是否确认修改
        /// </summary>
        public static bool ShowPreferenceDialog(PreferenceModel m)
        {
            var dialog = new PreferenceDialog() { DataContext = m, Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }

        /// <summary>
        /// 获取用户选择的文件夹
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>是否确认了选择</returns>
        public static bool ShowOpenFolderDialog(out string? folderName)
        {
            // XXX: 此处使用了 System.Windows.Forms 的文件夹浏览对话框
            var folderDialog = new OpenFolderDialog() { Multiselect = false };
            if (folderDialog.ShowDialog() is true)
            {
                folderName = folderDialog.FolderName;
                return true;
            }
            folderName = null;
            return false;
        }

        /// <summary>
        /// 获取用户选择的文件夹
        /// </summary>
        /// <param name="selectedPath"></param>
        /// <returns>是否确认了选择</returns>
        public static bool ShowSaveFileDialog(out string? selectedPath)
        {
            var dialog = new SaveFileDialog() { };
            selectedPath = null;
            // TODO
            return false;
        }
    }
}
