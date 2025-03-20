﻿using SpineViewer.Spine;
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

            if (version != Spine.Version.Auto && !Spine.Spine.ImplementedVersions.Contains(version))
            {
                MessageBox.Show($"{version.GetName()} 版本尚未实现（咕咕咕~）", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    public class BatchOpenSpineDialogResult(Spine.Version version, string[] skelPaths)
    {
        public Spine.Version Version { get; } = version;
        public string[] SkelPaths { get; } = skelPaths;
    }
}
