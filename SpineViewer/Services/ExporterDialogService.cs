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
    public static class ExporterDialogService
    {
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
    }
}
