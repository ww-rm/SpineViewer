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
    public partial class OpenSpineDialog : Form
    {
        public string SkelPath { get; private set; }
        public string? AtlasPath { get; private set; }
        public Spine.Version Version { get; private set; }

        public OpenSpineDialog()
        {
            InitializeComponent();
            comboBox_Version.DataSource = VersionHelper.Versions.ToList();
            comboBox_Version.DisplayMember = "Value";
            comboBox_Version.ValueMember = "Key";
            comboBox_Version.SelectedValue = Spine.Version.Auto;
        }

        private void OpenSpineDialog_Load(object sender, EventArgs e)
        {
            button_SelectSkel_Click(sender, e);
        }

        private void button_SelectSkel_Click(object sender, EventArgs e)
        {
            openFileDialog_Skel.InitialDirectory = Path.GetDirectoryName(textBox_SkelPath.Text);
            if (openFileDialog_Skel.ShowDialog() == DialogResult.OK)
            {
                textBox_SkelPath.Text = Path.GetFullPath(openFileDialog_Skel.FileName);
            }
        }

        private void button_SelectAtlas_Click(object sender, EventArgs e)
        {
            openFileDialog_Atlas.InitialDirectory = Path.GetDirectoryName(textBox_AtlasPath.Text);
            if (openFileDialog_Atlas.ShowDialog() == DialogResult.OK)
            {
                textBox_AtlasPath.Text = Path.GetFullPath(openFileDialog_Atlas.FileName);
            }
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            var skelPath = textBox_SkelPath.Text;
            var atlasPath = textBox_AtlasPath.Text;
            var version = (Spine.Version)comboBox_Version.SelectedValue;

            if (!File.Exists(skelPath))
            {
                MessageBox.Show($"{skelPath}", "skel文件不存在", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                skelPath = Path.GetFullPath(skelPath);
            }

            if (string.IsNullOrEmpty(atlasPath))
            {
                atlasPath = null;
            }
            else if (!File.Exists(atlasPath))
            {
                MessageBox.Show($"{atlasPath}", "atlas文件不存在", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                atlasPath = Path.GetFullPath(atlasPath);
            }

            if (version != Spine.Version.Auto && !Spine.Spine.ImplementedVersions.Contains(version))
            {
                MessageBox.Show($"{version.String()} 版本尚未实现（咕咕咕~）", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SkelPath = skelPath;
            AtlasPath = atlasPath;
            Version = version;

            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
