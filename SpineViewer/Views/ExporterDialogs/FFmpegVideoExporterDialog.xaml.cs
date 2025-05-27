using SpineViewer.Services;
using SpineViewer.ViewModels.Exporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpineViewer.Views.ExporterDialogs
{
    /// <summary>
    /// FFmpegVideoExporterViewModel.xaml 的交互逻辑
    /// </summary>
    public partial class FFmpegVideoExporterDialog : Window
    {
        public FFmpegVideoExporterDialog()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var vm = (FFmpegVideoExporterViewModel)DataContext;
            if (vm.Validate() is string err)
            {
                MessagePopupService.Error(err);
                return;
            }
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonSelectOutputDir_Click(object sender, RoutedEventArgs e)
        {
            if (OpenFolderService.OpenFolder(out var selectedPath))
            {
                var vm = (FFmpegVideoExporterViewModel)DataContext;
                vm.OutputDir = selectedPath;
            }
        }

        private void ButtonPickColor_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
