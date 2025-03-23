namespace SpineViewer.Controls
{
    partial class SkelFileListBox
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
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            button_AddFolder = new Button();
            button_AddFile = new Button();
            label_Tip = new Label();
            listBox = new ListBox();
            contextMenuStrip = new ContextMenuStrip(components);
            toolStripMenuItem_SelectAll = new ToolStripMenuItem();
            toolStripMenuItem_Paste = new ToolStripMenuItem();
            toolStripMenuItem_Remove = new ToolStripMenuItem();
            folderBrowserDialog = new FolderBrowserDialog();
            openFileDialog_Skel = new OpenFileDialog();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            contextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel1.Controls.Add(listBox, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(801, 394);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(button_AddFolder);
            flowLayoutPanel1.Controls.Add(button_AddFile);
            flowLayoutPanel1.Controls.Add(label_Tip);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(795, 40);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // button_AddFolder
            // 
            button_AddFolder.AutoSize = true;
            button_AddFolder.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_AddFolder.Location = new Point(3, 3);
            button_AddFolder.Name = "button_AddFolder";
            button_AddFolder.Size = new Size(122, 34);
            button_AddFolder.TabIndex = 0;
            button_AddFolder.Text = "添加文件夹...";
            button_AddFolder.UseVisualStyleBackColor = true;
            button_AddFolder.Click += button_AddFolder_Click;
            // 
            // button_AddFile
            // 
            button_AddFile.AutoSize = true;
            button_AddFile.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_AddFile.Location = new Point(131, 3);
            button_AddFile.Name = "button_AddFile";
            button_AddFile.Size = new Size(104, 34);
            button_AddFile.TabIndex = 1;
            button_AddFile.Text = "添加文件...";
            button_AddFile.UseVisualStyleBackColor = true;
            button_AddFile.Click += button_AddFile_Click;
            // 
            // label_Tip
            // 
            label_Tip.Anchor = AnchorStyles.Left;
            label_Tip.AutoSize = true;
            label_Tip.Location = new Point(241, 8);
            label_Tip.Name = "label_Tip";
            label_Tip.Size = new Size(139, 24);
            label_Tip.TabIndex = 3;
            label_Tip.Text = "已添加 0 个文件";
            label_Tip.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // listBox
            // 
            listBox.AllowDrop = true;
            listBox.ContextMenuStrip = contextMenuStrip;
            listBox.Dock = DockStyle.Fill;
            listBox.FormattingEnabled = true;
            listBox.HorizontalScrollbar = true;
            listBox.ItemHeight = 24;
            listBox.Location = new Point(3, 49);
            listBox.Name = "listBox";
            listBox.SelectionMode = SelectionMode.MultiExtended;
            listBox.Size = new Size(795, 342);
            listBox.TabIndex = 0;
            listBox.DragDrop += listBox_DragDrop;
            listBox.DragEnter += listBox_DragEnter;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.ImageScalingSize = new Size(24, 24);
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_SelectAll, toolStripMenuItem_Paste, toolStripMenuItem_Remove });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(184, 94);
            // 
            // toolStripMenuItem_SelectAll
            // 
            toolStripMenuItem_SelectAll.Name = "toolStripMenuItem_SelectAll";
            toolStripMenuItem_SelectAll.ShortcutKeys = Keys.Control | Keys.A;
            toolStripMenuItem_SelectAll.Size = new Size(183, 30);
            toolStripMenuItem_SelectAll.Text = "全选";
            toolStripMenuItem_SelectAll.Click += toolStripMenuItem_SelectAll_Click;
            // 
            // toolStripMenuItem_Paste
            // 
            toolStripMenuItem_Paste.Name = "toolStripMenuItem_Paste";
            toolStripMenuItem_Paste.ShortcutKeys = Keys.Control | Keys.V;
            toolStripMenuItem_Paste.Size = new Size(183, 30);
            toolStripMenuItem_Paste.Text = "粘贴";
            toolStripMenuItem_Paste.Click += toolStripMenuItem_Paste_Click;
            // 
            // toolStripMenuItem_Remove
            // 
            toolStripMenuItem_Remove.Name = "toolStripMenuItem_Remove";
            toolStripMenuItem_Remove.ShortcutKeys = Keys.Delete;
            toolStripMenuItem_Remove.Size = new Size(183, 30);
            toolStripMenuItem_Remove.Text = "移除";
            toolStripMenuItem_Remove.Click += toolStripMenuItem_Remove_Click;
            // 
            // openFileDialog_Skel
            // 
            openFileDialog_Skel.AddExtension = false;
            openFileDialog_Skel.AddToRecent = false;
            openFileDialog_Skel.Filter = "skel 文件 (*.skel; *.json)|*.skel;*.json|二进制文件 (*.skel)|*.skel|文本文件 (*.json)|*.json|所有文件 (*.*)|*.*";
            openFileDialog_Skel.Multiselect = true;
            openFileDialog_Skel.Title = "批量选择skel文件";
            // 
            // SkelFileListBox
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "SkelFileListBox";
            Size = new Size(801, 394);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            contextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private ListBox listBox;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button button_AddFolder;
        private Button button_AddFile;
        private FolderBrowserDialog folderBrowserDialog;
        private Label label_Tip;
        private ContextMenuStrip contextMenuStrip;
        private OpenFileDialog openFileDialog_Skel;
        private ToolStripMenuItem toolStripMenuItem_SelectAll;
        private ToolStripMenuItem toolStripMenuItem_Paste;
        private ToolStripMenuItem toolStripMenuItem_Remove;
    }
}
