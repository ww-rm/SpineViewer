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
        /// <summary>
        /// 运行 Action 任务并显示进度对话框, 不会阻塞 UI, 但是会阻塞返回
        /// </summary>
        public static void RunAsync(Action<IProgressReporter, CancellationToken> work, string title)
        {
            var vm = new ProgressDialogViewModelAction(work) { Title = title };
            var progressWindow = new ProgressDialog() { DataContext = vm, Owner = App.Current.MainWindow };
            progressWindow.ShowDialog();
        }

        /// <summary>
        /// 运行 Func 任务并显示进度对话框, 不会阻塞 UI, 但是会阻塞返回
        /// </summary>
        public static TResult? RunAsync<TResult>(Func<IProgressReporter, CancellationToken, TResult?> work, string title)
        {
            var vm = new ProgressDialogViewModelFunc<TResult>(work) { Title = title };
            var progressWindow = new ProgressDialog() { DataContext = vm, Owner = App.Current.MainWindow };
            progressWindow.ShowDialog();
            return vm.Result;
        }
    }
}
