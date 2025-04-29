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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkelFileListBox));
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
			resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
			tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 0);
			tableLayoutPanel1.Controls.Add(listBox, 0, 1);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// flowLayoutPanel1
			// 
			resources.ApplyResources(flowLayoutPanel1, "flowLayoutPanel1");
			flowLayoutPanel1.Controls.Add(button_AddFolder);
			flowLayoutPanel1.Controls.Add(button_AddFile);
			flowLayoutPanel1.Controls.Add(label_Tip);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			// 
			// button_AddFolder
			// 
			resources.ApplyResources(button_AddFolder, "button_AddFolder");
			button_AddFolder.Name = "button_AddFolder";
			button_AddFolder.UseVisualStyleBackColor = true;
			button_AddFolder.Click += button_AddFolder_Click;
			// 
			// button_AddFile
			// 
			resources.ApplyResources(button_AddFile, "button_AddFile");
			button_AddFile.Name = "button_AddFile";
			button_AddFile.UseVisualStyleBackColor = true;
			button_AddFile.Click += button_AddFile_Click;
			// 
			// label_Tip
			// 
			resources.ApplyResources(label_Tip, "label_Tip");
			label_Tip.Name = "label_Tip";
			// 
			// listBox
			// 
			listBox.AllowDrop = true;
			listBox.ContextMenuStrip = contextMenuStrip;
			resources.ApplyResources(listBox, "listBox");
			listBox.FormattingEnabled = true;
			listBox.Name = "listBox";
			listBox.SelectionMode = SelectionMode.MultiExtended;
			listBox.DragDrop += listBox_DragDrop;
			listBox.DragEnter += listBox_DragEnter;
			// 
			// contextMenuStrip
			// 
			contextMenuStrip.ImageScalingSize = new Size(24, 24);
			contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_SelectAll, toolStripMenuItem_Paste, toolStripMenuItem_Remove });
			contextMenuStrip.Name = "contextMenuStrip";
			resources.ApplyResources(contextMenuStrip, "contextMenuStrip");
			// 
			// toolStripMenuItem_SelectAll
			// 
			toolStripMenuItem_SelectAll.Name = "toolStripMenuItem_SelectAll";
			resources.ApplyResources(toolStripMenuItem_SelectAll, "toolStripMenuItem_SelectAll");
			toolStripMenuItem_SelectAll.Click += toolStripMenuItem_SelectAll_Click;
			// 
			// toolStripMenuItem_Paste
			// 
			toolStripMenuItem_Paste.Name = "toolStripMenuItem_Paste";
			resources.ApplyResources(toolStripMenuItem_Paste, "toolStripMenuItem_Paste");
			toolStripMenuItem_Paste.Click += toolStripMenuItem_Paste_Click;
			// 
			// toolStripMenuItem_Remove
			// 
			toolStripMenuItem_Remove.Name = "toolStripMenuItem_Remove";
			resources.ApplyResources(toolStripMenuItem_Remove, "toolStripMenuItem_Remove");
			toolStripMenuItem_Remove.Click += toolStripMenuItem_Remove_Click;
			// 
			// openFileDialog_Skel
			// 
			openFileDialog_Skel.AddExtension = false;
			openFileDialog_Skel.AddToRecent = false;
			resources.ApplyResources(openFileDialog_Skel, "openFileDialog_Skel");
			openFileDialog_Skel.Multiselect = true;
			// 
			// SkelFileListBox
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(tableLayoutPanel1);
			Name = "SkelFileListBox";
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
