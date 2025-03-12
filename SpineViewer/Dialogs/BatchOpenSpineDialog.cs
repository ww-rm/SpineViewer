using SpineViewer.Spine;
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
    public partial class BatchOpenSpineDialog : Form
    {
        public string[] SkelPaths { get; private set; }
        public Spine.Version Version { get; private set; }

        public BatchOpenSpineDialog()
        {
            InitializeComponent();
            comboBox_Version.DataSource = VersionHelper.Versions.ToList();
            comboBox_Version.DisplayMember = "Value";
            comboBox_Version.ValueMember = "Key";
            comboBox_Version.SelectedValue = Spine.Version.V38;
        }

        private void BatchOpenSpineDialog_Load(object sender, EventArgs e)
        {
            button_SelectSkel_Click(sender, e);
        }

        private void button_SelectSkel_Click(object sender, EventArgs e)
        {
            if (openFileDialog_Skel.ShowDialog() == DialogResult.OK)
            {
                listBox_FilePath.Items.Clear();
                foreach (var p in openFileDialog_Skel.FileNames)
                    listBox_FilePath.Items.Add(Path.GetFullPath(p));
                label_Tip.Text = $"已选择 {listBox_FilePath.Items.Count} 个文件";
            }
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            if (listBox_FilePath.Items.Count <= 0)
            {
                MessageBox.Show("未选择任何文件", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (string p in listBox_FilePath.Items)
            {
                if (!File.Exists(p))
                {
                    MessageBox.Show($"{p}", "skel文件不存在", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            SkelPaths = listBox_FilePath.Items.Cast<string>().ToArray();
            Version = (Spine.Version)comboBox_Version.SelectedValue;

            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
