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
            toolStripMenuItem_MoveUp = new ToolStripMenuItem();
            toolStripMenuItem_MoveDown = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripMenuItem_BatchAdd = new ToolStripMenuItem();
            toolStripMenuItem_RemoveAll = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripMenuItem_ChangeView = new ToolStripMenuItem();
            toolStripMenuItem_LargeIconView = new ToolStripMenuItem();
            toolStripMenuItem_SmallIconView = new ToolStripMenuItem();
            toolStripMenuItem_DetailsView = new ToolStripMenuItem();
            imageList_LargeIcon = new ImageList(components);
            imageList_SmallIcon = new ImageList(components);
            contextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // listView
            // 
            listView.AllowDrop = true;
            listView.Columns.AddRange(new ColumnHeader[] { columnHeader_Name });
            listView.ContextMenuStrip = contextMenuStrip;
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.LargeImageList = imageList_LargeIcon;
            listView.Location = new Point(0, 0);
            listView.Name = "listView";
            listView.ShowItemToolTips = true;
            listView.Size = new Size(336, 445);
            listView.SmallImageList = imageList_SmallIcon;
            listView.TabIndex = 1;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.ItemDrag += listView_ItemDrag;
            listView.SelectedIndexChanged += listView_SelectedIndexChanged;
            listView.DragDrop += listView_DragDrop;
            listView.DragOver += listView_DragOver;
            listView.KeyDown += listView_KeyDown;
            // 
            // columnHeader_Name
            // 
            columnHeader_Name.Text = "名称";
            columnHeader_Name.Width = 220;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.ImageScalingSize = new Size(24, 24);
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_Add, toolStripMenuItem_Insert, toolStripMenuItem_Remove, toolStripSeparator1, toolStripMenuItem_MoveUp, toolStripMenuItem_MoveDown, toolStripSeparator2, toolStripMenuItem_BatchAdd, toolStripMenuItem_RemoveAll, toolStripSeparator3, toolStripMenuItem_ChangeView });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(188, 262);
            contextMenuStrip.Opening += contextMenuStrip_Opening;
            // 
            // toolStripMenuItem_Add
            // 
            toolStripMenuItem_Add.Name = "toolStripMenuItem_Add";
            toolStripMenuItem_Add.Size = new Size(187, 30);
            toolStripMenuItem_Add.Text = "添加(&A)...";
            toolStripMenuItem_Add.Click += toolStripMenuItem_Add_Click;
            // 
            // toolStripMenuItem_Insert
            // 
            toolStripMenuItem_Insert.Enabled = false;
            toolStripMenuItem_Insert.Name = "toolStripMenuItem_Insert";
            toolStripMenuItem_Insert.Size = new Size(187, 30);
            toolStripMenuItem_Insert.Text = "插入(&I)...";
            toolStripMenuItem_Insert.Click += toolStripMenuItem_Insert_Click;
            // 
            // toolStripMenuItem_Remove
            // 
            toolStripMenuItem_Remove.Enabled = false;
            toolStripMenuItem_Remove.Name = "toolStripMenuItem_Remove";
            toolStripMenuItem_Remove.Size = new Size(187, 30);
            toolStripMenuItem_Remove.Text = "移除(&R)";
            toolStripMenuItem_Remove.Click += toolStripMenuItem_Remove_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(184, 6);
            // 
            // toolStripMenuItem_MoveUp
            // 
            toolStripMenuItem_MoveUp.Name = "toolStripMenuItem_MoveUp";
            toolStripMenuItem_MoveUp.Size = new Size(187, 30);
            toolStripMenuItem_MoveUp.Text = "上移(&U)";
            toolStripMenuItem_MoveUp.Click += toolStripMenuItem_MoveUp_Click;
            // 
            // toolStripMenuItem_MoveDown
            // 
            toolStripMenuItem_MoveDown.Name = "toolStripMenuItem_MoveDown";
            toolStripMenuItem_MoveDown.Size = new Size(187, 30);
            toolStripMenuItem_MoveDown.Text = "下移(&D)";
            toolStripMenuItem_MoveDown.Click += toolStripMenuItem_MoveDown_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(184, 6);
            // 
            // toolStripMenuItem_BatchAdd
            // 
            toolStripMenuItem_BatchAdd.Name = "toolStripMenuItem_BatchAdd";
            toolStripMenuItem_BatchAdd.Size = new Size(187, 30);
            toolStripMenuItem_BatchAdd.Text = "批量添加(&B)...";
            toolStripMenuItem_BatchAdd.Click += toolStripMenuItem_BatchAdd_Click;
            // 
            // toolStripMenuItem_RemoveAll
            // 
            toolStripMenuItem_RemoveAll.Enabled = false;
            toolStripMenuItem_RemoveAll.Name = "toolStripMenuItem_RemoveAll";
            toolStripMenuItem_RemoveAll.Size = new Size(187, 30);
            toolStripMenuItem_RemoveAll.Text = "移除全部(&X)";
            toolStripMenuItem_RemoveAll.Click += toolStripMenuItem_RemoveAll_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(184, 6);
            // 
            // toolStripMenuItem_ChangeView
            // 
            toolStripMenuItem_ChangeView.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_LargeIconView, toolStripMenuItem_SmallIconView, toolStripMenuItem_DetailsView });
            toolStripMenuItem_ChangeView.Name = "toolStripMenuItem_ChangeView";
            toolStripMenuItem_ChangeView.Size = new Size(187, 30);
            toolStripMenuItem_ChangeView.Text = "切换视图";
            // 
            // toolStripMenuItem_LargeIconView
            // 
            toolStripMenuItem_LargeIconView.Name = "toolStripMenuItem_LargeIconView";
            toolStripMenuItem_LargeIconView.Size = new Size(164, 34);
            toolStripMenuItem_LargeIconView.Text = "大图标";
            toolStripMenuItem_LargeIconView.Click += toolStripMenuItem_LargeIconView_Click;
            // 
            // toolStripMenuItem_SmallIconView
            // 
            toolStripMenuItem_SmallIconView.Name = "toolStripMenuItem_SmallIconView";
            toolStripMenuItem_SmallIconView.Size = new Size(164, 34);
            toolStripMenuItem_SmallIconView.Text = "小图标";
            toolStripMenuItem_SmallIconView.Click += toolStripMenuItem_SmallIconView_Click;
            // 
            // toolStripMenuItem_DetailsView
            // 
            toolStripMenuItem_DetailsView.Name = "toolStripMenuItem_DetailsView";
            toolStripMenuItem_DetailsView.Size = new Size(164, 34);
            toolStripMenuItem_DetailsView.Text = "列表";
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
            // SpineListView
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(listView);
            Name = "SpineListView";
            Size = new Size(336, 445);
            contextMenuStrip.ResumeLayout(false);
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
        private ToolStripMenuItem toolStripMenuItem_SmallIconView;
        private ToolStripMenuItem toolStripMenuItem_DetailsView;
    }
}
