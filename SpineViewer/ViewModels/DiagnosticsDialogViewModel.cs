using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SpineViewer.Resources;
using SpineViewer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.ViewModels
{
    public class DiagnosticsDialogViewModel : ObservableObject
    {
        public string CPU => Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0", 
            "ProcessorNameString", 
            "Unknown"
        ).ToString();

        public string GPU
        {
            get
            {
                var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                return string.Join("; ", searcher.Get().Cast<ManagementObject>().Select(mo => mo["Name"].ToString()));
            }
        }

        public string Memory => $"{new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024f / 1024f / 1024f:F1} GB";

        public string WindowsVersion
        {
            get
            {
                var registryKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                var productName = Registry.GetValue(registryKeyPath, "ProductName", "Unknown") as string;
                var editionId = Registry.GetValue(registryKeyPath, "EditionID", "Unknown") as string;
                var osVersion = Environment.OSVersion.ToString();
                return $"{productName}, {editionId}, {osVersion}";
            }
        }

        public string DotNetVersion => Environment.Version.ToString();

        public string ProgramVersion => App.Version;

        public string NLogVersion => typeof(NLog.Logger).Assembly.GetName().Version.ToString();

        public string SFMLVersion => typeof(SFML.ObjectBase).Assembly.GetName().Version.ToString();

        public string FFMpegCoreVersion => typeof(FFMpegCore.FFMpeg).Assembly.GetName().Version.ToString();

        public string SkiaSharpVersion => typeof(SkiaSharp.SkiaSharpVersion).Assembly.GetName().Version.ToString();

        public string HandyControlVersion => typeof(HandyControl.Themes.Theme).Assembly.GetName().Version.ToString();

        public RelayCommand Cmd_CopyToClipboard => _cmd_CopyToClipboard ??= new(() =>
        {
            var result = string.Join(Environment.NewLine, [
                $"CPU\t{CPU}",
                $"GPU\t{GPU}",
                $"Memory\t{Memory}",
                $"WindowsVersion\t{WindowsVersion}",
                $"DotNetVersion\t{DotNetVersion}",
                $"ProgramVersion\t{ProgramVersion}",
                $"NLogVersion\t{NLogVersion}",
                $"SFMLVersion\t{SFMLVersion}",
                $"FFMpegCoreVersion\t{FFMpegCoreVersion}",
                $"SkiaSharpVersion\t{SkiaSharpVersion}",
                $"HandyControlVersion\t{HandyControlVersion}",
            ]);
            Clipboard.SetText(result);
            MessagePopupService.Info(AppResource.Str_Copied);
        });
        private RelayCommand? _cmd_CopyToClipboard;
    }
}
