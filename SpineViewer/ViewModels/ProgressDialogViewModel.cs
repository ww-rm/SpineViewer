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
    public abstract partial class ProgressDialogViewModel : ObservableObject, IProgressReporter, IDisposable
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource _cts = new();
        private readonly Task _task;

        public ProgressDialogViewModel()
        {
            _task = new(() =>
            {
                try
                {
                    DoWork(_cts.Token);
                    OnCompleted(true);
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("Work cancelled by user: {0}", Title);
                    OnCompleted(false);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to finish work: {0}, {1}", Title, ex.Message);
                    OnCompleted(false);
                }
            });
        }

        public event EventHandler<bool>? Completed;

        public void Start() => _task.Start();

        protected abstract void DoWork(CancellationToken ct);

        protected virtual void OnCompleted(bool isCompletedSuccessfully)
        {
            IsCompleted = true;
            IsCompletedSuccessfully = isCompletedSuccessfully;
            Completed?.Invoke(this, isCompletedSuccessfully);
        }

        [ObservableProperty]
        private string _title = "Progress";

        [ObservableProperty]
        private float _total = 100;

        [ObservableProperty]
        private float _done = 0;

        [ObservableProperty]
        private string _progressText = "Working...";

        [ObservableProperty]
        private bool _isCompleted = false;

        [ObservableProperty]
        private bool _isCompletedSuccessfully = false;

        public RelayCommand Cmd_Cancel => _cmd_Cancel ??= new(Cancel_Execute, Cancel_CanExecute);
        private RelayCommand? _cmd_Cancel;

        private void Cancel_Execute()
        {
            if (!Cancel_CanExecute()) return;
            if (!MessagePopupService.OKCancel(AppResource.Str_CancelQuest)) return;
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

    public partial class ProgressDialogViewModelAction : ProgressDialogViewModel
    {
        private readonly Action<IProgressReporter, CancellationToken> _work;

        public ProgressDialogViewModelAction(Action<IProgressReporter, CancellationToken> work)
        {
            _work = work;
        }

        protected override void DoWork(CancellationToken ct)
        {
            _work.Invoke(this, ct);
        }
    }

    public partial class ProgressDialogViewModelFunc<TResult> : ProgressDialogViewModel
    {
        private readonly Func<IProgressReporter, CancellationToken, TResult?> _work;

        public ProgressDialogViewModelFunc(Func<IProgressReporter, CancellationToken, TResult?> work)
        {
            _work = work;
        }

        protected override void DoWork(CancellationToken ct)
        {
            Result = _work.Invoke(this, ct);
        }

        public TResult? Result { get; private set; }
    }
}
