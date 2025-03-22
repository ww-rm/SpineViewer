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
        /// <summary>
        /// 对话框结果, 取消时为 null
        /// </summary>
        public BatchOpenSpineDialogResult Result { get; private set; }

        public BatchOpenSpineDialog()
        {
            InitializeComponent();
            comboBox_Version.DataSource = VersionHelper.Names.ToList();
            comboBox_Version.DisplayMember = "Value";
            comboBox_Version.ValueMember = "Key";
            comboBox_Version.SelectedValue = Spine.Version.Auto;
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
            var version = (Spine.Version)comboBox_Version.SelectedValue;

            if (listBox_FilePath.Items.Count <= 0)
            {
                MessageBox.Info("未选择任何文件");
                return;
            }

            foreach (string p in listBox_FilePath.Items)
            {
                if (!File.Exists(p))
                {
                    MessageBox.Info($"{p}", "skel文件不存在");
                    return;
                }
            }

            if (version != Spine.Version.Auto && !Spine.Spine.ImplementedVersions.Contains(version))
            {
                MessageBox.Info($"{version.GetName()} 版本尚未实现（咕咕咕~）");
                return;
            }

            Result = new(version, listBox_FilePath.Items.Cast<string>().ToArray());
            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }

    /// <summary>
    /// 批量打开对话框结果
    /// </summary>
    public class BatchOpenSpineDialogResult(Spine.Version version, string[] skelPaths)
    {
        /// <summary>
        /// 版本
        /// </summary>
        public Spine.Version Version => version;

        /// <summary>
        /// 路径列表
        /// </summary>
        public string[] SkelPaths => skelPaths;
    }
}
