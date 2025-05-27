using SpineViewer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Services
{
    public static class DiagnosticsDialogService
    {
        public static bool ShowDiagnosticsDialog()
        {
            var dialog = new DiagnosticsDialog() { Owner = App.Current.MainWindow };
            return dialog.ShowDialog() ?? false;
        }
    }
}
