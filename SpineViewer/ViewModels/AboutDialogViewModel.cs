using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels
{
    public class AboutDialogViewModel : ObservableObject
    {
        public string ProgramVersion => $"v{App.Version}";

        public string ProjectUrl => "https://github.com/ww-rm/SpineViewer";

        public RelayCommand Cmd_OpenProjectUrl => _cmd_OpenProjectUrl ??= new(() =>
        {
            Process.Start(new ProcessStartInfo(ProjectUrl) { UseShellExecute = true });
        });
        private RelayCommand? _cmd_OpenProjectUrl;
    }
}
