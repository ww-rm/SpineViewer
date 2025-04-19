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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public nint RenderHandle { get => panel.Handle; }

        public WallpaperForm()
        {
            InitializeComponent();
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        // 先把“可见”样式关掉，这样 Win32 在创建完句柄时不会自动 ShowWindow
        //        cp.Style &= ~Win32.WS_VISIBLE;
        //        return cp;
        //    }
        //}

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        cp.X = cp.Y = 0;
        //        cp.Style = Win32.WS_POPUP;
        //        cp.ExStyle = Win32.WS_EX_TOOLWINDOW;
        //        return cp;
        //    }
        //}

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetWallpaper();
        }

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
                    Win32.SetParent(Handle, workerW);
                }
            }
        }
    }
}
