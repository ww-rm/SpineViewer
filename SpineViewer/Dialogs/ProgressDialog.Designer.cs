namespace SpineViewer.Dialogs
{
    partial class ProgressDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialog));
			progressBar = new ProgressBar();
			panel1 = new Panel();
			tableLayoutPanel1 = new TableLayoutPanel();
			button_Cancel = new Button();
			label_Tip = new Label();
			backgroundWorker = new System.ComponentModel.BackgroundWorker();
			panel1.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// progressBar
			// 
			resources.ApplyResources(progressBar, "progressBar");
			progressBar.Name = "progressBar";
			progressBar.Style = ProgressBarStyle.Continuous;
			// 
			// panel1
			// 
			resources.ApplyResources(panel1, "panel1");
			panel1.Controls.Add(tableLayoutPanel1);
			panel1.Name = "panel1";
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
			tableLayoutPanel1.Controls.Add(progressBar, 0, 1);
			tableLayoutPanel1.Controls.Add(button_Cancel, 0, 2);
			tableLayoutPanel1.Controls.Add(label_Tip, 0, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// button_Cancel
			// 
			resources.ApplyResources(button_Cancel, "button_Cancel");
			button_Cancel.Name = "button_Cancel";
			button_Cancel.UseVisualStyleBackColor = true;
			button_Cancel.Click += button_Cancel_Click;
			// 
			// label_Tip
			// 
			resources.ApplyResources(label_Tip, "label_Tip");
			label_Tip.Name = "label_Tip";
			// 
			// backgroundWorker
			// 
			backgroundWorker.WorkerReportsProgress = true;
			backgroundWorker.WorkerSupportsCancellation = true;
			backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
			backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
			// 
			// ProgressDialog
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			ControlBox = false;
			Controls.Add(panel1);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ProgressDialog";
			ShowIcon = false;
			ShowInTaskbar = false;
			panel1.ResumeLayout(false);
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private ProgressBar progressBar;
        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel1;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private Label label_Tip;
        private Button button_Cancel;
    }
}