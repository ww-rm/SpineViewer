using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public interface IBrowserOpenable
    {
        /// <summary>
        /// 浏览器打开网址
        /// </summary>
        public string OpenInBrowserUrl { get; }
    }

    public static class IBrowserOpenableExtension
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 在浏览器中打开网址
        /// </summary>
        public static void OpenUrlInBroswer(this IBrowserOpenable self)
        {
            if (self.OpenInBrowserUrl is null)
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = $"\"{self.OpenInBrowserUrl}\"",
                UseShellExecute = true,
            });
        }
    }
}
