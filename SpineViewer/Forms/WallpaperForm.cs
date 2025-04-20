using SpineViewer.Natives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer
{
    [ToolboxItem(true)]
    [Designer(typeof(ComponentDesigner), typeof(IDesigner))]
    [DesignTimeVisible(true)]
    public partial class WallpaperForm: Form
    {
        public WallpaperForm()
        {
            InitializeComponent();
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        cp.ExStyle = Win32.WS_EX_LAYERED;
        //        return cp;
        //    }
        //}

        //protected override void OnHandleCreated(EventArgs e)
        //{
        //    base.OnHandleCreated(e);
        //    Win32.SetLayeredWindowAttributes(Handle, 0, 255, Win32.LWA_ALPHA);
        //    SetWallpaper();
        //}

        public void SetWallpaper()
        {
            // 设置成嵌入桌面
            var progman = Win32.FindWindow("Progman", null);
            if (progman != IntPtr.Zero)
            {
                // 确保 WorkerW 被创建
                Win32.SendMessageTimeout(progman, Win32.WM_SPAWN_WORKER, IntPtr.Zero, IntPtr.Zero, Win32.SMTO_NORMAL, 1000, out _);
                var workerW = Win32.GetWorkerW();
                if (workerW != IntPtr.Zero)
                {
                    Win32.SetWindowLong(Handle, Win32.GWL_EXSTYLE, Win32.GetWindowLong(Handle, Win32.GWL_EXSTYLE) | Win32.WS_EX_LAYERED);
                    Win32.SetLayeredWindowAttributes(Handle, 0, 255, Win32.LWA_ALPHA);
                    Win32.SetParent(Handle, workerW);
                }
            }
        }
    }
}
