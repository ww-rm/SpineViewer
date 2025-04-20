using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SpineViewer.Spine;

namespace SpineViewer.Controls
{
    public partial class SkelFileListBox : UserControl
    {
        public SkelFileListBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ListBox.Items
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public ListBox.ObjectCollection Items { get => listBox.Items; }

        /// <summary>
        /// 从路径列表添加
        /// </summary>
        private void AddFromFileDrop(string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    if (SpineUtils.CommonSkelSuffix.Contains(Path.GetExtension(path).ToLower()))
                        listBox.Items.Add(Path.GetFullPath(path));
                }
                else if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        if (SpineUtils.CommonSkelSuffix.Contains(Path.GetExtension(file).ToLower()))
                            listBox.Items.Add(file);
                    }
                }
            }
        }

        private void button_AddFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;

            var path = folderBrowserDialog.SelectedPath;
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    if (SpineUtils.CommonSkelSuffix.Contains(Path.GetExtension(file).ToLower()))
                        listBox.Items.Add(file);
                }
            }

            label_Tip.Text = $"已选择 {listBox.Items.Count} 个文件";
        }

        private void button_AddFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog_Skel.ShowDialog() != DialogResult.OK)
                return;

            foreach (var p in openFileDialog_Skel.FileNames)
                listBox.Items.Add(Path.GetFullPath(p));

            label_Tip.Text = $"已选择 {listBox.Items.Count} 个文件";
        }

        private void listBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBox_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            AddFromFileDrop((string[])e.Data.GetData(DataFormats.FileDrop));
            label_Tip.Text = $"已选择 {listBox.Items.Count} 个文件";
        }

        private void toolStripMenuItem_SelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox.Items.Count; i++)
                listBox.SelectedIndices.Add(i);
        }

        private void toolStripMenuItem_Paste_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsFileDropList())
                return;

            var fileDropList = Clipboard.GetFileDropList();
            var paths = new string[fileDropList.Count];
            fileDropList.CopyTo(paths, 0);
            AddFromFileDrop(paths);
            label_Tip.Text = $"已选择 {listBox.Items.Count} 个文件";
        }

        private void toolStripMenuItem_Remove_Click(object sender, EventArgs e)
        {
            var indices = new int[listBox.SelectedIndices.Count];
            listBox.SelectedIndices.CopyTo(indices, 0);
            for (int i = indices.Length - 1; i >= 0; i--)
                listBox.Items.RemoveAt(indices[i]);
            label_Tip.Text = $"已选择 {listBox.Items.Count} 个文件";
        }
    }
}
