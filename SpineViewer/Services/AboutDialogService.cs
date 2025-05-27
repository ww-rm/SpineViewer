using SpineViewer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Services
{
    public static class AboutDialogService
    {
        public static bool ShowAboutDialog()
        {
            var dialog = new AboutDialog() { Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }
    }
}
