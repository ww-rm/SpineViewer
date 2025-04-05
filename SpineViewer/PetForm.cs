using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer
{
    public partial class PetForm: Form
    {
        public PetForm()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                //var style = Win32.GetWindowLong(hWnd, Win32.GWL_STYLE) | Win32.WS_POPUP;
                //var exStyle = Win32.GetWindowLong(hWnd, Win32.GWL_EXSTYLE) | Win32.WS_EX_LAYERED | Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_TOPMOST;
                //Win32.SetWindowLong(hWnd, Win32.GWL_STYLE, style);
                //Win32.SetWindowLong(hWnd, Win32.GWL_EXSTYLE, exStyle);
                //Win32.SetLayeredWindowAttributes(hWnd, crKey, 255, Win32.LWA_COLORKEY | Win32.LWA_ALPHA);
                //Win32.SetWindowPos(hWnd, Win32.HWND_TOPMOST, 0, 0, 0, 0, Win32.SWP_NOMOVE | Win32.SWP_NOSIZE);
                var cp = base.CreateParams;
                cp.ExStyle = Win32.WS_EX_LAYERED | Win32.WS_EX_TOPMOST;
                cp.Style = Win32.WS_POPUP;
                //cp.ExStyle |= Win32.WS_EX_LAYERED | Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_TOPMOST;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            ;
        }
    }
}
