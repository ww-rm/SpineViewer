﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using SpineViewer.Spine;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Specialized;

namespace SpineViewer.Controls
{
    public partial class SpineListView : UserControl
    {
        /// <summary>
        /// 显示骨骼信息的属性面板
        /// </summary>
        [Category("自定义"), Description("用于显示骨骼属性的属性页")]
        public PropertyGrid? PropertyGrid { get; set; }

        /// <summary>
        /// Spine 列表只读视图, 访问时必须使用 lock 语句锁定视图本身
        /// </summary>
        public readonly ReadOnlyCollection<Spine.Spine> Spines;

        /// <summary>
        /// Spine 列表, 访问时必须使用 lock 语句锁定只读视图 Spines
        /// </summary>
        private readonly List<Spine.Spine> spines = [];

        public SpineListView()
        {
            InitializeComponent();
            Spines = spines.AsReadOnly();
        }

        /// <summary>
        /// 选中的索引
        /// </summary>
        public ListView.SelectedIndexCollection SelectedIndices => listView.SelectedIndices;

        /// <summary>
        /// 弹出添加对话框在末尾添加
        /// </summary>
        public void Add() => Insert();

        /// <summary>
        /// 弹出添加对话框在指定位置之前插入一项, 如果索引无效则在末尾添加
        /// </summary>
        private void Insert(int index = -1)
        {
            var dialog = new Dialogs.OpenSpineDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            Insert(dialog.Result, index);
        }

        /// <summary>
        /// 从结果在指定位置之前插入一项, 如果索引无效则在末尾添加
        /// </summary>
        private void Insert(Dialogs.OpenSpineDialogResult result, int index = -1)
        {
            try
            {
                var spine = Spine.Spine.New(result.Version, result.SkelPath, result.AtlasPath);

                // 如果索引无效则在末尾添加
                if (index < 0 || index > listView.Items.Count)
                    index = listView.Items.Count;

                // 锁定外部的读操作
                lock (Spines)
                {
                    spines.Insert(index, spine);
                    listView.SmallImageList.Images.Add(spine.ID, spine.Preview);
                    listView.LargeImageList.Images.Add(spine.ID, spine.Preview);
                }
                listView.Items.Insert(index, new ListViewItem(spine.Name, spine.ID) { ToolTipText = spine.SkelPath });

                // 选中新增项
                listView.SelectedIndices.Clear();
                listView.SelectedIndices.Add(index);
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex.ToString());
                Program.Logger.Error("Failed to load {} {}", result.SkelPath, result.AtlasPath);
                MessageBox.Error(ex.ToString(), "骨骼加载失败");
            }

            Program.LogCurrentMemoryUsage();
        }

        /// <summary>
        /// 弹出批量添加对话框
        /// </summary>
        public void BatchAdd()
        {
            var openDialog = new Dialogs.BatchOpenSpineDialog();
            if (openDialog.ShowDialog() != DialogResult.OK)
                return;
            BatchAdd(openDialog.Result);
        }

        /// <summary>
        /// 从结果批量添加
        /// </summary>
        public void BatchAdd(Dialogs.BatchOpenSpineDialogResult result)
        {
            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += BatchAdd_Work;
            progressDialog.RunWorkerAsync(result);
            progressDialog.ShowDialog();
        }

        /// <summary>
        /// 批量添加后台任务
        /// </summary>
        private void BatchAdd_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as Dialogs.BatchOpenSpineDialogResult;
            var skelPaths = arguments.SkelPaths;
            var version = arguments.Version;

            int totalCount = skelPaths.Length;
            int success = 0;
            int error = 0;

            worker.ReportProgress(0, $"已处理 0/{totalCount}");
            for (int i = 0; i < totalCount; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                var skelPath = skelPaths[i];

                try
                {
                    var spine = Spine.Spine.New(version, skelPath);
                    var preview = spine.Preview;
                    lock (Spines) { spines.Add(spine); }
                    listView.Invoke(() =>
                    {
                        listView.SmallImageList.Images.Add(spine.ID, preview);
                        listView.LargeImageList.Images.Add(spine.ID, preview);
                        listView.Items.Add(new ListViewItem(spine.Name, spine.ID) { ToolTipText = spine.SkelPath });
                    });
                    success++;
                }
                catch (Exception ex)
                {
                    Program.Logger.Error(ex.ToString());
                    Program.Logger.Error("Failed to load {}", skelPath);
                    error++;
                }

                worker.ReportProgress((int)((i + 1) * 100.0) / totalCount, $"已处理 {i + 1}/{totalCount}");
            }

            if (error > 0)
            {
                Program.Logger.Warn("Batch load {} successfully, {} failed", success, error);
            }
            else
            {
                Program.Logger.Info("{} skel loaded successfully", success);
            }

            Program.LogCurrentMemoryUsage();
        }

        /// <summary>
        /// 从拖放/复制的路径列表添加
        /// </summary>
        private void AddFromFileDrop(IEnumerable<string> paths)
        {
            List<string> validPaths = [];
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    if (Spine.Spine.CommonSkelSuffix.Contains(Path.GetExtension(path).ToLower()))
                        validPaths.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        if (Spine.Spine.CommonSkelSuffix.Contains(Path.GetExtension(file).ToLower()))
                            validPaths.Add(file);
                    }
                }
            }

            if (validPaths.Count > 1)
            {
                if (validPaths.Count > 100)
                {
                    if (MessageBox.Quest($"共发现 {validPaths.Count} 个可加载骨骼，数量较多，是否一次性全部加载？") == DialogResult.Cancel)
                        return;
                }
                BatchAdd(new Dialogs.BatchOpenSpineDialogResult(Spine.Version.Auto, validPaths.ToArray()));
            }
            else if (validPaths.Count > 0)
            {
                Insert(new Dialogs.OpenSpineDialogResult(Spine.Version.Auto, validPaths[0]));
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PropertyGrid is not null)
            {
                lock (Spines)
                {
                    if (listView.SelectedIndices.Count <= 0)
                        PropertyGrid.SelectedObject = null;
                    else if (listView.SelectedIndices.Count <= 1)
                        PropertyGrid.SelectedObject = spines[listView.SelectedIndices[0]];
                    else
                        PropertyGrid.SelectedObjects = listView.SelectedIndices.Cast<int>().Select(index => spines[index]).ToArray();

                    // 标记选中的 Spine
                    for (int i = 0; i < spines.Count; i++)
                        spines[i].IsSelected = listView.SelectedIndices.Contains(i);
                }
            }

            // XXX: 图标显示的时候没法自动刷新顺序, 只能切换视图刷新, 不知道什么原理
            if (listView.View == View.LargeIcon)
            {
                listView.BeginUpdate();
                listView.View = View.List;
                listView.View = View.LargeIcon;
                listView.EndUpdate();
            }

            if (listView.SelectedItems.Count > 0)
                listView.SelectedItems[0].EnsureVisible();
        }

        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Serializable))
                e.Effect = DragDropEffects.Move;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Serializable))
            {
                // 获取鼠标位置并确定目标索引
                var point = listView.PointToClient(new(e.X, e.Y));
                var targetItem = listView.GetItemAt(point.X, point.Y);

                // 高亮目标项
                if (targetItem != null)
                {
                    foreach (ListViewItem item in listView.Items)
                    {
                        item.BackColor = listView.BackColor;
                    }
                    targetItem.BackColor = Color.LightGray;
                }
            }
        }

        private void listView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Serializable))
            {
                // 获取拖放源项和目标项
                var draggedItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
                int draggedIndex = draggedItem.Index;
                var point = listView.PointToClient(new Point(e.X, e.Y));
                var targetItem = listView.GetItemAt(point.X, point.Y);
                int targetIndex = targetItem is null ? listView.Items.Count : targetItem.Index;

                if (targetIndex <= draggedIndex)
                {
                    lock (Spines)
                    {
                        var draggedSpine = spines[draggedIndex];
                        spines.RemoveAt(draggedIndex);
                        spines.Insert(targetIndex, draggedSpine);
                    }
                    listView.Items.RemoveAt(draggedIndex);
                    listView.Items.Insert(targetIndex, draggedItem);
                }
                else
                {
                    lock (Spines)
                    {
                        var draggedSpine = spines[draggedIndex];
                        spines.RemoveAt(draggedIndex);
                        spines.Insert(targetIndex - 1, draggedSpine);
                    }
                    listView.Items.RemoveAt(draggedIndex);
                    listView.Items.Insert(targetIndex - 1, draggedItem);
                }

                // 重置背景颜色
                foreach (ListViewItem item in listView.Items)
                {
                    item.BackColor = listView.BackColor;
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                AddFromFileDrop((string[])e.Data.GetData(DataFormats.FileDrop));
            }
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var selectedIndices = listView.SelectedIndices;
            var selectedCount = selectedIndices.Count;
            var itemsCount = listView.Items.Count;
            toolStripMenuItem_Insert.Enabled = selectedCount == 1;
            toolStripMenuItem_Remove.Enabled = selectedCount >= 1;
            toolStripMenuItem_MoveTop.Enabled = selectedCount == 1 && selectedIndices[0] != 0;
            toolStripMenuItem_MoveUp.Enabled = selectedCount == 1 && selectedIndices[0] != 0;
            toolStripMenuItem_MoveDown.Enabled = selectedCount == 1 && selectedIndices[0] != itemsCount - 1;
            toolStripMenuItem_MoveBottom.Enabled = selectedCount == 1 && selectedIndices[0] != itemsCount - 1;
            toolStripMenuItem_RemoveAll.Enabled = itemsCount > 0;

            // 视图选项
            toolStripMenuItem_LargeIconView.Checked = listView.View == View.LargeIcon;
            toolStripMenuItem_ListView.Checked = listView.View == View.List;
            toolStripMenuItem_DetailsView.Checked = listView.View == View.Details;
        }

        private void contextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            // 不显示菜单的时候需要把菜单的各项功能启用, 这样才能正常捕获快捷键
            foreach (var item in contextMenuStrip.Items)
                if (item is ToolStripMenuItem tsmi)
                    tsmi.Enabled = true;
        }

        private void toolStripMenuItem_Add_Click(object sender, EventArgs e)
        {
            Insert();
        }

        private void toolStripMenuItem_BatchAdd_Click(object sender, EventArgs e)
        {
            BatchAdd();
        }

        private void toolStripMenuItem_Insert_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count == 1)
                Insert(listView.SelectedIndices[0]);
        }

        private void toolStripMenuItem_Remove_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count <= 0)
                return;

            if (listView.SelectedIndices.Count > 1)
            {
                if (MessageBox.Quest($"确定移除所选 {listView.SelectedIndices.Count} 项吗？") != DialogResult.OK)
                    return;
            }

            foreach (var i in listView.SelectedIndices.Cast<int>().OrderByDescending(x => x))
            {
                listView.Items.RemoveAt(i);
                lock (Spines)
                {
                    var spine = spines[i];
                    spines.RemoveAt(i);
                    listView.SmallImageList.Images.RemoveByKey(spine.ID);
                    listView.LargeImageList.Images.RemoveByKey(spine.ID);
                    spine.Dispose();
                }
            }
        }

        private void toolStripMenuItem_MoveTop_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count != 1)
                return;

            var index = listView.SelectedIndices[0];
            if (index > 0)
            {
                lock (Spines)
                {
                    var spine = spines[index];
                    spines.RemoveAt(index);
                    spines.Insert(0, spine);
                }
                var item = listView.Items[index];
                listView.Items.RemoveAt(index);
                listView.Items.Insert(0, item);
            }
        }

        private void toolStripMenuItem_MoveUp_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count != 1)
                return;

            var index = listView.SelectedIndices[0];
            if (index > 0)
            {
                lock (Spines) { (spines[index - 1], spines[index]) = (spines[index], spines[index - 1]); }
                var item = listView.Items[index];
                listView.BeginUpdate();
                listView.Items.RemoveAt(index);
                listView.Items.Insert(index - 1, item);
                listView.EndUpdate();
            }
        }

        private void toolStripMenuItem_MoveDown_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count != 1)
                return;

            var index = listView.SelectedIndices[0];
            if (index < listView.Items.Count - 1)
            {
                lock (Spines) { (spines[index], spines[index + 1]) = (spines[index + 1], spines[index]); }
                var item = listView.Items[index];
                listView.BeginUpdate();
                listView.Items.RemoveAt(index);
                listView.Items.Insert(index + 1, item);
                listView.EndUpdate();
            }
        }

        private void toolStripMenuItem_MoveBottom_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count != 1)
                return;

            var index = listView.SelectedIndices[0];
            if (index < listView.Items.Count - 1)
            {
                lock (Spines)
                {
                    var spine = spines[index];
                    spines.RemoveAt(index);
                    spines.Add(spine);
                }
                var item = listView.Items[index];
                listView.Items.RemoveAt(index);
                listView.Items.Add(item);
            }
        }

        private void toolStripMenuItem_RemoveAll_Click(object sender, EventArgs e)
        {
            if (listView.Items.Count <= 0)
                return;

            if (MessageBox.Quest($"确认移除所有 {listView.Items.Count} 项吗？") != DialogResult.OK)
                return;

            listView.Items.Clear();
            lock (Spines)
            {
                foreach (var spine in spines) spine.Dispose();
                spines.Clear();
                listView.SmallImageList.Images.Clear();
                listView.LargeImageList.Images.Clear();
            }
            if (PropertyGrid is not null)
                PropertyGrid.SelectedObject = null;
        }

        private void toolStripMenuItem_CopyPreview_Click(object sender, EventArgs e)
        {
            var fileDropList = new StringCollection();

            lock (Spines)
            {
                foreach (int i in listView.SelectedIndices)
                {
                    var spine = spines[i];
                    var image = spine.Preview;
                    var path = Path.Combine(Program.TempDir, $"{spine.ID}.png");
                    using (var clone = new Bitmap(image))
                        clone.Save(path);
                    fileDropList.Add(path);
                }
            }
            if (fileDropList.Count > 0)
                Clipboard.SetFileDropList(fileDropList);
        }

        private void toolStripMenuItem_AddFromClipboard_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsFileDropList())
            {
                var fileDropList = Clipboard.GetFileDropList();
                var paths = new string[fileDropList.Count];
                fileDropList.CopyTo(paths, 0);
                AddFromFileDrop(paths);
            }
        }

        private void toolStripMenuItem_SelectAll_Click(object sender, EventArgs e)
        {
            listView.BeginUpdate();
            foreach (ListViewItem item in listView.Items)
                item.Selected = true;
            listView.EndUpdate();
        }

        private void toolStripMenuItem_LargeIconView_Click(object sender, EventArgs e)
        {
            listView.View = View.LargeIcon;
        }

        private void toolStripMenuItem_ListView_Click(object sender, EventArgs e)
        {
            listView.View = View.List;
        }

        private void toolStripMenuItem_DetailsView_Click(object sender, EventArgs e)
        {
            listView.View = View.Details;
        }
    }
}
