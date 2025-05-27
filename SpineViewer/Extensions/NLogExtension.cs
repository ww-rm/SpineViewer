using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Extensions
{
    public static class NLogExtension
    {
        /// <summary>
        /// 输出当前进程的内存占用
        /// </summary>
        public static void LogCurrentProcessMemoryUsage(this NLog.Logger self)
        {
            var process = Process.GetCurrentProcess();
            self.Info("Current memory usage for {0}: {1:F2} MB", process.ProcessName, process.WorkingSet64 / 1024.0 / 1024.0);
        }
    }
}
