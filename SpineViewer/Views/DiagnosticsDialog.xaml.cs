using SpineViewer.Natives;
using SpineViewer.Resources;
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

namespace SpineViewer.Views
{
    /// <summary>
    /// DiagnosticsDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DiagnosticsDialog : Window
    {
        public DiagnosticsDialog()
        {
            InitializeComponent();
            SourceInitialized += DiagnosticsDialog_SourceInitialized;
        }

        private void DiagnosticsDialog_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            Dwmapi.SetWindowTextColor(hwnd, AppResource.Color_PrimaryText);
            Dwmapi.SetWindowCaptionColor(hwnd, AppResource.Color_Region);
        }
    }
}
