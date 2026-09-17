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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpineViewer.Views.AssetsDialogs
{
    /// <summary>
    /// AddGitHubAssetsReposDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddGitHubAssetsReposDialog : Window
    {
        public AddGitHubAssetsReposDialog()
        {
            InitializeComponent();
            SourceInitialized += GitHubAddAssetsReposDialog_SourceInitialized;
        }

        private void GitHubAddAssetsReposDialog_SourceInitialized(object? sender, EventArgs e)
        {
            this.SetWindowTextColor(AppResource.Color_PrimaryText);
            this.SetWindowCaptionColor(AppResource.Color_Region);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
