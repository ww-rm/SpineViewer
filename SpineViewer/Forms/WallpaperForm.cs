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
                cp.X = cp.Y = 0;
                cp.ExStyle = Win32.WS_EX_LAYERED | Win32.WS_EX_TOOLWINDOW;
                cp.Style = Win32.WS_POPUP;
                return cp;
            }
        }
    }
}
