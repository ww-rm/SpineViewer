using SpineViewer.Spine;
using SpineViewer.Utils;
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
        /// <summary>
        /// 对话框结果, 取消时为 null
        /// </summary>
        public ConvertFileFormatDialogResult Result { get; private set; }

        public ConvertFileFormatDialog()
        {
            InitializeComponent();

            comboBox_SourceVersion.DataSource = SpineHelper.Names.ToList();
            comboBox_SourceVersion.DisplayMember = "Value";
            comboBox_SourceVersion.ValueMember = "Key";
            comboBox_SourceVersion.SelectedValue = SpineVersion.Auto;

            // 目标版本不包含自动
            var versionsWithoutAuto = SpineHelper.Names.ToDictionary();
            versionsWithoutAuto.Remove(SpineVersion.Auto);
            comboBox_TargetVersion.DataSource = versionsWithoutAuto.ToList();
            comboBox_TargetVersion.DisplayMember = "Value";
            comboBox_TargetVersion.ValueMember = "Key";
            comboBox_TargetVersion.SelectedValue = SpineVersion.V38;
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            var sourceVersion = (SpineVersion)comboBox_SourceVersion.SelectedValue;
            var targetVersion = (SpineVersion)comboBox_TargetVersion.SelectedValue;
            var jsonTarget = radioButton_JsonTarget.Checked;

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

            if (sourceVersion != SpineVersion.Auto && !SkeletonConverter.HasImplementation(sourceVersion))
            {
                MessagePopup.Info($"{sourceVersion.GetName()} 版本尚未实现（咕咕咕~）");
                return;
            }

            if (!SkeletonConverter.HasImplementation(targetVersion))
            {
                MessagePopup.Info($"{targetVersion.GetName()} 版本尚未实现（咕咕咕~）");
                return;
            }

            Result = new(items.Cast<string>().ToArray(), sourceVersion, targetVersion, jsonTarget);
            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }

    /// <summary>
    /// 文件格式转换对话框结果包装类
    /// </summary>
    public class ConvertFileFormatDialogResult(string[] skelPaths, SpineVersion sourceVersion, SpineVersion targetVersion, bool jsonTarget)
    {
        /// <summary>
        /// 骨骼文件路径列表
        /// </summary>
        public string[] SkelPaths => skelPaths;

        /// <summary>
        /// 源版本
        /// </summary>
        public SpineVersion SourceVersion => sourceVersion;

        /// <summary>
        /// 目标版本
        /// </summary>
        public SpineVersion TargetVersion => targetVersion;

        /// <summary>
        /// 目标格式是否为 Json
        /// </summary>
        public bool JsonTarget => jsonTarget;
    }
}
