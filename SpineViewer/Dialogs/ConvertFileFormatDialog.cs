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
        public Spine.Version SourceVersion { get; private set; }
        public Spine.Version TargetVersion { get; private set; }
        public bool JsonSource { get; private set; }
        public bool JsonTarget { get; private set; }

        public ConvertFileFormatDialog()
        {
            InitializeComponent();

            // XXX: 文件格式转换暂时不支持自动检测版本
            var impVersions = VersionHelper.Names.ToDictionary();
            impVersions.Remove(Spine.Version.Auto);

            comboBox_SourceVersion.DataSource = impVersions.ToList();
            comboBox_SourceVersion.DisplayMember = "Value";
            comboBox_SourceVersion.ValueMember = "Key";
            comboBox_SourceVersion.SelectedValue = Spine.Version.V38;
            //comboBox_TargetVersion.DataSource = VersionHelper.Versions.ToList();
            //comboBox_TargetVersion.DisplayMember = "Value";
            //comboBox_TargetVersion.ValueMember = "Key";
            //comboBox_TargetVersion.SelectedValue = Spine.Version.V38;
        }

        private void ConvertFileFormatDialog_Load(object sender, EventArgs e)
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
            var sourceVersion = (Spine.Version)comboBox_SourceVersion.SelectedValue;
            var targetVersion = (Spine.Version)comboBox_SourceVersion.SelectedValue;  // TODO: 增加目标版本
            var jsonSource = radioButton_JsonSource.Checked;
            var jsonTarget = radioButton_JsonTarget.Checked;

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

            if (!SkeletonConverter.ImplementedVersions.Contains(sourceVersion))
            {
                MessageBox.Show($"{sourceVersion.GetName()} 版本尚未实现（咕咕咕~）", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!SkeletonConverter.ImplementedVersions.Contains(targetVersion))
            {
                MessageBox.Show($"{targetVersion.GetName()} 版本尚未实现（咕咕咕~）", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (jsonSource == jsonTarget && sourceVersion == targetVersion)
            {
                MessageBox.Show($"不需要转换相同的格式和版本", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SkelPaths = listBox_FilePath.Items.Cast<string>().ToArray();
            SourceVersion = sourceVersion;
            TargetVersion = targetVersion;
            JsonSource = jsonSource;
            JsonTarget = jsonTarget;

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
