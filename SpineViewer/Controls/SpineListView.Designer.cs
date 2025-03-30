namespace SpineViewer.Controls
{
    partial class SpineListView
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            listView = new ListView();
            columnHeader_Name = new ColumnHeader();
            contextMenuStrip = new ContextMenuStrip(components);
            toolStripMenuItem_Add = new ToolStripMenuItem();
            toolStripMenuItem_Insert = new ToolStripMenuItem();
            toolStripMenuItem_Remove = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItem_BatchAdd = new ToolStripMenuItem();
            toolStripMenuItem_RemoveAll = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripMenuItem_MoveUp = new ToolStripMenuItem();
            toolStripMenuItem_MoveDown = new ToolStripMenuItem();
            toolStripMenuItem_MoveTop = new ToolStripMenuItem();
            toolStripMenuItem_MoveBottom = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripMenuItem_CopyPreview = new ToolStripMenuItem();
            toolStripMenuItem_AddFromClipboard = new ToolStripMenuItem();
            toolStripMenuItem_SelectAll = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripMenuItem_ChangeView = new ToolStripMenuItem();
            toolStripMenuItem_LargeIconView = new ToolStripMenuItem();
            toolStripMenuItem_ListView = new ToolStripMenuItem();
            toolStripMenuItem_DetailsView = new ToolStripMenuItem();
            imageList_LargeIcon = new ImageList(components);
            imageList_SmallIcon = new ImageList(components);
            timer_SelectedIndexChangedDebounce = new System.Windows.Forms.Timer(components);
            statusStrip = new StatusStrip();
            toolStripStatusLabel_CountInfo = new ToolStripStatusLabel();
            tableLayoutPanel = new TableLayoutPanel();
            contextMenuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // listView
            // 
            listView.Alignment = ListViewAlignment.Left;
            listView.AllowDrop = true;
            listView.Columns.AddRange(new ColumnHeader[] { columnHeader_Name });
            listView.ContextMenuStrip = contextMenuStrip;
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.LargeImageList = imageList_LargeIcon;
            listView.Location = new Point(0, 0);
            listView.Margin = new Padding(0);
            listView.Name = "listView";
            listView.ShowItemToolTips = true;
            listView.Size = new Size(336, 414);
            listView.SmallImageList = imageList_SmallIcon;
            listView.TabIndex = 1;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.ItemDrag += listView_ItemDrag;
            listView.SelectedIndexChanged += listView_SelectedIndexChanged;
            listView.DragDrop += listView_DragDrop;
            listView.DragEnter += listView_DragEnter;
            listView.DragOver += listView_DragOver;
            // 
            // columnHeader_Name
            // 
            columnHeader_Name.Text = "名称";
            columnHeader_Name.Width = 300;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.ImageScalingSize = new Size(24, 24);
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_Add, toolStripMenuItem_Insert, toolStripMenuItem_Remove, toolStripSeparator1, toolStripMenuItem_BatchAdd, toolStripMenuItem_RemoveAll, toolStripSeparator2, toolStripMenuItem_MoveUp, toolStripMenuItem_MoveDown, toolStripMenuItem_MoveTop, toolStripMenuItem_MoveBottom, toolStripSeparator3, toolStripMenuItem_CopyPreview, toolStripMenuItem_AddFromClipboard, toolStripMenuItem_SelectAll, toolStripSeparator4, toolStripMenuItem_ChangeView });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(329, 418);
            contextMenuStrip.Closed += contextMenuStrip_Closed;
            contextMenuStrip.Opening += contextMenuStrip_Opening;
            // 
            // toolStripMenuItem_Add
            // 
            toolStripMenuItem_Add.Name = "toolStripMenuItem_Add";
            toolStripMenuItem_Add.Size = new Size(328, 30);
            toolStripMenuItem_Add.Text = "添加...";
            toolStripMenuItem_Add.Click += toolStripMenuItem_Add_Click;
            // 
            // toolStripMenuItem_Insert
            // 
            toolStripMenuItem_Insert.Name = "toolStripMenuItem_Insert";
            toolStripMenuItem_Insert.Size = new Size(328, 30);
            toolStripMenuItem_Insert.Text = "插入...";
            toolStripMenuItem_Insert.Click += toolStripMenuItem_Insert_Click;
            // 
            // toolStripMenuItem_Remove
            // 
            toolStripMenuItem_Remove.Name = "toolStripMenuItem_Remove";
            toolStripMenuItem_Remove.ShortcutKeys = Keys.Delete;
            toolStripMenuItem_Remove.Size = new Size(328, 30);
            toolStripMenuItem_Remove.Text = "移除";
            toolStripMenuItem_Remove.Click += toolStripMenuItem_Remove_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(325, 6);
            // 
            // toolStripMenuItem_BatchAdd
            // 
            toolStripMenuItem_BatchAdd.Name = "toolStripMenuItem_BatchAdd";
            toolStripMenuItem_BatchAdd.Size = new Size(328, 30);
            toolStripMenuItem_BatchAdd.Text = "批量添加...";
            toolStripMenuItem_BatchAdd.Click += toolStripMenuItem_BatchAdd_Click;
            // 
            // toolStripMenuItem_RemoveAll
            // 
            toolStripMenuItem_RemoveAll.Name = "toolStripMenuItem_RemoveAll";
            toolStripMenuItem_RemoveAll.Size = new Size(328, 30);
            toolStripMenuItem_RemoveAll.Text = "移除全部";
            toolStripMenuItem_RemoveAll.Click += toolStripMenuItem_RemoveAll_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(325, 6);
            // 
            // toolStripMenuItem_MoveUp
            // 
            toolStripMenuItem_MoveUp.Name = "toolStripMenuItem_MoveUp";
            toolStripMenuItem_MoveUp.ShortcutKeys = Keys.Alt | Keys.W;
            toolStripMenuItem_MoveUp.Size = new Size(328, 30);
            toolStripMenuItem_MoveUp.Text = "上移";
            toolStripMenuItem_MoveUp.Click += toolStripMenuItem_MoveUp_Click;
            // 
            // toolStripMenuItem_MoveDown
            // 
            toolStripMenuItem_MoveDown.Name = "toolStripMenuItem_MoveDown";
            toolStripMenuItem_MoveDown.ShortcutKeys = Keys.Alt | Keys.S;
            toolStripMenuItem_MoveDown.Size = new Size(328, 30);
            toolStripMenuItem_MoveDown.Text = "下移";
            toolStripMenuItem_MoveDown.Click += toolStripMenuItem_MoveDown_Click;
            // 
            // toolStripMenuItem_MoveTop
            // 
            toolStripMenuItem_MoveTop.Name = "toolStripMenuItem_MoveTop";
            toolStripMenuItem_MoveTop.ShortcutKeys = Keys.Alt | Keys.Shift | Keys.W;
            toolStripMenuItem_MoveTop.Size = new Size(328, 30);
            toolStripMenuItem_MoveTop.Text = "置顶";
            toolStripMenuItem_MoveTop.Click += toolStripMenuItem_MoveTop_Click;
            // 
            // toolStripMenuItem_MoveBottom
            // 
            toolStripMenuItem_MoveBottom.Name = "toolStripMenuItem_MoveBottom";
            toolStripMenuItem_MoveBottom.ShortcutKeys = Keys.Alt | Keys.Shift | Keys.S;
            toolStripMenuItem_MoveBottom.Size = new Size(328, 30);
            toolStripMenuItem_MoveBottom.Text = "置底";
            toolStripMenuItem_MoveBottom.Click += toolStripMenuItem_MoveBottom_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(325, 6);
            // 
            // toolStripMenuItem_CopyPreview
            // 
            toolStripMenuItem_CopyPreview.Name = "toolStripMenuItem_CopyPreview";
            toolStripMenuItem_CopyPreview.ShortcutKeys = Keys.Control | Keys.C;
            toolStripMenuItem_CopyPreview.Size = new Size(328, 30);
            toolStripMenuItem_CopyPreview.Text = "复制预览图 (256x256)";
            toolStripMenuItem_CopyPreview.Click += toolStripMenuItem_CopyPreview_Click;
            // 
            // toolStripMenuItem_AddFromClipboard
            // 
            toolStripMenuItem_AddFromClipboard.Name = "toolStripMenuItem_AddFromClipboard";
            toolStripMenuItem_AddFromClipboard.ShortcutKeys = Keys.Control | Keys.V;
            toolStripMenuItem_AddFromClipboard.Size = new Size(328, 30);
            toolStripMenuItem_AddFromClipboard.Text = "从剪贴板添加";
            toolStripMenuItem_AddFromClipboard.Click += toolStripMenuItem_AddFromClipboard_Click;
            // 
            // toolStripMenuItem_SelectAll
            // 
            toolStripMenuItem_SelectAll.Name = "toolStripMenuItem_SelectAll";
            toolStripMenuItem_SelectAll.ShortcutKeys = Keys.Control | Keys.A;
            toolStripMenuItem_SelectAll.Size = new Size(328, 30);
            toolStripMenuItem_SelectAll.Text = "全选";
            toolStripMenuItem_SelectAll.Click += toolStripMenuItem_SelectAll_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(325, 6);
            // 
            // toolStripMenuItem_ChangeView
            // 
            toolStripMenuItem_ChangeView.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_LargeIconView, toolStripMenuItem_ListView, toolStripMenuItem_DetailsView });
            toolStripMenuItem_ChangeView.Name = "toolStripMenuItem_ChangeView";
            toolStripMenuItem_ChangeView.Size = new Size(328, 30);
            toolStripMenuItem_ChangeView.Text = "切换视图";
            // 
            // toolStripMenuItem_LargeIconView
            // 
            toolStripMenuItem_LargeIconView.Name = "toolStripMenuItem_LargeIconView";
            toolStripMenuItem_LargeIconView.ShortcutKeys = Keys.Alt | Keys.D1;
            toolStripMenuItem_LargeIconView.Size = new Size(241, 34);
            toolStripMenuItem_LargeIconView.Text = "大图标";
            toolStripMenuItem_LargeIconView.Click += toolStripMenuItem_LargeIconView_Click;
            // 
            // toolStripMenuItem_ListView
            // 
            toolStripMenuItem_ListView.Name = "toolStripMenuItem_ListView";
            toolStripMenuItem_ListView.ShortcutKeys = Keys.Alt | Keys.D2;
            toolStripMenuItem_ListView.Size = new Size(241, 34);
            toolStripMenuItem_ListView.Text = "列表";
            toolStripMenuItem_ListView.Click += toolStripMenuItem_ListView_Click;
            // 
            // toolStripMenuItem_DetailsView
            // 
            toolStripMenuItem_DetailsView.Name = "toolStripMenuItem_DetailsView";
            toolStripMenuItem_DetailsView.ShortcutKeys = Keys.Alt | Keys.D3;
            toolStripMenuItem_DetailsView.Size = new Size(241, 34);
            toolStripMenuItem_DetailsView.Text = "详细信息";
            toolStripMenuItem_DetailsView.Click += toolStripMenuItem_DetailsView_Click;
            // 
            // imageList_LargeIcon
            // 
            imageList_LargeIcon.ColorDepth = ColorDepth.Depth32Bit;
            imageList_LargeIcon.ImageSize = new Size(96, 96);
            imageList_LargeIcon.TransparentColor = Color.Transparent;
            // 
            // imageList_SmallIcon
            // 
            imageList_SmallIcon.ColorDepth = ColorDepth.Depth32Bit;
            imageList_SmallIcon.ImageSize = new Size(48, 48);
            imageList_SmallIcon.TransparentColor = Color.Transparent;
            // 
            // timer_SelectedIndexChangedDebounce
            // 
            timer_SelectedIndexChangedDebounce.Interval = 30;
            timer_SelectedIndexChangedDebounce.Tick += timer_SelectedIndexChangedDebounce_Tick;
            // 
            // statusStrip
            // 
            statusStrip.Dock = DockStyle.Fill;
            statusStrip.ImageScalingSize = new Size(24, 24);
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel_CountInfo });
            statusStrip.Location = new Point(0, 414);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(336, 31);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 2;
            statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_CountInfo
            // 
            toolStripStatusLabel_CountInfo.Name = "toolStripStatusLabel_CountInfo";
            toolStripStatusLabel_CountInfo.Size = new Size(178, 24);
            toolStripStatusLabel_CountInfo.Text = "已选择 0 项，共 0 项";
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(listView, 0, 0);
            tableLayoutPanel.Controls.Add(statusStrip, 0, 1);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.Size = new Size(336, 445);
            tableLayoutPanel.TabIndex = 3;
            // 
            // SpineListView
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel);
            Name = "SpineListView";
            Size = new Size(336, 445);
            contextMenuStrip.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem toolStripMenuItem_Add;
        private ToolStripMenuItem toolStripMenuItem_Insert;
        private ToolStripMenuItem toolStripMenuItem_Remove;
        private ToolStripMenuItem toolStripMenuItem_RemoveAll;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItem_BatchAdd;
        private ListView listView;
        private ToolStripMenuItem toolStripMenuItem_MoveUp;
        private ToolStripMenuItem toolStripMenuItem_MoveDown;
        private ToolStripSeparator toolStripSeparator2;
        private ColumnHeader columnHeader_Name;
        private ImageList imageList_SmallIcon;
        private ImageList imageList_LargeIcon;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem toolStripMenuItem_ChangeView;
        private ToolStripMenuItem toolStripMenuItem_LargeIconView;
        private ToolStripMenuItem toolStripMenuItem_ListView;
        private ToolStripMenuItem toolStripMenuItem_DetailsView;
        private ToolStripMenuItem toolStripMenuItem_MoveTop;
        private ToolStripMenuItem toolStripMenuItem_MoveBottom;
        private ToolStripMenuItem toolStripMenuItem_CopyPreview;
        private ToolStripMenuItem toolStripMenuItem_SelectAll;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem toolStripMenuItem_AddFromClipboard;
        private System.Windows.Forms.Timer timer_SelectedIndexChangedDebounce;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel_CountInfo;
        private TableLayoutPanel tableLayoutPanel;
    }
}
