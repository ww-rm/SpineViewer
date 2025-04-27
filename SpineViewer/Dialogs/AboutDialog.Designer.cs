namespace SpineViewer.Dialogs
{
    partial class AboutDialog
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
			tableLayoutPanel_About = new TableLayoutPanel();
			label3 = new Label();
			label1 = new Label();
			label_Version = new Label();
			linkLabel_RepoUrl = new LinkLabel();
			panel1 = new Panel();
			tableLayoutPanel_About.SuspendLayout();
			panel1.SuspendLayout();
			SuspendLayout();
			// 
			// tableLayoutPanel_About
			// 
			resources.ApplyResources(tableLayoutPanel_About, "tableLayoutPanel_About");
			tableLayoutPanel_About.BackColor = Color.Transparent;
			tableLayoutPanel_About.Controls.Add(label3, 0, 1);
			tableLayoutPanel_About.Controls.Add(label1, 0, 0);
			tableLayoutPanel_About.Controls.Add(label_Version, 1, 0);
			tableLayoutPanel_About.Controls.Add(linkLabel_RepoUrl, 1, 1);
			tableLayoutPanel_About.Name = "tableLayoutPanel_About";
			// 
			// label3
			// 
			resources.ApplyResources(label3, "label3");
			label3.Name = "label3";
			// 
			// label1
			// 
			resources.ApplyResources(label1, "label1");
			label1.Name = "label1";
			// 
			// label_Version
			// 
			resources.ApplyResources(label_Version, "label_Version");
			label_Version.Name = "label_Version";
			// 
			// linkLabel_RepoUrl
			// 
			resources.ApplyResources(linkLabel_RepoUrl, "linkLabel_RepoUrl");
			linkLabel_RepoUrl.Name = "linkLabel_RepoUrl";
			linkLabel_RepoUrl.TabStop = true;
			linkLabel_RepoUrl.LinkClicked += linkLabel_RepoUrl_LinkClicked;
			// 
			// panel1
			// 
			resources.ApplyResources(panel1, "panel1");
			panel1.Controls.Add(tableLayoutPanel_About);
			panel1.Name = "panel1";
			// 
			// AboutDialog
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(panel1);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "AboutDialog";
			ShowInTaskbar = false;
			tableLayoutPanel_About.ResumeLayout(false);
			tableLayoutPanel_About.PerformLayout();
			panel1.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel tableLayoutPanel_About;
        private Label label3;
        private Label label1;
        private Label label_Version;
        private LinkLabel linkLabel_RepoUrl;
        private Panel panel1;
    }
}
