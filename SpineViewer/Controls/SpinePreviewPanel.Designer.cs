namespace SpineViewer.Controls
{
    partial class SpinePreviewPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpinePreviewPanel));
            panel_Render = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            button_Stop = new Button();
            imageList = new ImageList(components);
            button_Restart = new Button();
            button_Start = new Button();
            button_ForwardStep = new Button();
            button_ForwardFast = new Button();
            button_FullScreen = new Button();
            panel_ViewContainer = new Panel();
            panel_RenderContainer = new Panel();
            toolTip = new ToolTip(components);
            spinePreviewFullScreenForm = new SpineViewer.Forms.SpinePreviewFullScreenForm();
            wallpaperForm = new WallpaperForm();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            panel_ViewContainer.SuspendLayout();
            panel_RenderContainer.SuspendLayout();
            SuspendLayout();
            // 
            // panel_Render
            // 
            panel_Render.BackColor = SystemColors.ControlDarkDark;
            panel_Render.Location = new Point(157, 136);
            panel_Render.Margin = new Padding(0);
            panel_Render.Name = "panel_Render";
            panel_Render.Size = new Size(320, 320);
            panel_Render.TabIndex = 1;
            panel_Render.MouseDown += panel_Render_MouseDown;
            panel_Render.MouseMove += panel_Render_MouseMove;
            panel_Render.MouseUp += panel_Render_MouseUp;
            panel_Render.MouseWheel += panel_Render_MouseWheel;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
            tableLayoutPanel1.Controls.Add(panel_ViewContainer, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(641, 636);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.None;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(button_Stop);
            flowLayoutPanel1.Controls.Add(button_Restart);
            flowLayoutPanel1.Controls.Add(button_Start);
            flowLayoutPanel1.Controls.Add(button_ForwardStep);
            flowLayoutPanel1.Controls.Add(button_ForwardFast);
            flowLayoutPanel1.Controls.Add(button_FullScreen);
            flowLayoutPanel1.Location = new Point(101, 594);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(438, 42);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // button_Stop
            // 
            button_Stop.AutoSize = true;
            button_Stop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_Stop.ImageKey = "stop";
            button_Stop.ImageList = imageList;
            button_Stop.Location = new Point(3, 3);
            button_Stop.Name = "button_Stop";
            button_Stop.Padding = new Padding(15, 3, 15, 3);
            button_Stop.Size = new Size(67, 36);
            button_Stop.TabIndex = 0;
            toolTip.SetToolTip(button_Stop, "停止播放并重置时间到初始");
            button_Stop.UseVisualStyleBackColor = true;
            button_Stop.Click += button_Stop_Click;
            // 
            // imageList
            // 
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageStream = (ImageListStreamer)resources.GetObject("imageList.ImageStream");
            imageList.TransparentColor = Color.Transparent;
            imageList.Images.SetKeyName(0, "arrows-maximize");
            imageList.Images.SetKeyName(1, "forward-fast");
            imageList.Images.SetKeyName(2, "forward-step");
            imageList.Images.SetKeyName(3, "pause");
            imageList.Images.SetKeyName(4, "rotate-left");
            imageList.Images.SetKeyName(5, "start");
            imageList.Images.SetKeyName(6, "stop");
            // 
            // button_Restart
            // 
            button_Restart.AutoSize = true;
            button_Restart.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_Restart.ImageKey = "rotate-left";
            button_Restart.ImageList = imageList;
            button_Restart.Location = new Point(76, 3);
            button_Restart.Name = "button_Restart";
            button_Restart.Padding = new Padding(15, 3, 15, 3);
            button_Restart.Size = new Size(67, 36);
            button_Restart.TabIndex = 1;
            toolTip.SetToolTip(button_Restart, "从头开始播放");
            button_Restart.UseVisualStyleBackColor = true;
            button_Restart.Click += button_Restart_Click;
            // 
            // button_Start
            // 
            button_Start.AutoSize = true;
            button_Start.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_Start.BackgroundImageLayout = ImageLayout.Center;
            button_Start.ImageKey = "pause";
            button_Start.ImageList = imageList;
            button_Start.Location = new Point(149, 3);
            button_Start.Name = "button_Start";
            button_Start.Padding = new Padding(15, 3, 15, 3);
            button_Start.Size = new Size(67, 36);
            button_Start.TabIndex = 2;
            toolTip.SetToolTip(button_Start, "开始/暂停");
            button_Start.UseVisualStyleBackColor = true;
            button_Start.Click += button_Start_Click;
            // 
            // button_ForwardStep
            // 
            button_ForwardStep.AutoSize = true;
            button_ForwardStep.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_ForwardStep.ImageKey = "forward-step";
            button_ForwardStep.ImageList = imageList;
            button_ForwardStep.Location = new Point(222, 3);
            button_ForwardStep.Name = "button_ForwardStep";
            button_ForwardStep.Padding = new Padding(15, 3, 15, 3);
            button_ForwardStep.Size = new Size(67, 36);
            button_ForwardStep.TabIndex = 3;
            toolTip.SetToolTip(button_ForwardStep, "快进 1 帧");
            button_ForwardStep.UseVisualStyleBackColor = true;
            button_ForwardStep.Click += button_ForwardStep_Click;
            // 
            // button_ForwardFast
            // 
            button_ForwardFast.AutoSize = true;
            button_ForwardFast.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_ForwardFast.ImageKey = "forward-fast";
            button_ForwardFast.ImageList = imageList;
            button_ForwardFast.Location = new Point(295, 3);
            button_ForwardFast.Name = "button_ForwardFast";
            button_ForwardFast.Padding = new Padding(15, 3, 15, 3);
            button_ForwardFast.Size = new Size(67, 36);
            button_ForwardFast.TabIndex = 4;
            toolTip.SetToolTip(button_ForwardFast, "快进 10 帧");
            button_ForwardFast.UseVisualStyleBackColor = true;
            button_ForwardFast.Click += button_ForwardFast_Click;
            // 
            // button_FullScreen
            // 
            button_FullScreen.AutoSize = true;
            button_FullScreen.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_FullScreen.ImageKey = "arrows-maximize";
            button_FullScreen.ImageList = imageList;
            button_FullScreen.Location = new Point(368, 3);
            button_FullScreen.Name = "button_FullScreen";
            button_FullScreen.Padding = new Padding(15, 3, 15, 3);
            button_FullScreen.Size = new Size(67, 36);
            button_FullScreen.TabIndex = 5;
            toolTip.SetToolTip(button_FullScreen, "全屏预览");
            button_FullScreen.UseVisualStyleBackColor = true;
            button_FullScreen.Click += button_FullScreen_Click;
            // 
            // panel_ViewContainer
            // 
            panel_ViewContainer.Controls.Add(panel_RenderContainer);
            panel_ViewContainer.Dock = DockStyle.Fill;
            panel_ViewContainer.Location = new Point(0, 0);
            panel_ViewContainer.Margin = new Padding(0);
            panel_ViewContainer.Name = "panel_ViewContainer";
            panel_ViewContainer.Size = new Size(641, 594);
            panel_ViewContainer.TabIndex = 6;
            // 
            // panel_RenderContainer
            // 
            panel_RenderContainer.BackColor = SystemColors.ControlDark;
            panel_RenderContainer.Controls.Add(panel_Render);
            panel_RenderContainer.Dock = DockStyle.Fill;
            panel_RenderContainer.Location = new Point(0, 0);
            panel_RenderContainer.Margin = new Padding(0);
            panel_RenderContainer.Name = "panel_RenderContainer";
            panel_RenderContainer.Size = new Size(641, 594);
            panel_RenderContainer.TabIndex = 0;
            panel_RenderContainer.SizeChanged += panel_RenderContainer_SizeChanged;
            // 
            // spinePreviewFullScreenForm
            // 
            spinePreviewFullScreenForm.ClientSize = new Size(2560, 1440);
            spinePreviewFullScreenForm.ControlBox = false;
            spinePreviewFullScreenForm.FormBorderStyle = FormBorderStyle.None;
            spinePreviewFullScreenForm.MaximizeBox = false;
            spinePreviewFullScreenForm.MinimizeBox = false;
            spinePreviewFullScreenForm.Name = "SpinePreviewFullScreenForm";
            spinePreviewFullScreenForm.ShowIcon = false;
            spinePreviewFullScreenForm.ShowInTaskbar = false;
            spinePreviewFullScreenForm.StartPosition = FormStartPosition.Manual;
            spinePreviewFullScreenForm.TopMost = true;
            spinePreviewFullScreenForm.Visible = false;
            spinePreviewFullScreenForm.FormClosing += spinePreviewFullScreenForm_FormClosing;
            spinePreviewFullScreenForm.KeyDown += spinePreviewFullScreenForm_KeyDown;
            // 
            // wallpaperForm
            // 
            wallpaperForm.ClientSize = new Size(0, 0);
            wallpaperForm.ControlBox = false;
            wallpaperForm.FormBorderStyle = FormBorderStyle.None;
            wallpaperForm.MaximizeBox = false;
            wallpaperForm.MinimizeBox = false;
            wallpaperForm.Name = "WallpaperForm";
            wallpaperForm.ShowIcon = false;
            wallpaperForm.ShowInTaskbar = false;
            wallpaperForm.StartPosition = FormStartPosition.Manual;
            wallpaperForm.Visible = false;
            wallpaperForm.WindowState = FormWindowState.Minimized;
            wallpaperForm.FormClosing += wallpaperForm_FormClosing;
            // 
            // SpinePreviewPanel
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "SpinePreviewPanel";
            Size = new Size(641, 636);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            panel_ViewContainer.ResumeLayout(false);
            panel_RenderContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel_Render;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel_RenderContainer;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button button_Stop;
        private Button button_Start;
        private ImageList imageList;
        private ToolTip toolTip;
        private Button button_ForwardStep;
        private Button button_ForwardFast;
        private Button button_Restart;
        private Button button_FullScreen;
        private Panel panel_ViewContainer;
        private Forms.SpinePreviewFullScreenForm spinePreviewFullScreenForm;
        private WallpaperForm wallpaperForm;
    }
}
