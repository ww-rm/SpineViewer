using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using SpineViewer.Resources;
using SpineViewer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels
{
    public partial class ProgressDialogViewModel : ObservableObject, IProgressReporter, IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource _cts = new();
        private readonly Task _task;

        public ProgressDialogViewModel(Action<IProgressReporter, CancellationToken> work)
        {
            _task = new(() =>
            {
                try
                {
                    work(this, _cts.Token);
                    WorkFinished?.Invoke(this, true);
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("Work cancelled by user: {0}", _title);
                    WorkFinished?.Invoke(this, false);
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to finish work: {0}, {1}", _title, ex.Message);
                    WorkFinished?.Invoke(this, false);
                }
            });
        }

        public void Start() => _task.Start();

        public event EventHandler<bool>? WorkFinished;

        [ObservableProperty]
        private string _title = "Progress";

        [ObservableProperty]
        private float _total = 100;

        [ObservableProperty]
        private float _done = 0;

        [ObservableProperty]
        private string _progressText = "Working...";

        public RelayCommand Cmd_Cancel => _cmd_Cancel ??= new(Cancel_Execute, Cancel_CanExecute);
        private RelayCommand? _cmd_Cancel;

        private void Cancel_Execute()
        {
            if (!MessagePopupService.Quest(AppResource.Str_CancelQuest)) return;
            _cts.Cancel();
            Cmd_Cancel.NotifyCanExecuteChanged();
        }

        private bool Cancel_CanExecute() => !_cts.IsCancellationRequested;

        #region IDisposable 接口实现

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _cts.Dispose();
                _task.Dispose();
            }
            _disposed = true;
        }

        ~ProgressDialogViewModel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            if (_disposed)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
