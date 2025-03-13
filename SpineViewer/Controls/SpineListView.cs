using System;
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

namespace SpineViewer.Controls
{
    public partial class SpineListView : UserControl
    {
        [Category("自定义"), Description("用于显示骨骼属性的属性页")]
        public PropertyGrid? PropertyGrid { get; set; }

        /// <summary>
        /// 获取数组快照, 访问时必须使用 lock 语句锁定对象本身
        /// </summary>
        public readonly ReadOnlyCollection<Spine.Spine> Spines;

        /// <summary>
        /// Spine 列表, 访问时必须使用 lock 语句锁定 Spines
        /// </summary>
        private readonly List<Spine.Spine> spines = [];

        public SpineListView()
        {
            InitializeComponent();
            Spines = spines.AsReadOnly();
        }

        /// <summary>
        /// listView.SelectedIndices
        /// </summary>
        public ListView.SelectedIndexCollection SelectedIndices { get => listView.SelectedIndices; }

        /// <summary>
        /// 弹出添加对话框在指定位置之前插入一项
        /// </summary>
        private void Insert(int index = -1)
        {
            var dialog = new Dialogs.OpenSpineDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                var spine = Spine.Spine.New(dialog.Version, dialog.SkelPath, dialog.AtlasPath);

                // 如果索引无效则在末尾添加
                if (index < 0 || index > listView.Items.Count)
                    index = listView.Items.Count;

                // 锁定外部的读操作
                lock (Spines) { spines.Insert(index, spine); }
                listView.Items.Insert(index, new ListViewItem(spine.Name) { ToolTipText = spine.SkelPath });

                // 选中新增项
                listView.SelectedIndices.Clear();
                listView.SelectedIndices.Add(index);
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex.ToString());
                Program.Logger.Error("Failed to load {} {}", dialog.SkelPath, dialog.AtlasPath);
                if (ex is NotImplementedException)
                    MessageBox.Show($"{dialog.Version.String()} 版本尚未实现（咕咕咕~）", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(ex.ToString(), "骨骼加载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Program.Logger.Info($"Current memory usage: {Program.Process.WorkingSet64 / 1024.0 / 1024.0:F2} MB");
        }

        /// <summary>
        /// 弹出添加对话框
        /// </summary>
        public void Add()
        {
            Insert();
        }

        /// <summary>
        /// 弹出批量添加对话框
        /// </summary>
        public void BatchAdd()
        {
            var openDialog = new Dialogs.BatchOpenSpineDialog();
            if (openDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += BatchAdd_Work;
            progressDialog.RunWorkerAsync(openDialog);
            progressDialog.ShowDialog();
        }

        private void BatchAdd_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as Dialogs.BatchOpenSpineDialog;
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
                    lock (Spines) { spines.Add(spine); }
                    listView.Invoke(() => listView.Items.Add(new ListViewItem(spine.Name) { ToolTipText = spine.SkelPath }));
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

            Program.Logger.Info($"Current memory usage: {Program.Process.WorkingSet64 / 1024.0 / 1024.0:F2} MB");
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
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                listView.BeginUpdate();
                foreach (ListViewItem item in listView.Items)
                {
                    item.Selected = true;
                }
                listView.EndUpdate();
            }
        }

        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void listView_DragOver(object sender, DragEventArgs e)
        {
            // 检查拖放目标是否有效
            e.Effect = DragDropEffects.Move;

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

        private void listView_DragDrop(object sender, DragEventArgs e)
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

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var selectedCount = listView.SelectedIndices.Count;
            var itemsCount = listView.Items.Count;
            toolStripMenuItem_Insert.Enabled = selectedCount == 1;
            toolStripMenuItem_Remove.Enabled = selectedCount >= 1;
            toolStripMenuItem_MoveUp.Enabled = selectedCount == 1 && listView.SelectedIndices[0] != 0;
            toolStripMenuItem_MoveDown.Enabled = selectedCount == 1 && listView.SelectedIndices[0] != itemsCount - 1;
            toolStripMenuItem_RemoveAll.Enabled = itemsCount > 0;
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
                if (MessageBox.Show($"确定移除所选 {listView.SelectedIndices.Count} 项吗？", "操作确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    return;
            }

            foreach (var i in listView.SelectedIndices.Cast<int>().OrderByDescending(x => x))
            {
                lock (Spines)
                {
                    spines[i].Dispose();
                    spines.RemoveAt(i);
                }
                listView.Items.RemoveAt(i);
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
                listView.Items.RemoveAt(index);
                listView.Items.Insert(index - 1, item);
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
                var item = listView.Items[index + 1];
                listView.Items.RemoveAt(index + 1);
                listView.Items.Insert(index, item);
            }
        }

        private void toolStripMenuItem_RemoveAll_Click(object sender, EventArgs e)
        {
            if (listView.Items.Count <= 0)
                return;

            if (MessageBox.Show($"确认移除所有 {listView.Items.Count} 项吗？", "操作确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                return;

            lock (Spines) 
            {
                foreach (var spine in spines)
                    spine.Dispose();
                spines.Clear(); 
            }
            listView.Items.Clear();
            if (PropertyGrid is not null)
                PropertyGrid.SelectedObject = null;
        }
    }
}
