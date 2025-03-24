namespace SpineViewer.Controls
{
    partial class SpinePreviewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpinePreviewer));
            panel = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel_Container = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            button_Start = new Button();
            imageList = new ImageList(components);
            button_Stop = new Button();
            toolTip = new ToolTip(components);
            tableLayoutPanel1.SuspendLayout();
            panel_Container.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel
            // 
            panel.BackColor = SystemColors.ControlDarkDark;
            panel.Location = new Point(151, 136);
            panel.Margin = new Padding(0);
            panel.Name = "panel";
            panel.Size = new Size(320, 320);
            panel.TabIndex = 1;
            panel.MouseDown += panel_MouseDown;
            panel.MouseMove += panel_MouseMove;
            panel.MouseUp += panel_MouseUp;
            panel.MouseWheel += panel_MouseWheel;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel_Container, 0, 0);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
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
            // panel_Container
            // 
            panel_Container.BackColor = SystemColors.ControlDark;
            panel_Container.Controls.Add(panel);
            panel_Container.Dock = DockStyle.Fill;
            panel_Container.Location = new Point(0, 0);
            panel_Container.Margin = new Padding(0);
            panel_Container.Name = "panel_Container";
            panel_Container.Size = new Size(641, 594);
            panel_Container.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.None;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(button_Start);
            flowLayoutPanel1.Controls.Add(button_Stop);
            flowLayoutPanel1.Location = new Point(254, 594);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(132, 42);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // button_Start
            // 
            button_Start.AutoSize = true;
            button_Start.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_Start.BackgroundImageLayout = ImageLayout.Center;
            button_Start.ImageKey = "pause";
            button_Start.ImageList = imageList;
            button_Start.Location = new Point(3, 3);
            button_Start.Name = "button_Start";
            button_Start.Padding = new Padding(15, 3, 15, 3);
            button_Start.Size = new Size(60, 36);
            button_Start.TabIndex = 1;
            toolTip.SetToolTip(button_Start, "开始/暂停");
            button_Start.UseVisualStyleBackColor = true;
            button_Start.Click += button_Start_Click;
            // 
            // imageList
            // 
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageStream = (ImageListStreamer)resources.GetObject("imageList.ImageStream");
            imageList.TransparentColor = Color.Transparent;
            imageList.Images.SetKeyName(0, "start");
            imageList.Images.SetKeyName(1, "pause");
            imageList.Images.SetKeyName(2, "stop");
            // 
            // button_Stop
            // 
            button_Stop.AutoSize = true;
            button_Stop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_Stop.ImageKey = "stop";
            button_Stop.ImageList = imageList;
            button_Stop.Location = new Point(69, 3);
            button_Stop.Name = "button_Stop";
            button_Stop.Padding = new Padding(15, 3, 15, 3);
            button_Stop.Size = new Size(60, 36);
            button_Stop.TabIndex = 0;
            toolTip.SetToolTip(button_Stop, "停止更新并重置时间到初始");
            button_Stop.UseVisualStyleBackColor = true;
            button_Stop.Click += button_Stop_Click;
            // 
            // SpinePreviewer
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "SpinePreviewer";
            Size = new Size(641, 636);
            SizeChanged += SpinePreviewer_SizeChanged;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel_Container.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel_Container;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button button_Stop;
        private Button button_Start;
        private ImageList imageList;
        private ToolTip toolTip;
    }
}
