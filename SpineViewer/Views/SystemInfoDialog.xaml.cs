using SpineViewer.Extensions;
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
    /// SystemInfoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SystemInfoDialog : Window
    {
        public SystemInfoDialog()
        {
            InitializeComponent();
            SourceInitialized += SystemInfoDialog_SourceInitialized;
        }

        private void SystemInfoDialog_SourceInitialized(object? sender, EventArgs e)
        {
            this.SetWindowTextColor(AppResource.Color_PrimaryText);
            this.SetWindowCaptionColor(AppResource.Color_Region);
        }
    }
}
