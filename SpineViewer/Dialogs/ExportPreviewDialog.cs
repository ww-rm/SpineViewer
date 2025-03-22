using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Dialogs
{
    public partial class ExportPreviewDialog: Form
    {
        // TODO: 用单独的结果包装类
        public string OutputDir { get; private set; }
        public uint PreviewWidth { get; private set; }
        public uint PreviewHeight { get; private set; }

        public ExportPreviewDialog()
        {
            InitializeComponent();
        }

        private void ExportPreviewDialog_Load(object sender, EventArgs e)
        {
            button_SelectOutputDir_Click(sender, e);
        }

        private void button_SelectOutputDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.InitialDirectory = textBox_OutputDir.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox_OutputDir.Text = Path.GetFullPath(folderBrowserDialog.SelectedPath);
            }
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            var outputDir = textBox_OutputDir.Text;
            if (File.Exists(outputDir))
            {
                MessageBox.Info("输出文件夹无效");
                return;
            }

            if (!Directory.Exists(outputDir))
            {
                if (MessageBox.Quest($"文件夹 {outputDir} 不存在，是否创建？") != DialogResult.OK)
                    return;

                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    Program.Logger.Error(ex.ToString());
                    MessageBox.Error(ex.ToString(), "文件夹创建失败");
                    return;
                }
            }

            OutputDir = Path.GetFullPath(outputDir);
            PreviewWidth = (uint)numericUpDown_Width.Value;
            PreviewHeight = (uint)numericUpDown_Height.Value;

            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
