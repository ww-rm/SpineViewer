using SpineViewer.ViewModels;
using SpineViewer.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Services
{
    public interface IProgressReporter
    {
        public float Total { get; set; }

        public float Done { get; set; }

        public string ProgressText { get; set; }
    }

    public static class ProgressService
    {
        public static void RunAsync(Action<IProgressReporter, CancellationToken> work, string title)
        {
            var vm = new ProgressDialogViewModel(work) { Title = title };
            var progressWindow = new ProgressDialog() { DataContext = vm, Owner = App.Current.MainWindow };
            progressWindow.ShowDialog();
        }
    }
}
