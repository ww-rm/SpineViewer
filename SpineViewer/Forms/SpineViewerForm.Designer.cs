namespace SpineViewer
{
    partial class SpineViewerForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpineViewerForm));
            menuStrip = new MenuStrip();
            toolStripMenuItem_File = new ToolStripMenuItem();
            toolStripMenuItem_Open = new ToolStripMenuItem();
            toolStripMenuItem_BatchOpen = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItem_Export = new ToolStripMenuItem();
            toolStripMenuItem_ExportFrame = new ToolStripMenuItem();
            toolStripMenuItem_ExportFrameSequence = new ToolStripMenuItem();
            toolStripMenuItem_ExportGif = new ToolStripMenuItem();
            toolStripMenuItem_ExportMp4 = new ToolStripMenuItem();
            toolStripMenuItem_ExportWebm = new ToolStripMenuItem();
            toolStripMenuItem_ExportMkv = new ToolStripMenuItem();
            toolStripMenuItem_ExportMov = new ToolStripMenuItem();
            toolStripMenuItem_ExportCustom = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripMenuItem_Exit = new ToolStripMenuItem();
            toolStripMenuItem_Tool = new ToolStripMenuItem();
            toolStripMenuItem_ConvertFileFormat = new ToolStripMenuItem();
            toolStripMenuItem_Download = new ToolStripMenuItem();
            toolStripMenuItem_ManageResource = new ToolStripMenuItem();
            toolStripMenuItem_Help = new ToolStripMenuItem();
            toolStripMenuItem_Diagnostics = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripMenuItem_About = new ToolStripMenuItem();
            rtbLog = new RichTextBox();
            splitContainer_MainForm = new SplitContainer();
            splitContainer_Functional = new SplitContainer();
            splitContainer_Information = new SplitContainer();
            groupBox_SkelList = new GroupBox();
            spineListView = new SpineViewer.Controls.SpineListView();
            spinePropertyGrid = new SpineViewer.Controls.SpinePropertyGrid();
            tabControl_Config = new TabControl();
            tabPage_Previewer = new TabPage();
            groupBox_PreviewConfig = new GroupBox();
            propertyGrid_Previewer = new PropertyGrid();
            tabPage_SpineProperty = new TabPage();
            groupBox_SkelConfig = new GroupBox();
            groupBox_Preview = new GroupBox();
            spinePreviewer = new SpineViewer.Controls.SpinePreviewer();
            panel_MainForm = new Panel();
            toolTip = new ToolTip(components);
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripSeparator5 = new ToolStripSeparator();
            toolStripSeparator6 = new ToolStripSeparator();
            toolStripMenuItem_ExportWebp = new ToolStripMenuItem();
            toolStripMenuItem_ExportAvif = new ToolStripMenuItem();
            menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer_MainForm).BeginInit();
            splitContainer_MainForm.Panel1.SuspendLayout();
            splitContainer_MainForm.Panel2.SuspendLayout();
            splitContainer_MainForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer_Functional).BeginInit();
            splitContainer_Functional.Panel1.SuspendLayout();
            splitContainer_Functional.Panel2.SuspendLayout();
            splitContainer_Functional.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer_Information).BeginInit();
            splitContainer_Information.Panel1.SuspendLayout();
            splitContainer_Information.Panel2.SuspendLayout();
            splitContainer_Information.SuspendLayout();
            groupBox_SkelList.SuspendLayout();
            tabControl_Config.SuspendLayout();
            tabPage_Previewer.SuspendLayout();
            groupBox_PreviewConfig.SuspendLayout();
            tabPage_SpineProperty.SuspendLayout();
            groupBox_SkelConfig.SuspendLayout();
            groupBox_Preview.SuspendLayout();
            panel_MainForm.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.BackColor = SystemColors.Control;
            menuStrip.ImageScalingSize = new Size(24, 24);
            menuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_File, toolStripMenuItem_Tool, toolStripMenuItem_Download, toolStripMenuItem_Help });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1778, 32);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "菜单";
            // 
            // toolStripMenuItem_File
            // 
            toolStripMenuItem_File.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_Open, toolStripMenuItem_BatchOpen, toolStripSeparator1, toolStripMenuItem_Export, toolStripSeparator2, toolStripMenuItem_Exit });
            toolStripMenuItem_File.Name = "toolStripMenuItem_File";
            toolStripMenuItem_File.Size = new Size(84, 28);
            toolStripMenuItem_File.Text = "文件(&F)";
            // 
            // toolStripMenuItem_Open
            // 
            toolStripMenuItem_Open.Name = "toolStripMenuItem_Open";
            toolStripMenuItem_Open.ShortcutKeys = Keys.Control | Keys.O;
            toolStripMenuItem_Open.Size = new Size(270, 34);
            toolStripMenuItem_Open.Text = "打开(&O)...";
            toolStripMenuItem_Open.Click += toolStripMenuItem_Open_Click;
            // 
            // toolStripMenuItem_BatchOpen
            // 
            toolStripMenuItem_BatchOpen.Name = "toolStripMenuItem_BatchOpen";
            toolStripMenuItem_BatchOpen.Size = new Size(270, 34);
            toolStripMenuItem_BatchOpen.Text = "批量打开(&B)...";
            toolStripMenuItem_BatchOpen.Click += toolStripMenuItem_BatchOpen_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(267, 6);
            // 
            // toolStripMenuItem_Export
            // 
            toolStripMenuItem_Export.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_ExportFrame, toolStripMenuItem_ExportFrameSequence, toolStripSeparator4, toolStripMenuItem_ExportGif, toolStripMenuItem_ExportWebp, toolStripMenuItem_ExportAvif, toolStripSeparator5, toolStripMenuItem_ExportMp4, toolStripMenuItem_ExportWebm, toolStripMenuItem_ExportMkv, toolStripMenuItem_ExportMov, toolStripSeparator6, toolStripMenuItem_ExportCustom });
            toolStripMenuItem_Export.Name = "toolStripMenuItem_Export";
            toolStripMenuItem_Export.Size = new Size(270, 34);
            toolStripMenuItem_Export.Text = "导出(&E)";
            // 
            // toolStripMenuItem_ExportFrame
            // 
            toolStripMenuItem_ExportFrame.Name = "toolStripMenuItem_ExportFrame";
            toolStripMenuItem_ExportFrame.Size = new Size(288, 34);
            toolStripMenuItem_ExportFrame.Text = "单帧画面...";
            toolStripMenuItem_ExportFrame.Click += toolStripMenuItem_ExportFrame_Click;
            // 
            // toolStripMenuItem_ExportFrameSequence
            // 
            toolStripMenuItem_ExportFrameSequence.Name = "toolStripMenuItem_ExportFrameSequence";
            toolStripMenuItem_ExportFrameSequence.Size = new Size(288, 34);
            toolStripMenuItem_ExportFrameSequence.Text = "帧序列...";
            toolStripMenuItem_ExportFrameSequence.Click += toolStripMenuItem_ExportFrameSequence_Click;
            // 
            // toolStripMenuItem_ExportGif
            // 
            toolStripMenuItem_ExportGif.Name = "toolStripMenuItem_ExportGif";
            toolStripMenuItem_ExportGif.Size = new Size(288, 34);
            toolStripMenuItem_ExportGif.Text = "GIF...";
            toolStripMenuItem_ExportGif.Click += toolStripMenuItem_ExportGif_Click;
            // 
            // toolStripMenuItem_ExportMp4
            // 
            toolStripMenuItem_ExportMp4.Name = "toolStripMenuItem_ExportMp4";
            toolStripMenuItem_ExportMp4.Size = new Size(288, 34);
            toolStripMenuItem_ExportMp4.Text = "MP4...";
            toolStripMenuItem_ExportMp4.Click += toolStripMenuItem_ExportMp4_Click;
            // 
            // toolStripMenuItem_ExportWebm
            // 
            toolStripMenuItem_ExportWebm.Name = "toolStripMenuItem_ExportWebm";
            toolStripMenuItem_ExportWebm.Size = new Size(288, 34);
            toolStripMenuItem_ExportWebm.Text = "WebM...";
            toolStripMenuItem_ExportWebm.Click += toolStripMenuItem_ExportWebm_Click;
            // 
            // toolStripMenuItem_ExportMkv
            // 
            toolStripMenuItem_ExportMkv.Name = "toolStripMenuItem_ExportMkv";
            toolStripMenuItem_ExportMkv.Size = new Size(288, 34);
            toolStripMenuItem_ExportMkv.Text = "MKV...";
            toolStripMenuItem_ExportMkv.Click += toolStripMenuItem_ExportMkv_Click;
            // 
            // toolStripMenuItem_ExportMov
            // 
            toolStripMenuItem_ExportMov.Name = "toolStripMenuItem_ExportMov";
            toolStripMenuItem_ExportMov.Size = new Size(288, 34);
            toolStripMenuItem_ExportMov.Text = "MOV...";
            toolStripMenuItem_ExportMov.Click += toolStripMenuItem_ExportMov_Click;
            // 
            // toolStripMenuItem_ExportCustom
            // 
            toolStripMenuItem_ExportCustom.Name = "toolStripMenuItem_ExportCustom";
            toolStripMenuItem_ExportCustom.Size = new Size(288, 34);
            toolStripMenuItem_ExportCustom.Text = "FFmpeg 自定义导出...";
            toolStripMenuItem_ExportCustom.Click += toolStripMenuItem_ExportCustom_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(267, 6);
            // 
            // toolStripMenuItem_Exit
            // 
            toolStripMenuItem_Exit.Name = "toolStripMenuItem_Exit";
            toolStripMenuItem_Exit.ShortcutKeys = Keys.Alt | Keys.F4;
            toolStripMenuItem_Exit.Size = new Size(270, 34);
            toolStripMenuItem_Exit.Text = "退出(&X)";
            toolStripMenuItem_Exit.Click += toolStripMenuItem_Exit_Click;
            // 
            // toolStripMenuItem_Tool
            // 
            toolStripMenuItem_Tool.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_ConvertFileFormat });
            toolStripMenuItem_Tool.Name = "toolStripMenuItem_Tool";
            toolStripMenuItem_Tool.Size = new Size(84, 28);
            toolStripMenuItem_Tool.Text = "工具(&T)";
            // 
            // toolStripMenuItem_ConvertFileFormat
            // 
            toolStripMenuItem_ConvertFileFormat.Name = "toolStripMenuItem_ConvertFileFormat";
            toolStripMenuItem_ConvertFileFormat.Size = new Size(254, 34);
            toolStripMenuItem_ConvertFileFormat.Text = "转换文件格式(&C)...";
            toolStripMenuItem_ConvertFileFormat.Click += toolStripMenuItem_ConvertFileFormat_Click;
            // 
            // toolStripMenuItem_Download
            // 
            toolStripMenuItem_Download.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_ManageResource });
            toolStripMenuItem_Download.Name = "toolStripMenuItem_Download";
            toolStripMenuItem_Download.Size = new Size(88, 28);
            toolStripMenuItem_Download.Text = "下载(&D)";
            // 
            // toolStripMenuItem_ManageResource
            // 
            toolStripMenuItem_ManageResource.Name = "toolStripMenuItem_ManageResource";
            toolStripMenuItem_ManageResource.Size = new Size(260, 34);
            toolStripMenuItem_ManageResource.Text = "管理下载资源(&M)...";
            toolStripMenuItem_ManageResource.Click += toolStripMenuItem_ManageResource_Click;
            // 
            // toolStripMenuItem_Help
            // 
            toolStripMenuItem_Help.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_Diagnostics, toolStripSeparator3, toolStripMenuItem_About });
            toolStripMenuItem_Help.Name = "toolStripMenuItem_Help";
            toolStripMenuItem_Help.Size = new Size(88, 28);
            toolStripMenuItem_Help.Text = "帮助(&H)";
            // 
            // toolStripMenuItem_Diagnostics
            // 
            toolStripMenuItem_Diagnostics.Name = "toolStripMenuItem_Diagnostics";
            toolStripMenuItem_Diagnostics.Size = new Size(208, 34);
            toolStripMenuItem_Diagnostics.Text = "诊断信息(&D)";
            toolStripMenuItem_Diagnostics.Click += toolStripMenuItem_Diagnostics_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(205, 6);
            // 
            // toolStripMenuItem_About
            // 
            toolStripMenuItem_About.Name = "toolStripMenuItem_About";
            toolStripMenuItem_About.Size = new Size(208, 34);
            toolStripMenuItem_About.Text = "关于(&A)";
            toolStripMenuItem_About.Click += toolStripMenuItem_About_Click;
            // 
            // rtbLog
            // 
            rtbLog.BackColor = SystemColors.Window;
            rtbLog.BorderStyle = BorderStyle.None;
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.Font = new Font("Consolas", 9F);
            rtbLog.Location = new Point(0, 0);
            rtbLog.Margin = new Padding(3, 2, 3, 2);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(1758, 172);
            rtbLog.TabIndex = 0;
            rtbLog.Text = "";
            rtbLog.WordWrap = false;
            // 
            // splitContainer_MainForm
            // 
            splitContainer_MainForm.Cursor = Cursors.SizeNS;
            splitContainer_MainForm.Dock = DockStyle.Fill;
            splitContainer_MainForm.FixedPanel = FixedPanel.Panel2;
            splitContainer_MainForm.Location = new Point(10, 5);
            splitContainer_MainForm.Name = "splitContainer_MainForm";
            splitContainer_MainForm.Orientation = Orientation.Horizontal;
            // 
            // splitContainer_MainForm.Panel1
            // 
            splitContainer_MainForm.Panel1.Controls.Add(splitContainer_Functional);
            splitContainer_MainForm.Panel1.Cursor = Cursors.Default;
            // 
            // splitContainer_MainForm.Panel2
            // 
            splitContainer_MainForm.Panel2.Controls.Add(rtbLog);
            splitContainer_MainForm.Panel2.Cursor = Cursors.Default;
            splitContainer_MainForm.Size = new Size(1758, 1097);
            splitContainer_MainForm.SplitterDistance = 917;
            splitContainer_MainForm.SplitterWidth = 8;
            splitContainer_MainForm.TabIndex = 3;
            splitContainer_MainForm.TabStop = false;
            splitContainer_MainForm.SplitterMoved += splitContainer_SplitterMoved;
            splitContainer_MainForm.MouseUp += splitContainer_MouseUp;
            // 
            // splitContainer_Functional
            // 
            splitContainer_Functional.Cursor = Cursors.SizeWE;
            splitContainer_Functional.Dock = DockStyle.Fill;
            splitContainer_Functional.FixedPanel = FixedPanel.Panel1;
            splitContainer_Functional.Location = new Point(0, 0);
            splitContainer_Functional.Name = "splitContainer_Functional";
            // 
            // splitContainer_Functional.Panel1
            // 
            splitContainer_Functional.Panel1.Controls.Add(splitContainer_Information);
            splitContainer_Functional.Panel1.Cursor = Cursors.Default;
            // 
            // splitContainer_Functional.Panel2
            // 
            splitContainer_Functional.Panel2.Controls.Add(groupBox_Preview);
            splitContainer_Functional.Panel2.Cursor = Cursors.Default;
            splitContainer_Functional.Size = new Size(1758, 917);
            splitContainer_Functional.SplitterDistance = 759;
            splitContainer_Functional.SplitterWidth = 8;
            splitContainer_Functional.TabIndex = 2;
            splitContainer_Functional.TabStop = false;
            splitContainer_Functional.SplitterMoved += splitContainer_SplitterMoved;
            splitContainer_Functional.MouseUp += splitContainer_MouseUp;
            // 
            // splitContainer_Information
            // 
            splitContainer_Information.Cursor = Cursors.SizeWE;
            splitContainer_Information.Dock = DockStyle.Fill;
            splitContainer_Information.Location = new Point(0, 0);
            splitContainer_Information.Name = "splitContainer_Information";
            // 
            // splitContainer_Information.Panel1
            // 
            splitContainer_Information.Panel1.Controls.Add(groupBox_SkelList);
            splitContainer_Information.Panel1.Cursor = Cursors.Default;
            // 
            // splitContainer_Information.Panel2
            // 
            splitContainer_Information.Panel2.Controls.Add(tabControl_Config);
            splitContainer_Information.Panel2.Cursor = Cursors.Default;
            splitContainer_Information.Size = new Size(759, 917);
            splitContainer_Information.SplitterDistance = 354;
            splitContainer_Information.SplitterWidth = 8;
            splitContainer_Information.TabIndex = 1;
            splitContainer_Information.TabStop = false;
            splitContainer_Information.SplitterMoved += splitContainer_SplitterMoved;
            splitContainer_Information.MouseUp += splitContainer_MouseUp;
            // 
            // groupBox_SkelList
            // 
            groupBox_SkelList.Controls.Add(spineListView);
            groupBox_SkelList.Dock = DockStyle.Fill;
            groupBox_SkelList.Location = new Point(0, 0);
            groupBox_SkelList.Name = "groupBox_SkelList";
            groupBox_SkelList.Size = new Size(354, 917);
            groupBox_SkelList.TabIndex = 0;
            groupBox_SkelList.TabStop = false;
            groupBox_SkelList.Text = "模型列表";
            // 
            // spineListView
            // 
            spineListView.Dock = DockStyle.Fill;
            spineListView.Location = new Point(3, 26);
            spineListView.Name = "spineListView";
            spineListView.Size = new Size(348, 888);
            spineListView.SpinePropertyGrid = spinePropertyGrid;
            spineListView.TabIndex = 0;
            // 
            // spinePropertyGrid
            // 
            spinePropertyGrid.Dock = DockStyle.Fill;
            spinePropertyGrid.Location = new Point(3, 26);
            spinePropertyGrid.Name = "spinePropertyGrid";
            spinePropertyGrid.Size = new Size(383, 849);
            spinePropertyGrid.TabIndex = 0;
            // 
            // tabControl_Config
            // 
            tabControl_Config.Alignment = TabAlignment.Bottom;
            tabControl_Config.Controls.Add(tabPage_Previewer);
            tabControl_Config.Controls.Add(tabPage_SpineProperty);
            tabControl_Config.Dock = DockStyle.Fill;
            tabControl_Config.ItemSize = new Size(100, 35);
            tabControl_Config.Location = new Point(0, 0);
            tabControl_Config.Multiline = true;
            tabControl_Config.Name = "tabControl_Config";
            tabControl_Config.Padding = new Point(0, 0);
            tabControl_Config.SelectedIndex = 0;
            tabControl_Config.Size = new Size(397, 917);
            tabControl_Config.TabIndex = 0;
            // 
            // tabPage_Previewer
            // 
            tabPage_Previewer.Controls.Add(groupBox_PreviewConfig);
            tabPage_Previewer.Location = new Point(4, 4);
            tabPage_Previewer.Margin = new Padding(0);
            tabPage_Previewer.Name = "tabPage_Previewer";
            tabPage_Previewer.Size = new Size(389, 874);
            tabPage_Previewer.TabIndex = 0;
            tabPage_Previewer.Text = "画面参数";
            // 
            // groupBox_PreviewConfig
            // 
            groupBox_PreviewConfig.Controls.Add(propertyGrid_Previewer);
            groupBox_PreviewConfig.Dock = DockStyle.Fill;
            groupBox_PreviewConfig.Location = new Point(0, 0);
            groupBox_PreviewConfig.Margin = new Padding(0);
            groupBox_PreviewConfig.Name = "groupBox_PreviewConfig";
            groupBox_PreviewConfig.Size = new Size(389, 874);
            groupBox_PreviewConfig.TabIndex = 1;
            groupBox_PreviewConfig.TabStop = false;
            groupBox_PreviewConfig.Text = "画面参数";
            // 
            // propertyGrid_Previewer
            // 
            propertyGrid_Previewer.Dock = DockStyle.Fill;
            propertyGrid_Previewer.HelpVisible = false;
            propertyGrid_Previewer.Location = new Point(3, 26);
            propertyGrid_Previewer.Name = "propertyGrid_Previewer";
            propertyGrid_Previewer.Size = new Size(383, 845);
            propertyGrid_Previewer.TabIndex = 1;
            propertyGrid_Previewer.ToolbarVisible = false;
            propertyGrid_Previewer.PropertyValueChanged += propertyGrid_PropertyValueChanged;
            // 
            // tabPage_SpineProperty
            // 
            tabPage_SpineProperty.BackColor = SystemColors.Control;
            tabPage_SpineProperty.Controls.Add(groupBox_SkelConfig);
            tabPage_SpineProperty.Location = new Point(4, 4);
            tabPage_SpineProperty.Margin = new Padding(0);
            tabPage_SpineProperty.Name = "tabPage_SpineProperty";
            tabPage_SpineProperty.Size = new Size(389, 878);
            tabPage_SpineProperty.TabIndex = 1;
            tabPage_SpineProperty.Text = "模型参数";
            // 
            // groupBox_SkelConfig
            // 
            groupBox_SkelConfig.Controls.Add(spinePropertyGrid);
            groupBox_SkelConfig.Dock = DockStyle.Fill;
            groupBox_SkelConfig.Location = new Point(0, 0);
            groupBox_SkelConfig.Margin = new Padding(0);
            groupBox_SkelConfig.Name = "groupBox_SkelConfig";
            groupBox_SkelConfig.Size = new Size(389, 878);
            groupBox_SkelConfig.TabIndex = 0;
            groupBox_SkelConfig.TabStop = false;
            groupBox_SkelConfig.Text = "模型参数";
            // 
            // groupBox_Preview
            // 
            groupBox_Preview.Controls.Add(spinePreviewer);
            groupBox_Preview.Dock = DockStyle.Fill;
            groupBox_Preview.Location = new Point(0, 0);
            groupBox_Preview.Name = "groupBox_Preview";
            groupBox_Preview.Size = new Size(991, 917);
            groupBox_Preview.TabIndex = 1;
            groupBox_Preview.TabStop = false;
            groupBox_Preview.Text = "预览画面";
            // 
            // spinePreviewer
            // 
            spinePreviewer.Dock = DockStyle.Fill;
            spinePreviewer.Location = new Point(3, 26);
            spinePreviewer.Name = "spinePreviewer";
            spinePreviewer.PropertyGrid = propertyGrid_Previewer;
            spinePreviewer.Size = new Size(985, 888);
            spinePreviewer.SpineListView = spineListView;
            spinePreviewer.TabIndex = 0;
            // 
            // panel_MainForm
            // 
            panel_MainForm.Controls.Add(splitContainer_MainForm);
            panel_MainForm.Dock = DockStyle.Fill;
            panel_MainForm.Location = new Point(0, 32);
            panel_MainForm.Name = "panel_MainForm";
            panel_MainForm.Padding = new Padding(10, 5, 10, 10);
            panel_MainForm.Size = new Size(1778, 1112);
            panel_MainForm.TabIndex = 4;
            // 
            // toolTip
            // 
            toolTip.ShowAlways = true;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(285, 6);
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(285, 6);
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(285, 6);
            // 
            // toolStripMenuItem_ExportWebp
            // 
            toolStripMenuItem_ExportWebp.Name = "toolStripMenuItem_ExportWebp";
            toolStripMenuItem_ExportWebp.Size = new Size(288, 34);
            toolStripMenuItem_ExportWebp.Text = "WebP...";
            toolStripMenuItem_ExportWebp.Click += toolStripMenuItem_ExportWebp_Click;
            // 
            // toolStripMenuItem_ExportAvif
            // 
            toolStripMenuItem_ExportAvif.Name = "toolStripMenuItem_ExportAvif";
            toolStripMenuItem_ExportAvif.Size = new Size(288, 34);
            toolStripMenuItem_ExportAvif.Text = "AVIF...";
            toolStripMenuItem_ExportAvif.Click += toolStripMenuItem_ExportAvif_Click;
            // 
            // SpineViewerForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1778, 1144);
            Controls.Add(panel_MainForm);
            Controls.Add(menuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            Margin = new Padding(3, 2, 3, 2);
            Name = "SpineViewerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SpineViewer";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            splitContainer_MainForm.Panel1.ResumeLayout(false);
            splitContainer_MainForm.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer_MainForm).EndInit();
            splitContainer_MainForm.ResumeLayout(false);
            splitContainer_Functional.Panel1.ResumeLayout(false);
            splitContainer_Functional.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer_Functional).EndInit();
            splitContainer_Functional.ResumeLayout(false);
            splitContainer_Information.Panel1.ResumeLayout(false);
            splitContainer_Information.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer_Information).EndInit();
            splitContainer_Information.ResumeLayout(false);
            groupBox_SkelList.ResumeLayout(false);
            tabControl_Config.ResumeLayout(false);
            tabPage_Previewer.ResumeLayout(false);
            groupBox_PreviewConfig.ResumeLayout(false);
            tabPage_SpineProperty.ResumeLayout(false);
            groupBox_SkelConfig.ResumeLayout(false);
            groupBox_Preview.ResumeLayout(false);
            panel_MainForm.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem toolStripMenuItem_File;
        private ToolStripMenuItem toolStripMenuItem_Open;
        private ToolStripMenuItem toolStripMenuItem_Exit;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private RichTextBox rtbLog;
        private SplitContainer splitContainer_MainForm;
        private SplitContainer splitContainer_Functional;
        private SplitContainer splitContainer_Information;
        private GroupBox groupBox_SkelList;
        private GroupBox groupBox_SkelConfig;
        private GroupBox groupBox_PreviewConfig;
        private Panel panel_MainForm;
        private ToolStripMenuItem toolStripMenuItem_Help;
        private ToolStripMenuItem toolStripMenuItem_About;
        private ToolStripMenuItem toolStripMenuItem_BatchOpen;
        private GroupBox groupBox_Preview;
        private ToolTip toolTip;
        private Controls.SpineListView spineListView;
        private PropertyGrid propertyGrid_Previewer;
        private Controls.SpinePreviewer spinePreviewer;
        private ToolStripMenuItem toolStripMenuItem_Diagnostics;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem toolStripMenuItem_Download;
        private ToolStripMenuItem toolStripMenuItem_ManageResource;
        private ToolStripMenuItem toolStripMenuItem_Tool;
        private ToolStripMenuItem toolStripMenuItem_ConvertFileFormat;
        private ToolStripMenuItem toolStripMenuItem_Export;
        private ToolStripMenuItem toolStripMenuItem_ExportFrame;
        private ToolStripMenuItem toolStripMenuItem_ExportFrameSequence;
        private ToolStripMenuItem toolStripMenuItem_ExportGif;
        private ToolStripMenuItem toolStripMenuItem_ExportMp4;
        private ToolStripMenuItem toolStripMenuItem_ExportMov;
        private ToolStripMenuItem toolStripMenuItem_ExportMkv;
        private ToolStripMenuItem toolStripMenuItem_ExportWebm;
        private ToolStripMenuItem toolStripMenuItem_ExportCustom;
        private Controls.SpinePropertyGrid spinePropertyGrid;
        private TabControl tabControl_Config;
        private TabPage tabPage_Previewer;
        private TabPage tabPage_SpineProperty;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem toolStripMenuItem_ExportWebp;
        private ToolStripMenuItem toolStripMenuItem_ExportAvif;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator6;
    }
}
