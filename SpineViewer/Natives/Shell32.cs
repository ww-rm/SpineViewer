using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Natives
{
    /// <summary>
    /// shell32.dll 包装类
    /// </summary>
    public static class Shell32
    {
        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, nint dwItem1, nint dwItem2);

        public static void NotifyAssociationChanged()
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, nint.Zero, nint.Zero);
        }
    }
}
