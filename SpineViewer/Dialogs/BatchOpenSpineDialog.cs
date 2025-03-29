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

        private void button_Ok_Click(object sender, EventArgs e)
        {
            var version = (Spine.Version)comboBox_Version.SelectedValue;

            var items = skelFileListBox.Items;

            if (items.Count <= 0)
            {
                MessageBox.Info("未选择任何文件");
                return;
            }

            foreach (string p in items)
            {
                if (!File.Exists(p))
                {
                    MessageBox.Info($"{p}", "skel文件不存在");
                    return;
                }
            }

            if (version != Spine.Version.Auto && !Spine.Spine.HasImplementation(version))
            {
                MessageBox.Info($"{version.GetName()} 版本尚未实现（咕咕咕~）");
                return;
            }

            Result = new(version, items.Cast<string>().ToArray());
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
