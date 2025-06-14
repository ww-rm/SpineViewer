using SpineViewer.Resources;
using SpineViewer.ViewModels.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Exporters
{
    public abstract class VideoExporterViewModel(MainWindowViewModel vmMain) : BaseExporterViewModel(vmMain)
    {
        public float Duration { get => _duration; set => SetProperty(ref _duration, value); }
        protected float _duration = -1;

        public uint Fps { get => _fps; set => SetProperty(ref _fps, Math.Max(1, value)); }
        protected uint _fps = 30;

        public bool KeepLast { get => _keepLast; set => SetProperty(ref _keepLast, value); }
        protected bool _keepLast = true;
    }
}
