using SpineViewer.ExportHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Dialogs
{
    public partial class ExportDialog: Form
    {
        /// <summary>
        /// 要绑定的导出参数
        /// </summary>
        public required ExportArgs ExportArgs 
        { 
            get => propertyGrid_ExportArgs.SelectedObject as ExportArgs; 
            init => propertyGrid_ExportArgs.SelectedObject = value; 
        }

        public ExportDialog()
        {
            InitializeComponent();
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            if (ExportArgs.Validate() is string error)
            { 
                MessageBox.Info(error, "参数错误");
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
