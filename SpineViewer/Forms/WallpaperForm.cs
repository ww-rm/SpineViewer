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

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_LAYERED;
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 设置成嵌入桌面
            var progman = Win32.FindWindow("Progman", null);
            if (progman != IntPtr.Zero)
            {
                // 确保 WorkerW 被创建
                Win32.SendMessageTimeout(progman, Win32.WM_SPAWN_WORKER, IntPtr.Zero, IntPtr.Zero, Win32.SMTO_NORMAL, 1000, out _);
                var workerW = Win32.GetWorkerW();
                if (workerW != IntPtr.Zero)
                {
                    Win32.SetLayeredWindowAttributes(Handle, 0, 255, Win32.LWA_ALPHA);
                    Win32.SetParent(Handle, workerW); // 嵌入之前必须保证有 WS_EX_LAYERED 标志
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public byte LayeredWindowAlpha
        { 
            get
            {
                uint crKey = 0;
                byte bAlpha = 255;
                uint dwFlags = Win32.LWA_ALPHA;
                Win32.GetLayeredWindowAttributes(Handle, ref crKey, ref bAlpha, ref dwFlags);
                return bAlpha;
            }
            set
            {
                Win32.SetLayeredWindowAttributes(Handle, 0, value, Win32.LWA_ALPHA);
            }
        }
    }
}
