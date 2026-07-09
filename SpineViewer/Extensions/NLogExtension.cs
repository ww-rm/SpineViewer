using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Extensions
{
    public static class NLogExtension
    {
        private static readonly ulong _systemMemory = int.MaxValue;

        static NLogExtension()
        {
            // 获取系统可用总内存大小
            var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                _systemMemory = (ulong)obj["TotalPhysicalMemory"];
                break;
            }
        }

        /// <summary>
        /// 输出当前进程的内存占用
        /// </summary>
        public static void LogCurrentProcessMemoryUsage(this NLog.Logger self)
        {
            var process = Process.GetCurrentProcess();
            ulong workingSet64 = (ulong)process.WorkingSet64;
            if (workingSet64 < _systemMemory / 2)
                self.Info("Current memory usage for {0}: {1:F2} MB", process.ProcessName, workingSet64 / 1024.0 / 1024.0);
            else
                self.Warn("Current memory usage for {0}: {1:F2} MB", process.ProcessName, workingSet64 / 1024.0 / 1024.0);
        }
    }
}
