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
    public partial class ExportPngDialog : Form
    {
        public string OutputDir { get; private set; }
        public float Duration { get; private set; }
        public uint Fps { get; private set; }

        public ExportPngDialog()
        {
            InitializeComponent();
        }

        private void ExportPngDialog_Load(object sender, EventArgs e)
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
                MessageBox.Show("输出文件夹无效", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!Directory.Exists(outputDir))
            {
                if (MessageBox.Show($"文件夹 {outputDir} 不存在，是否创建？", "操作确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    try
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error(ex.ToString());
                        MessageBox.Show(ex.ToString(), "文件夹创建失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            OutputDir = Path.GetFullPath(outputDir);
            Duration = (float)numericUpDown_Duration.Value;
            Fps = (uint)numericUpDown_Fps.Value;

            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
