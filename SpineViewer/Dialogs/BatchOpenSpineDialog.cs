using SpineViewer.Spine;
using SpineViewer.Utilities;
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
            comboBox_Version.DataSource = SpineHelper.Names.ToList();
            comboBox_Version.DisplayMember = "Value";
            comboBox_Version.ValueMember = "Key";
            comboBox_Version.SelectedValue = SpineVersion.Auto;
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            var version = (SpineVersion)comboBox_Version.SelectedValue;

            var items = skelFileListBox.Items;

            if (items.Count <= 0)
            {
                MessagePopup.Info("未选择任何文件");
                return;
            }

            foreach (string p in items)
            {
                if (!File.Exists(p))
                {
                    MessagePopup.Info($"{p}", "skel文件不存在");
                    return;
                }
            }

            if (version != SpineVersion.Auto && !Spine.Spine.HasImplementation(version))
            {
                MessagePopup.Info($"{version.GetName()} 版本尚未实现（咕咕咕~）");
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
    public class BatchOpenSpineDialogResult(SpineVersion version, string[] skelPaths)
    {
        /// <summary>
        /// 版本
        /// </summary>
        public SpineVersion Version => version;

        /// <summary>
        /// 路径列表
        /// </summary>
        public string[] SkelPaths => skelPaths;
    }
}
