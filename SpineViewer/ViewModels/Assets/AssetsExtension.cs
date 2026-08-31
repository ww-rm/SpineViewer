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
    public static class AssetsExtension
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 在资源管理器中打开目录
        /// </summary>
        public static void OpenDirectoryInExplorer(this IExplorerOpenable self)
        {
            if (!Directory.Exists(self.OpenInExplorerDirectory))
            {
                _logger.Error("Directory '{0}' is not existed.", self.OpenInExplorerDirectory);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{self.OpenInExplorerDirectory}\"",
                UseShellExecute = true,
            });
        }
    }
}
