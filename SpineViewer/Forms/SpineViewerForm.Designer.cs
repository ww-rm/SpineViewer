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
            splitContainer_MainForm = new SplitContainer();
            splitContainer_Functional = new SplitContainer();
            splitContainer_Information = new SplitContainer();
            groupBox_SkelList = new GroupBox();
            spineListView = new SpineViewer.Controls.SpineListView();
            spineViewPropertyGrid = new SpineViewer.Controls.SpineViewPropertyGrid();
            splitContainer_Config = new SplitContainer();
            groupBox_PreviewConfig = new GroupBox();
            propertyGrid_Previewer = new PropertyGrid();
            groupBox_SkelConfig = new GroupBox();
            groupBox_Preview = new GroupBox();
            spinePreviewPanel = new SpineViewer.Controls.SpinePreviewPanel();
            rtbLog = new RichTextBox();
            menuStrip = new MenuStrip();
            toolStripMenuItem_File = new ToolStripMenuItem();
            toolStripMenuItem_Open = new ToolStripMenuItem();
            toolStripMenuItem_BatchOpen = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItem_Export = new ToolStripMenuItem();
            toolStripMenuItem_ExportFrame = new ToolStripMenuItem();
            toolStripMenuItem_ExportFrameSequence = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripMenuItem_ExportGif = new ToolStripMenuItem();
            toolStripMenuItem_ExportWebp = new ToolStripMenuItem();
            toolStripMenuItem_ExportAvif = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            toolStripMenuItem_ExportMp4 = new ToolStripMenuItem();
            toolStripMenuItem_ExportWebm = new ToolStripMenuItem();
            toolStripMenuItem_ExportMkv = new ToolStripMenuItem();
            toolStripMenuItem_ExportMov = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
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
            toolStripMenuItem_Debug = new ToolStripMenuItem();
            toolStripMenuItem_Experiment = new ToolStripMenuItem();
            toolStripMenuItem_DesktopProjection = new ToolStripMenuItem();
            ToolStripMenuItem_Language = new ToolStripMenuItem();
            ToolStripMenuItem_English = new ToolStripMenuItem();
            ToolStripMenuItem_Chinese = new ToolStripMenuItem();
            panel_MainForm = new Panel();
            toolTip = new ToolTip(components);
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
            ((System.ComponentModel.ISupportInitialize)splitContainer_Config).BeginInit();
            splitContainer_Config.Panel1.SuspendLayout();
            splitContainer_Config.Panel2.SuspendLayout();
            splitContainer_Config.SuspendLayout();
            groupBox_PreviewConfig.SuspendLayout();
            groupBox_SkelConfig.SuspendLayout();
            groupBox_Preview.SuspendLayout();
            menuStrip.SuspendLayout();
            panel_MainForm.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer_MainForm
            // 
            resources.ApplyResources(splitContainer_MainForm, "splitContainer_MainForm");
            splitContainer_MainForm.Cursor = Cursors.SizeNS;
            splitContainer_MainForm.FixedPanel = FixedPanel.Panel2;
            splitContainer_MainForm.Name = "splitContainer_MainForm";
            // 
            // splitContainer_MainForm.Panel1
            // 
            resources.ApplyResources(splitContainer_MainForm.Panel1, "splitContainer_MainForm.Panel1");
            splitContainer_MainForm.Panel1.Controls.Add(splitContainer_Functional);
            splitContainer_MainForm.Panel1.Cursor = Cursors.Default;
            toolTip.SetToolTip(splitContainer_MainForm.Panel1, resources.GetString("splitContainer_MainForm.Panel1.ToolTip"));
            // 
            // splitContainer_MainForm.Panel2
            // 
            resources.ApplyResources(splitContainer_MainForm.Panel2, "splitContainer_MainForm.Panel2");
            splitContainer_MainForm.Panel2.Controls.Add(rtbLog);
            splitContainer_MainForm.Panel2.Cursor = Cursors.Default;
            toolTip.SetToolTip(splitContainer_MainForm.Panel2, resources.GetString("splitContainer_MainForm.Panel2.ToolTip"));
            splitContainer_MainForm.TabStop = false;
            toolTip.SetToolTip(splitContainer_MainForm, resources.GetString("splitContainer_MainForm.ToolTip"));
            splitContainer_MainForm.SplitterMoved += splitContainer_SplitterMoved;
            splitContainer_MainForm.MouseUp += splitContainer_MouseUp;
            // 
            // splitContainer_Functional
            // 
            resources.ApplyResources(splitContainer_Functional, "splitContainer_Functional");
            splitContainer_Functional.Cursor = Cursors.SizeWE;
            splitContainer_Functional.FixedPanel = FixedPanel.Panel1;
            splitContainer_Functional.Name = "splitContainer_Functional";
            // 
            // splitContainer_Functional.Panel1
            // 
            resources.ApplyResources(splitContainer_Functional.Panel1, "splitContainer_Functional.Panel1");
            splitContainer_Functional.Panel1.Controls.Add(splitContainer_Information);
            splitContainer_Functional.Panel1.Cursor = Cursors.Default;
            toolTip.SetToolTip(splitContainer_Functional.Panel1, resources.GetString("splitContainer_Functional.Panel1.ToolTip"));
            // 
            // splitContainer_Functional.Panel2
            // 
            resources.ApplyResources(splitContainer_Functional.Panel2, "splitContainer_Functional.Panel2");
            splitContainer_Functional.Panel2.Controls.Add(groupBox_Preview);
            splitContainer_Functional.Panel2.Cursor = Cursors.Default;
            toolTip.SetToolTip(splitContainer_Functional.Panel2, resources.GetString("splitContainer_Functional.Panel2.ToolTip"));
            splitContainer_Functional.TabStop = false;
            toolTip.SetToolTip(splitContainer_Functional, resources.GetString("splitContainer_Functional.ToolTip"));
            splitContainer_Functional.SplitterMoved += splitContainer_SplitterMoved;
            splitContainer_Functional.MouseUp += splitContainer_MouseUp;
            // 
            // splitContainer_Information
            // 
            resources.ApplyResources(splitContainer_Information, "splitContainer_Information");
            splitContainer_Information.Cursor = Cursors.SizeWE;
            splitContainer_Information.Name = "splitContainer_Information";
            // 
            // splitContainer_Information.Panel1
            // 
            resources.ApplyResources(splitContainer_Information.Panel1, "splitContainer_Information.Panel1");
            splitContainer_Information.Panel1.Controls.Add(groupBox_SkelList);
            splitContainer_Information.Panel1.Cursor = Cursors.Default;
            toolTip.SetToolTip(splitContainer_Information.Panel1, resources.GetString("splitContainer_Information.Panel1.ToolTip"));
            // 
            // splitContainer_Information.Panel2
            // 
            resources.ApplyResources(splitContainer_Information.Panel2, "splitContainer_Information.Panel2");
            splitContainer_Information.Panel2.Controls.Add(splitContainer_Config);
            splitContainer_Information.Panel2.Cursor = Cursors.Default;
            toolTip.SetToolTip(splitContainer_Information.Panel2, resources.GetString("splitContainer_Information.Panel2.ToolTip"));
            splitContainer_Information.TabStop = false;
            toolTip.SetToolTip(splitContainer_Information, resources.GetString("splitContainer_Information.ToolTip"));
            splitContainer_Information.SplitterMoved += splitContainer_SplitterMoved;
            splitContainer_Information.MouseUp += splitContainer_MouseUp;
            // 
            // groupBox_SkelList
            // 
            resources.ApplyResources(groupBox_SkelList, "groupBox_SkelList");
            groupBox_SkelList.Controls.Add(spineListView);
            groupBox_SkelList.Name = "groupBox_SkelList";
            groupBox_SkelList.TabStop = false;
            toolTip.SetToolTip(groupBox_SkelList, resources.GetString("groupBox_SkelList.ToolTip"));
            // 
            // spineListView
            // 
            resources.ApplyResources(spineListView, "spineListView");
            spineListView.Name = "spineListView";
            spineListView.SpinePropertyGrid = spineViewPropertyGrid;
            toolTip.SetToolTip(spineListView, resources.GetString("spineListView.ToolTip"));
            // 
            // spineViewPropertyGrid
            // 
            resources.ApplyResources(spineViewPropertyGrid, "spineViewPropertyGrid");
            spineViewPropertyGrid.Name = "spineViewPropertyGrid";
            toolTip.SetToolTip(spineViewPropertyGrid, resources.GetString("spineViewPropertyGrid.ToolTip"));
            // 
            // splitContainer_Config
            // 
            resources.ApplyResources(splitContainer_Config, "splitContainer_Config");
            splitContainer_Config.Name = "splitContainer_Config";
            // 
            // splitContainer_Config.Panel1
            // 
            resources.ApplyResources(splitContainer_Config.Panel1, "splitContainer_Config.Panel1");
            splitContainer_Config.Panel1.Controls.Add(groupBox_PreviewConfig);
            toolTip.SetToolTip(splitContainer_Config.Panel1, resources.GetString("splitContainer_Config.Panel1.ToolTip"));
            // 
            // splitContainer_Config.Panel2
            // 
            resources.ApplyResources(splitContainer_Config.Panel2, "splitContainer_Config.Panel2");
            splitContainer_Config.Panel2.Controls.Add(groupBox_SkelConfig);
            toolTip.SetToolTip(splitContainer_Config.Panel2, resources.GetString("splitContainer_Config.Panel2.ToolTip"));
            toolTip.SetToolTip(splitContainer_Config, resources.GetString("splitContainer_Config.ToolTip"));
            // 
            // groupBox_PreviewConfig
            // 
            resources.ApplyResources(groupBox_PreviewConfig, "groupBox_PreviewConfig");
            groupBox_PreviewConfig.Controls.Add(propertyGrid_Previewer);
            groupBox_PreviewConfig.Name = "groupBox_PreviewConfig";
            groupBox_PreviewConfig.TabStop = false;
            toolTip.SetToolTip(groupBox_PreviewConfig, resources.GetString("groupBox_PreviewConfig.ToolTip"));
            // 
            // propertyGrid_Previewer
            // 
            resources.ApplyResources(propertyGrid_Previewer, "propertyGrid_Previewer");
            propertyGrid_Previewer.Name = "propertyGrid_Previewer";
            propertyGrid_Previewer.ToolbarVisible = false;
            toolTip.SetToolTip(propertyGrid_Previewer, resources.GetString("propertyGrid_Previewer.ToolTip"));
            // 
            // groupBox_SkelConfig
            // 
            resources.ApplyResources(groupBox_SkelConfig, "groupBox_SkelConfig");
            groupBox_SkelConfig.Controls.Add(spineViewPropertyGrid);
            groupBox_SkelConfig.Name = "groupBox_SkelConfig";
            groupBox_SkelConfig.TabStop = false;
            toolTip.SetToolTip(groupBox_SkelConfig, resources.GetString("groupBox_SkelConfig.ToolTip"));
            // 
            // groupBox_Preview
            // 
            resources.ApplyResources(groupBox_Preview, "groupBox_Preview");
            groupBox_Preview.Controls.Add(spinePreviewPanel);
            groupBox_Preview.Name = "groupBox_Preview";
            groupBox_Preview.TabStop = false;
            toolTip.SetToolTip(groupBox_Preview, resources.GetString("groupBox_Preview.ToolTip"));
            // 
            // spinePreviewPanel
            // 
            resources.ApplyResources(spinePreviewPanel, "spinePreviewPanel");
            spinePreviewPanel.Name = "spinePreviewPanel";
            spinePreviewPanel.PropertyGrid = propertyGrid_Previewer;
            spinePreviewPanel.SpineListView = spineListView;
            toolTip.SetToolTip(spinePreviewPanel, resources.GetString("spinePreviewPanel.ToolTip"));
            // 
            // rtbLog
            // 
            resources.ApplyResources(rtbLog, "rtbLog");
            rtbLog.BackColor = SystemColors.Window;
            rtbLog.BorderStyle = BorderStyle.None;
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            toolTip.SetToolTip(rtbLog, resources.GetString("rtbLog.ToolTip"));
            // 
            // menuStrip
            // 
            resources.ApplyResources(menuStrip, "menuStrip");
            menuStrip.BackColor = SystemColors.Control;
            menuStrip.ImageScalingSize = new Size(24, 24);
            menuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_File, toolStripMenuItem_Tool, toolStripMenuItem_Download, toolStripMenuItem_Help, toolStripMenuItem_Experiment, ToolStripMenuItem_Language });
            menuStrip.Name = "menuStrip";
            toolTip.SetToolTip(menuStrip, resources.GetString("menuStrip.ToolTip"));
            // 
            // toolStripMenuItem_File
            // 
            resources.ApplyResources(toolStripMenuItem_File, "toolStripMenuItem_File");
            toolStripMenuItem_File.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_Open, toolStripMenuItem_BatchOpen, toolStripSeparator1, toolStripMenuItem_Export, toolStripSeparator2, toolStripMenuItem_Exit });
            toolStripMenuItem_File.Name = "toolStripMenuItem_File";
            // 
            // toolStripMenuItem_Open
            // 
            resources.ApplyResources(toolStripMenuItem_Open, "toolStripMenuItem_Open");
            toolStripMenuItem_Open.Name = "toolStripMenuItem_Open";
            toolStripMenuItem_Open.Click += toolStripMenuItem_Open_Click;
            // 
            // toolStripMenuItem_BatchOpen
            // 
            resources.ApplyResources(toolStripMenuItem_BatchOpen, "toolStripMenuItem_BatchOpen");
            toolStripMenuItem_BatchOpen.Name = "toolStripMenuItem_BatchOpen";
            toolStripMenuItem_BatchOpen.Click += toolStripMenuItem_BatchOpen_Click;
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // toolStripMenuItem_Export
            // 
            resources.ApplyResources(toolStripMenuItem_Export, "toolStripMenuItem_Export");
            toolStripMenuItem_Export.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_ExportFrame, toolStripMenuItem_ExportFrameSequence, toolStripSeparator4, toolStripMenuItem_ExportGif, toolStripMenuItem_ExportWebp, toolStripMenuItem_ExportAvif, toolStripSeparator5, toolStripMenuItem_ExportMp4, toolStripMenuItem_ExportWebm, toolStripMenuItem_ExportMkv, toolStripMenuItem_ExportMov, toolStripSeparator6, toolStripMenuItem_ExportCustom });
            toolStripMenuItem_Export.Name = "toolStripMenuItem_Export";
            // 
            // toolStripMenuItem_ExportFrame
            // 
            resources.ApplyResources(toolStripMenuItem_ExportFrame, "toolStripMenuItem_ExportFrame");
            toolStripMenuItem_ExportFrame.Name = "toolStripMenuItem_ExportFrame";
            toolStripMenuItem_ExportFrame.Click += toolStripMenuItem_ExportFrame_Click;
            // 
            // toolStripMenuItem_ExportFrameSequence
            // 
            resources.ApplyResources(toolStripMenuItem_ExportFrameSequence, "toolStripMenuItem_ExportFrameSequence");
            toolStripMenuItem_ExportFrameSequence.Name = "toolStripMenuItem_ExportFrameSequence";
            toolStripMenuItem_ExportFrameSequence.Click += toolStripMenuItem_ExportFrameSequence_Click;
            // 
            // toolStripSeparator4
            // 
            resources.ApplyResources(toolStripSeparator4, "toolStripSeparator4");
            toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // toolStripMenuItem_ExportGif
            // 
            resources.ApplyResources(toolStripMenuItem_ExportGif, "toolStripMenuItem_ExportGif");
            toolStripMenuItem_ExportGif.Name = "toolStripMenuItem_ExportGif";
            toolStripMenuItem_ExportGif.Click += toolStripMenuItem_ExportGif_Click;
            // 
            // toolStripMenuItem_ExportWebp
            // 
            resources.ApplyResources(toolStripMenuItem_ExportWebp, "toolStripMenuItem_ExportWebp");
            toolStripMenuItem_ExportWebp.Name = "toolStripMenuItem_ExportWebp";
            toolStripMenuItem_ExportWebp.Click += toolStripMenuItem_ExportWebp_Click;
            // 
            // toolStripMenuItem_ExportAvif
            // 
            resources.ApplyResources(toolStripMenuItem_ExportAvif, "toolStripMenuItem_ExportAvif");
            toolStripMenuItem_ExportAvif.Name = "toolStripMenuItem_ExportAvif";
            toolStripMenuItem_ExportAvif.Click += toolStripMenuItem_ExportAvif_Click;
            // 
            // toolStripSeparator5
            // 
            resources.ApplyResources(toolStripSeparator5, "toolStripSeparator5");
            toolStripSeparator5.Name = "toolStripSeparator5";
            // 
            // toolStripMenuItem_ExportMp4
            // 
            resources.ApplyResources(toolStripMenuItem_ExportMp4, "toolStripMenuItem_ExportMp4");
            toolStripMenuItem_ExportMp4.Name = "toolStripMenuItem_ExportMp4";
            toolStripMenuItem_ExportMp4.Click += toolStripMenuItem_ExportMp4_Click;
            // 
            // toolStripMenuItem_ExportWebm
            // 
            resources.ApplyResources(toolStripMenuItem_ExportWebm, "toolStripMenuItem_ExportWebm");
            toolStripMenuItem_ExportWebm.Name = "toolStripMenuItem_ExportWebm";
            toolStripMenuItem_ExportWebm.Click += toolStripMenuItem_ExportWebm_Click;
            // 
            // toolStripMenuItem_ExportMkv
            // 
            resources.ApplyResources(toolStripMenuItem_ExportMkv, "toolStripMenuItem_ExportMkv");
            toolStripMenuItem_ExportMkv.Name = "toolStripMenuItem_ExportMkv";
            toolStripMenuItem_ExportMkv.Click += toolStripMenuItem_ExportMkv_Click;
            // 
            // toolStripMenuItem_ExportMov
            // 
            resources.ApplyResources(toolStripMenuItem_ExportMov, "toolStripMenuItem_ExportMov");
            toolStripMenuItem_ExportMov.Name = "toolStripMenuItem_ExportMov";
            toolStripMenuItem_ExportMov.Click += toolStripMenuItem_ExportMov_Click;
            // 
            // toolStripSeparator6
            // 
            resources.ApplyResources(toolStripSeparator6, "toolStripSeparator6");
            toolStripSeparator6.Name = "toolStripSeparator6";
            // 
            // toolStripMenuItem_ExportCustom
            // 
            resources.ApplyResources(toolStripMenuItem_ExportCustom, "toolStripMenuItem_ExportCustom");
            toolStripMenuItem_ExportCustom.Name = "toolStripMenuItem_ExportCustom";
            toolStripMenuItem_ExportCustom.Click += toolStripMenuItem_ExportCustom_Click;
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // toolStripMenuItem_Exit
            // 
            resources.ApplyResources(toolStripMenuItem_Exit, "toolStripMenuItem_Exit");
            toolStripMenuItem_Exit.Name = "toolStripMenuItem_Exit";
            toolStripMenuItem_Exit.Click += toolStripMenuItem_Exit_Click;
            // 
            // toolStripMenuItem_Tool
            // 
            resources.ApplyResources(toolStripMenuItem_Tool, "toolStripMenuItem_Tool");
            toolStripMenuItem_Tool.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_ConvertFileFormat });
            toolStripMenuItem_Tool.Name = "toolStripMenuItem_Tool";
            // 
            // toolStripMenuItem_ConvertFileFormat
            // 
            resources.ApplyResources(toolStripMenuItem_ConvertFileFormat, "toolStripMenuItem_ConvertFileFormat");
            toolStripMenuItem_ConvertFileFormat.Name = "toolStripMenuItem_ConvertFileFormat";
            toolStripMenuItem_ConvertFileFormat.Click += toolStripMenuItem_ConvertFileFormat_Click;
            // 
            // toolStripMenuItem_Download
            // 
            resources.ApplyResources(toolStripMenuItem_Download, "toolStripMenuItem_Download");
            toolStripMenuItem_Download.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_ManageResource });
            toolStripMenuItem_Download.Name = "toolStripMenuItem_Download";
            // 
            // toolStripMenuItem_ManageResource
            // 
            resources.ApplyResources(toolStripMenuItem_ManageResource, "toolStripMenuItem_ManageResource");
            toolStripMenuItem_ManageResource.Name = "toolStripMenuItem_ManageResource";
            toolStripMenuItem_ManageResource.Click += toolStripMenuItem_ManageResource_Click;
            // 
            // toolStripMenuItem_Help
            // 
            resources.ApplyResources(toolStripMenuItem_Help, "toolStripMenuItem_Help");
            toolStripMenuItem_Help.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_Diagnostics, toolStripSeparator3, toolStripMenuItem_About, toolStripMenuItem_Debug });
            toolStripMenuItem_Help.Name = "toolStripMenuItem_Help";
            // 
            // toolStripMenuItem_Diagnostics
            // 
            resources.ApplyResources(toolStripMenuItem_Diagnostics, "toolStripMenuItem_Diagnostics");
            toolStripMenuItem_Diagnostics.Name = "toolStripMenuItem_Diagnostics";
            toolStripMenuItem_Diagnostics.Click += toolStripMenuItem_Diagnostics_Click;
            // 
            // toolStripSeparator3
            // 
            resources.ApplyResources(toolStripSeparator3, "toolStripSeparator3");
            toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // toolStripMenuItem_About
            // 
            resources.ApplyResources(toolStripMenuItem_About, "toolStripMenuItem_About");
            toolStripMenuItem_About.Name = "toolStripMenuItem_About";
            toolStripMenuItem_About.Click += toolStripMenuItem_About_Click;
            // 
            // toolStripMenuItem_Debug
            // 
            resources.ApplyResources(toolStripMenuItem_Debug, "toolStripMenuItem_Debug");
            toolStripMenuItem_Debug.Name = "toolStripMenuItem_Debug";
            toolStripMenuItem_Debug.Click += toolStripMenuItem_Debug_Click;
            // 
            // toolStripMenuItem_Experiment
            // 
            resources.ApplyResources(toolStripMenuItem_Experiment, "toolStripMenuItem_Experiment");
            toolStripMenuItem_Experiment.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem_DesktopProjection });
            toolStripMenuItem_Experiment.Name = "toolStripMenuItem_Experiment";
            // 
            // toolStripMenuItem_DesktopProjection
            // 
            resources.ApplyResources(toolStripMenuItem_DesktopProjection, "toolStripMenuItem_DesktopProjection");
            toolStripMenuItem_DesktopProjection.Name = "toolStripMenuItem_DesktopProjection";
            toolStripMenuItem_DesktopProjection.Click += toolStripMenuItem_DesktopProjection_Click;
            // 
            // ToolStripMenuItem_Language
            // 
            resources.ApplyResources(ToolStripMenuItem_Language, "ToolStripMenuItem_Language");
            ToolStripMenuItem_Language.DropDownItems.AddRange(new ToolStripItem[] { ToolStripMenuItem_English, ToolStripMenuItem_Chinese });
            ToolStripMenuItem_Language.Name = "ToolStripMenuItem_Language";
            // 
            // ToolStripMenuItem_English
            // 
            resources.ApplyResources(ToolStripMenuItem_English, "ToolStripMenuItem_English");
            ToolStripMenuItem_English.Name = "ToolStripMenuItem_English";
            ToolStripMenuItem_English.Click += ToolStripMenuItem_English_Click;
            // 
            // ToolStripMenuItem_Chinese
            // 
            resources.ApplyResources(ToolStripMenuItem_Chinese, "ToolStripMenuItem_Chinese");
            ToolStripMenuItem_Chinese.Name = "ToolStripMenuItem_Chinese";
            ToolStripMenuItem_Chinese.Click += ToolStripMenuItem_Chinese_Click;
            // 
            // panel_MainForm
            // 
            resources.ApplyResources(panel_MainForm, "panel_MainForm");
            panel_MainForm.Controls.Add(splitContainer_MainForm);
            panel_MainForm.Name = "panel_MainForm";
            toolTip.SetToolTip(panel_MainForm, resources.GetString("panel_MainForm.ToolTip"));
            // 
            // toolTip
            // 
            toolTip.ShowAlways = true;
            // 
            // SpineViewerForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(panel_MainForm);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "SpineViewerForm";
            toolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
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
            splitContainer_Config.Panel1.ResumeLayout(false);
            splitContainer_Config.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer_Config).EndInit();
            splitContainer_Config.ResumeLayout(false);
            groupBox_PreviewConfig.ResumeLayout(false);
            groupBox_SkelConfig.ResumeLayout(false);
            groupBox_Preview.ResumeLayout(false);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
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
        private Controls.SpinePreviewPanel spinePreviewPanel;
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
        private Controls.SpineViewPropertyGrid spineViewPropertyGrid;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem toolStripMenuItem_ExportWebp;
        private ToolStripMenuItem toolStripMenuItem_ExportAvif;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator6;
        private SplitContainer splitContainer_Config;
        private ToolStripMenuItem toolStripMenuItem_Experiment;
        private ToolStripMenuItem toolStripMenuItem_DesktopProjection;
        private ToolStripMenuItem toolStripMenuItem_Debug;
		private ToolStripMenuItem ToolStripMenuItem_Language;
		private ToolStripMenuItem ToolStripMenuItem_English;
		private ToolStripMenuItem ToolStripMenuItem_Chinese;
	}
}
