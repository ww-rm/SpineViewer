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
        /// <summary>
        /// 对话框结果
        /// </summary>
        public OpenSpineDialogResult Result { get; private set; }        

        public OpenSpineDialog()
        {
            InitializeComponent();
            comboBox_Version.DataSource = SpineHelper.Names.ToList();
            comboBox_Version.DisplayMember = "Value";
            comboBox_Version.ValueMember = "Key";
            comboBox_Version.SelectedValue = SpineVersion.Auto;
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
            var version = (SpineVersion)comboBox_Version.SelectedValue;

            if (!File.Exists(skelPath))
            {
                MessageBox.Info($"{skelPath}", "skel文件不存在");
                return;
            }
            else
            {
                skelPath = Path.GetFullPath(skelPath);
            }

            if (string.IsNullOrWhiteSpace(atlasPath))
            {
                atlasPath = null;
            }
            else if (!File.Exists(atlasPath))
            {
                MessageBox.Info($"{atlasPath}", "atlas文件不存在");
                return;
            }
            else
            {
                atlasPath = Path.GetFullPath(atlasPath);
            }

            if (version != SpineVersion.Auto && !Spine.Spine.HasImplementation(version))
            {
                MessageBox.Info($"{version.GetName()} 版本尚未实现（咕咕咕~）");
                return;
            }

            Result = new(version, skelPath, atlasPath);
            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }

    /// <summary>
    /// 打开骨骼对话框结果
    /// </summary>
    public class OpenSpineDialogResult(SpineVersion version, string skelPath, string? atlasPath = null)
    {
        /// <summary>
        /// 版本
        /// </summary>
        public SpineVersion Version => version;

        /// <summary>
        /// skel 文件路径
        /// </summary>
        public string SkelPath => skelPath;

        /// <summary>
        /// atlas 文件路径
        /// </summary>
        public string? AtlasPath => atlasPath;
    }
}
