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
        /// <summary>
        /// 对话框结果, 取消时为 null
        /// </summary>
        public ConvertFileFormatDialogResult Result { get; private set; }

        public ConvertFileFormatDialog()
        {
            InitializeComponent();

            comboBox_SourceVersion.DataSource = VersionHelper.Names.ToList();
            comboBox_SourceVersion.DisplayMember = "Value";
            comboBox_SourceVersion.ValueMember = "Key";
            comboBox_SourceVersion.SelectedValue = Spine.Version.Auto;

            // 目标版本不包含自动
            var versionsWithoutAuto = VersionHelper.Names.ToDictionary();
            versionsWithoutAuto.Remove(Spine.Version.Auto);
            comboBox_TargetVersion.DataSource = versionsWithoutAuto.ToList();
            comboBox_TargetVersion.DisplayMember = "Value";
            comboBox_TargetVersion.ValueMember = "Key";
            comboBox_TargetVersion.SelectedValue = Spine.Version.V38;
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            var sourceVersion = (Spine.Version)comboBox_SourceVersion.SelectedValue;
            var targetVersion = (Spine.Version)comboBox_TargetVersion.SelectedValue;
            var jsonTarget = radioButton_JsonTarget.Checked;

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

            if (sourceVersion != Spine.Version.Auto && !SkeletonConverter.ImplementedVersions.Contains(sourceVersion))
            {
                MessageBox.Info($"{sourceVersion.GetName()} 版本尚未实现（咕咕咕~）");
                return;
            }

            if (!SkeletonConverter.ImplementedVersions.Contains(targetVersion))
            {
                MessageBox.Info($"{targetVersion.GetName()} 版本尚未实现（咕咕咕~）");
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
    public class ConvertFileFormatDialogResult(string[] skelPaths, Spine.Version sourceVersion, Spine.Version targetVersion, bool jsonTarget)
    {
        /// <summary>
        /// 骨骼文件路径列表
        /// </summary>
        public string[] SkelPaths => skelPaths;

        /// <summary>
        /// 源版本
        /// </summary>
        public Spine.Version SourceVersion => sourceVersion;

        /// <summary>
        /// 目标版本
        /// </summary>
        public Spine.Version TargetVersion => targetVersion;

        /// <summary>
        /// 目标格式是否为 Json
        /// </summary>
        public bool JsonTarget => jsonTarget;
    }
}
