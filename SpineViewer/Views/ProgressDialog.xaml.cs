using SpineViewer.Extensions;
using SpineViewer.Resources;
using SpineViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using Win32Natives;

namespace SpineViewer.Views
{
    /// <summary>
    /// ProgressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public ProgressDialog()
        {
            InitializeComponent();
            SourceInitialized += ProgressDialog_SourceInitialized;
            Loaded += ProgressWindow_Loaded;
        }

        private void ProgressDialog_SourceInitialized(object? sender, EventArgs e)
        {
            this.SetWindowTextColor(AppResource.Color_PrimaryText);
            this.SetWindowCaptionColor(AppResource.Color_Region);
        }

        private void ProgressWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int currentStyle = User32.GetWindowLong(hwnd, User32.GWL_STYLE);
            User32.SetWindowLong(hwnd, User32.GWL_STYLE, currentStyle & ~User32.WS_SYSMENU);

            var vm = (ProgressDialogViewModel)DataContext;
            vm.WorkFinished += (s, e) => Dispatcher.Invoke(() => { DialogResult = e; });
            vm.Start();
        }
    }
}
