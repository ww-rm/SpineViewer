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
    public partial class ConvertFileFormatDialog : Form
    {
        public string[] SkelPaths { get; private set; }
        public Spine.Version Version { get; private set; }
        public bool ConvertToJson { get; private set; }

        public ConvertFileFormatDialog()
        {
            InitializeComponent();
            comboBox_Version.DataSource = VersionHelper.Versions.ToList();
            comboBox_Version.DisplayMember = "Value";
            comboBox_Version.ValueMember = "Key";
            comboBox_Version.SelectedValue = Spine.Version.V38;
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
            var version = (Spine.Version)comboBox_Version.SelectedValue;

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

            if (!Spine.Spine.ImplementedVersions.Contains(version) ||
                !SkeletonConverter.ImplementedVersions.Contains(version))
            {
                MessageBox.Show($"{version.String()} 版本尚未实现（咕咕咕~）", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SkelPaths = listBox_FilePath.Items.Cast<string>().ToArray();
            Version = version;
            ConvertToJson = radioButton_BinarySource.Checked;

            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void radioButton_Source_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_BinarySource.Checked)
                radioButton_JsonTarget.Checked = true;
            else
                radioButton_BinaryTarget.Checked = true;
        }

        private void radioButton_Target_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_BinaryTarget.Checked)
                radioButton_JsonSource.Checked = true;
            else
                radioButton_BinarySource.Checked = true;
        }
    }
}
