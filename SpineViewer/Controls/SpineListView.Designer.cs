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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpineListView));
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
            resources.ApplyResources(listView, "listView");
            listView.AllowDrop = true;
            listView.Columns.AddRange(new ColumnHeader[] { columnHeader_Name });
            listView.ContextMenuStrip = contextMenuStrip;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.LargeImageList = imageList_LargeIcon;
            listView.Name = "listView";
            listView.ShowItemToolTips = true;
            listView.SmallImageList = imageList_SmallIcon;
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
            resources.ApplyResources(columnHeader_Name, "columnHeader_Name");
            // 
            // contextMenuStrip
            // 
            resources.ApplyResources(contextMenuStrip, "contextMenuStrip");
            contextMenuStrip.ImageScalingSize = new Size(24, 24);
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_Add, toolStripMenuItem_Insert, toolStripMenuItem_Remove, toolStripSeparator1, toolStripMenuItem_BatchAdd, toolStripMenuItem_RemoveAll, toolStripSeparator2, toolStripMenuItem_MoveUp, toolStripMenuItem_MoveDown, toolStripMenuItem_MoveTop, toolStripMenuItem_MoveBottom, toolStripSeparator3, toolStripMenuItem_CopyPreview, toolStripMenuItem_AddFromClipboard, toolStripMenuItem_SelectAll, toolStripSeparator4, toolStripMenuItem_ChangeView });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Closed += contextMenuStrip_Closed;
            contextMenuStrip.Opening += contextMenuStrip_Opening;
            // 
            // toolStripMenuItem_Add
            // 
            resources.ApplyResources(toolStripMenuItem_Add, "toolStripMenuItem_Add");
            toolStripMenuItem_Add.Name = "toolStripMenuItem_Add";
            toolStripMenuItem_Add.Click += toolStripMenuItem_Add_Click;
            // 
            // toolStripMenuItem_Insert
            // 
            resources.ApplyResources(toolStripMenuItem_Insert, "toolStripMenuItem_Insert");
            toolStripMenuItem_Insert.Name = "toolStripMenuItem_Insert";
            toolStripMenuItem_Insert.Click += toolStripMenuItem_Insert_Click;
            // 
            // toolStripMenuItem_Remove
            // 
            resources.ApplyResources(toolStripMenuItem_Remove, "toolStripMenuItem_Remove");
            toolStripMenuItem_Remove.Name = "toolStripMenuItem_Remove";
            toolStripMenuItem_Remove.Click += toolStripMenuItem_Remove_Click;
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // toolStripMenuItem_BatchAdd
            // 
            resources.ApplyResources(toolStripMenuItem_BatchAdd, "toolStripMenuItem_BatchAdd");
            toolStripMenuItem_BatchAdd.Name = "toolStripMenuItem_BatchAdd";
            toolStripMenuItem_BatchAdd.Click += toolStripMenuItem_BatchAdd_Click;
            // 
            // toolStripMenuItem_RemoveAll
            // 
            resources.ApplyResources(toolStripMenuItem_RemoveAll, "toolStripMenuItem_RemoveAll");
            toolStripMenuItem_RemoveAll.Name = "toolStripMenuItem_RemoveAll";
            toolStripMenuItem_RemoveAll.Click += toolStripMenuItem_RemoveAll_Click;
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // toolStripMenuItem_MoveUp
            // 
            resources.ApplyResources(toolStripMenuItem_MoveUp, "toolStripMenuItem_MoveUp");
            toolStripMenuItem_MoveUp.Name = "toolStripMenuItem_MoveUp";
            toolStripMenuItem_MoveUp.Click += toolStripMenuItem_MoveUp_Click;
            // 
            // toolStripMenuItem_MoveDown
            // 
            resources.ApplyResources(toolStripMenuItem_MoveDown, "toolStripMenuItem_MoveDown");
            toolStripMenuItem_MoveDown.Name = "toolStripMenuItem_MoveDown";
            toolStripMenuItem_MoveDown.Click += toolStripMenuItem_MoveDown_Click;
            // 
            // toolStripMenuItem_MoveTop
            // 
            resources.ApplyResources(toolStripMenuItem_MoveTop, "toolStripMenuItem_MoveTop");
            toolStripMenuItem_MoveTop.Name = "toolStripMenuItem_MoveTop";
            toolStripMenuItem_MoveTop.Click += toolStripMenuItem_MoveTop_Click;
            // 
            // toolStripMenuItem_MoveBottom
            // 
            resources.ApplyResources(toolStripMenuItem_MoveBottom, "toolStripMenuItem_MoveBottom");
            toolStripMenuItem_MoveBottom.Name = "toolStripMenuItem_MoveBottom";
            toolStripMenuItem_MoveBottom.Click += toolStripMenuItem_MoveBottom_Click;
            // 
            // toolStripSeparator3
            // 
            resources.ApplyResources(toolStripSeparator3, "toolStripSeparator3");
            toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // toolStripMenuItem_CopyPreview
            // 
            resources.ApplyResources(toolStripMenuItem_CopyPreview, "toolStripMenuItem_CopyPreview");
            toolStripMenuItem_CopyPreview.Name = "toolStripMenuItem_CopyPreview";
            toolStripMenuItem_CopyPreview.Click += toolStripMenuItem_CopyPreview_Click;
            // 
            // toolStripMenuItem_AddFromClipboard
            // 
            resources.ApplyResources(toolStripMenuItem_AddFromClipboard, "toolStripMenuItem_AddFromClipboard");
            toolStripMenuItem_AddFromClipboard.Name = "toolStripMenuItem_AddFromClipboard";
            toolStripMenuItem_AddFromClipboard.Click += toolStripMenuItem_AddFromClipboard_Click;
            // 
            // toolStripMenuItem_SelectAll
            // 
            resources.ApplyResources(toolStripMenuItem_SelectAll, "toolStripMenuItem_SelectAll");
            toolStripMenuItem_SelectAll.Name = "toolStripMenuItem_SelectAll";
            toolStripMenuItem_SelectAll.Click += toolStripMenuItem_SelectAll_Click;
            // 
            // toolStripSeparator4
            // 
            resources.ApplyResources(toolStripSeparator4, "toolStripSeparator4");
            toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // toolStripMenuItem_ChangeView
            // 
            resources.ApplyResources(toolStripMenuItem_ChangeView, "toolStripMenuItem_ChangeView");
            toolStripMenuItem_ChangeView.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_LargeIconView, toolStripMenuItem_ListView, toolStripMenuItem_DetailsView });
            toolStripMenuItem_ChangeView.Name = "toolStripMenuItem_ChangeView";
            // 
            // toolStripMenuItem_LargeIconView
            // 
            resources.ApplyResources(toolStripMenuItem_LargeIconView, "toolStripMenuItem_LargeIconView");
            toolStripMenuItem_LargeIconView.Name = "toolStripMenuItem_LargeIconView";
            toolStripMenuItem_LargeIconView.Click += toolStripMenuItem_LargeIconView_Click;
            // 
            // toolStripMenuItem_ListView
            // 
            resources.ApplyResources(toolStripMenuItem_ListView, "toolStripMenuItem_ListView");
            toolStripMenuItem_ListView.Name = "toolStripMenuItem_ListView";
            toolStripMenuItem_ListView.Click += toolStripMenuItem_ListView_Click;
            // 
            // toolStripMenuItem_DetailsView
            // 
            resources.ApplyResources(toolStripMenuItem_DetailsView, "toolStripMenuItem_DetailsView");
            toolStripMenuItem_DetailsView.Name = "toolStripMenuItem_DetailsView";
            toolStripMenuItem_DetailsView.Click += toolStripMenuItem_DetailsView_Click;
            // 
            // imageList_LargeIcon
            // 
            imageList_LargeIcon.ColorDepth = ColorDepth.Depth32Bit;
            resources.ApplyResources(imageList_LargeIcon, "imageList_LargeIcon");
            imageList_LargeIcon.TransparentColor = Color.Transparent;
            // 
            // imageList_SmallIcon
            // 
            imageList_SmallIcon.ColorDepth = ColorDepth.Depth32Bit;
            resources.ApplyResources(imageList_SmallIcon, "imageList_SmallIcon");
            imageList_SmallIcon.TransparentColor = Color.Transparent;
            // 
            // timer_SelectedIndexChangedDebounce
            // 
            timer_SelectedIndexChangedDebounce.Interval = 30;
            timer_SelectedIndexChangedDebounce.Tick += timer_SelectedIndexChangedDebounce_Tick;
            // 
            // statusStrip
            // 
            resources.ApplyResources(statusStrip, "statusStrip");
            statusStrip.ImageScalingSize = new Size(24, 24);
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel_CountInfo });
            statusStrip.Name = "statusStrip";
            statusStrip.SizingGrip = false;
            // 
            // toolStripStatusLabel_CountInfo
            // 
            resources.ApplyResources(toolStripStatusLabel_CountInfo, "toolStripStatusLabel_CountInfo");
            toolStripStatusLabel_CountInfo.Name = "toolStripStatusLabel_CountInfo";
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(tableLayoutPanel, "tableLayoutPanel");
            tableLayoutPanel.Controls.Add(listView, 0, 0);
            tableLayoutPanel.Controls.Add(statusStrip, 0, 1);
            tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // SpineListView
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel);
            Name = "SpineListView";
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
