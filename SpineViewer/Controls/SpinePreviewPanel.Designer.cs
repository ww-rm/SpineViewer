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
			spinePreviewFullScreenForm = new Forms.SpinePreviewFullScreenForm();
			wallpaperForm = new WallpaperForm();
			tableLayoutPanel1.SuspendLayout();
			flowLayoutPanel1.SuspendLayout();
			panel_ViewContainer.SuspendLayout();
			panel_RenderContainer.SuspendLayout();
			SuspendLayout();
			// 
			// panel_Render
			// 
			resources.ApplyResources(panel_Render, "panel_Render");
			panel_Render.BackColor = SystemColors.ControlDarkDark;
			panel_Render.Name = "panel_Render";
			toolTip.SetToolTip(panel_Render, resources.GetString("panel_Render.ToolTip"));
			panel_Render.MouseDown += panel_Render_MouseDown;
			panel_Render.MouseMove += panel_Render_MouseMove;
			panel_Render.MouseUp += panel_Render_MouseUp;
			panel_Render.MouseWheel += panel_Render_MouseWheel;
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
			tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
			tableLayoutPanel1.Controls.Add(panel_ViewContainer, 0, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			toolTip.SetToolTip(tableLayoutPanel1, resources.GetString("tableLayoutPanel1.ToolTip"));
			// 
			// flowLayoutPanel1
			// 
			resources.ApplyResources(flowLayoutPanel1, "flowLayoutPanel1");
			flowLayoutPanel1.Controls.Add(button_Stop);
			flowLayoutPanel1.Controls.Add(button_Restart);
			flowLayoutPanel1.Controls.Add(button_Start);
			flowLayoutPanel1.Controls.Add(button_ForwardStep);
			flowLayoutPanel1.Controls.Add(button_ForwardFast);
			flowLayoutPanel1.Controls.Add(button_FullScreen);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			toolTip.SetToolTip(flowLayoutPanel1, resources.GetString("flowLayoutPanel1.ToolTip"));
			// 
			// button_Stop
			// 
			resources.ApplyResources(button_Stop, "button_Stop");
			button_Stop.ImageList = imageList;
			button_Stop.Name = "button_Stop";
			toolTip.SetToolTip(button_Stop, resources.GetString("button_Stop.ToolTip"));
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
			resources.ApplyResources(button_Restart, "button_Restart");
			button_Restart.ImageList = imageList;
			button_Restart.Name = "button_Restart";
			toolTip.SetToolTip(button_Restart, resources.GetString("button_Restart.ToolTip"));
			button_Restart.UseVisualStyleBackColor = true;
			button_Restart.Click += button_Restart_Click;
			// 
			// button_Start
			// 
			resources.ApplyResources(button_Start, "button_Start");
			button_Start.ImageList = imageList;
			button_Start.Name = "button_Start";
			toolTip.SetToolTip(button_Start, resources.GetString("button_Start.ToolTip"));
			button_Start.UseVisualStyleBackColor = true;
			button_Start.Click += button_Start_Click;
			// 
			// button_ForwardStep
			// 
			resources.ApplyResources(button_ForwardStep, "button_ForwardStep");
			button_ForwardStep.ImageList = imageList;
			button_ForwardStep.Name = "button_ForwardStep";
			toolTip.SetToolTip(button_ForwardStep, resources.GetString("button_ForwardStep.ToolTip"));
			button_ForwardStep.UseVisualStyleBackColor = true;
			button_ForwardStep.Click += button_ForwardStep_Click;
			// 
			// button_ForwardFast
			// 
			resources.ApplyResources(button_ForwardFast, "button_ForwardFast");
			button_ForwardFast.ImageList = imageList;
			button_ForwardFast.Name = "button_ForwardFast";
			toolTip.SetToolTip(button_ForwardFast, resources.GetString("button_ForwardFast.ToolTip"));
			button_ForwardFast.UseVisualStyleBackColor = true;
			button_ForwardFast.Click += button_ForwardFast_Click;
			// 
			// button_FullScreen
			// 
			resources.ApplyResources(button_FullScreen, "button_FullScreen");
			button_FullScreen.ImageList = imageList;
			button_FullScreen.Name = "button_FullScreen";
			toolTip.SetToolTip(button_FullScreen, resources.GetString("button_FullScreen.ToolTip"));
			button_FullScreen.UseVisualStyleBackColor = true;
			button_FullScreen.Click += button_FullScreen_Click;
			// 
			// panel_ViewContainer
			// 
			resources.ApplyResources(panel_ViewContainer, "panel_ViewContainer");
			panel_ViewContainer.Controls.Add(panel_RenderContainer);
			panel_ViewContainer.Name = "panel_ViewContainer";
			toolTip.SetToolTip(panel_ViewContainer, resources.GetString("panel_ViewContainer.ToolTip"));
			// 
			// panel_RenderContainer
			// 
			resources.ApplyResources(panel_RenderContainer, "panel_RenderContainer");
			panel_RenderContainer.BackColor = SystemColors.ControlDark;
			panel_RenderContainer.Controls.Add(panel_Render);
			panel_RenderContainer.Name = "panel_RenderContainer";
			toolTip.SetToolTip(panel_RenderContainer, resources.GetString("panel_RenderContainer.ToolTip"));
			panel_RenderContainer.SizeChanged += panel_RenderContainer_SizeChanged;
			// 
			// spinePreviewFullScreenForm
			// 
			resources.ApplyResources(spinePreviewFullScreenForm, "spinePreviewFullScreenForm");
			spinePreviewFullScreenForm.ControlBox = false;
			spinePreviewFullScreenForm.FormBorderStyle = FormBorderStyle.None;
			spinePreviewFullScreenForm.MaximizeBox = false;
			spinePreviewFullScreenForm.MinimizeBox = false;
			spinePreviewFullScreenForm.Name = "SpinePreviewFullScreenForm";
			spinePreviewFullScreenForm.ShowIcon = false;
			spinePreviewFullScreenForm.ShowInTaskbar = false;
			toolTip.SetToolTip(spinePreviewFullScreenForm, resources.GetString("spinePreviewFullScreenForm.ToolTip"));
			spinePreviewFullScreenForm.TopMost = true;
			spinePreviewFullScreenForm.FormClosing += spinePreviewFullScreenForm_FormClosing;
			spinePreviewFullScreenForm.KeyDown += spinePreviewFullScreenForm_KeyDown;
			// 
			// wallpaperForm
			// 
			resources.ApplyResources(wallpaperForm, "wallpaperForm");
			wallpaperForm.ControlBox = false;
			wallpaperForm.FormBorderStyle = FormBorderStyle.None;
			wallpaperForm.MaximizeBox = false;
			wallpaperForm.MinimizeBox = false;
			wallpaperForm.Name = "WallpaperForm";
			wallpaperForm.ShowIcon = false;
			wallpaperForm.ShowInTaskbar = false;
			toolTip.SetToolTip(wallpaperForm, resources.GetString("wallpaperForm.ToolTip"));
			wallpaperForm.WindowState = FormWindowState.Minimized;
			wallpaperForm.FormClosing += wallpaperForm_FormClosing;
			// 
			// SpinePreviewPanel
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(tableLayoutPanel1);
			Name = "SpinePreviewPanel";
			toolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
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
