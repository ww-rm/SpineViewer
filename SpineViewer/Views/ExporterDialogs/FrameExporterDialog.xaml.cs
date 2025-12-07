using SpineViewer.Extensions;
using SpineViewer.Resources;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpineViewer.Views.ExporterDialogs
{
    /// <summary>
    /// FrameExportDialog.xaml 的交互逻辑
    /// </summary>
    public partial class FrameExporterDialog : Window
    {
        public FrameExporterDialog()
        {
            InitializeComponent();
            SourceInitialized += FrameExporterDialog_SourceInitialized;
        }

        private void FrameExporterDialog_SourceInitialized(object? sender, EventArgs e)
        {
            this.SetWindowTextColor(AppResource.Color_PrimaryText);
            this.SetWindowCaptionColor(AppResource.Color_Region);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var vm = (FrameExporterViewModel)DataContext;
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

        private void ButtonPickColor_Click(object sender, RoutedEventArgs e)
        {
            _colorPopup.IsOpen = !_colorPopup.IsOpen;
        }

        private void ColorPicker_Confirmed(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            _colorPopup.IsOpen = false;
            var color = e.Info;
            var vm = (BaseExporterViewModel)DataContext;
            vm.BackgroundColor = color;
        }

        private void ColorPicker_Canceled(object sender, EventArgs e)
        {
            _colorPopup.IsOpen = false;
        }
    }
}
