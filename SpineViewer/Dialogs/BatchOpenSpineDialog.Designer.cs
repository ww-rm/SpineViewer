namespace SpineViewer.Dialogs
{
    partial class BatchOpenSpineDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BatchOpenSpineDialog));
			panel = new Panel();
			tableLayoutPanel1 = new TableLayoutPanel();
			label4 = new Label();
			label3 = new Label();
			comboBox_Version = new ComboBox();
			tableLayoutPanel2 = new TableLayoutPanel();
			button_Ok = new Button();
			button_Cancel = new Button();
			skelFileListBox = new Controls.SkelFileListBox();
			panel.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();
			tableLayoutPanel2.SuspendLayout();
			SuspendLayout();
			// 
			// panel
			// 
			resources.ApplyResources(panel, "panel");
			panel.Controls.Add(tableLayoutPanel1);
			panel.Name = "panel";
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
			tableLayoutPanel1.Controls.Add(label4, 0, 0);
			tableLayoutPanel1.Controls.Add(label3, 0, 2);
			tableLayoutPanel1.Controls.Add(comboBox_Version, 1, 2);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 3);
			tableLayoutPanel1.Controls.Add(skelFileListBox, 0, 1);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// label4
			// 
			resources.ApplyResources(label4, "label4");
			tableLayoutPanel1.SetColumnSpan(label4, 4);
			label4.Name = "label4";
			// 
			// label3
			// 
			resources.ApplyResources(label3, "label3");
			label3.Name = "label3";
			// 
			// comboBox_Version
			// 
			resources.ApplyResources(comboBox_Version, "comboBox_Version");
			comboBox_Version.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_Version.FormattingEnabled = true;
			comboBox_Version.Name = "comboBox_Version";
			comboBox_Version.Sorted = true;
			// 
			// tableLayoutPanel2
			// 
			resources.ApplyResources(tableLayoutPanel2, "tableLayoutPanel2");
			tableLayoutPanel1.SetColumnSpan(tableLayoutPanel2, 4);
			tableLayoutPanel2.Controls.Add(button_Ok, 0, 0);
			tableLayoutPanel2.Controls.Add(button_Cancel, 1, 0);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			// 
			// button_Ok
			// 
			resources.ApplyResources(button_Ok, "button_Ok");
			button_Ok.Name = "button_Ok";
			button_Ok.UseVisualStyleBackColor = true;
			button_Ok.Click += button_Ok_Click;
			// 
			// button_Cancel
			// 
			resources.ApplyResources(button_Cancel, "button_Cancel");
			button_Cancel.Name = "button_Cancel";
			button_Cancel.UseVisualStyleBackColor = true;
			button_Cancel.Click += button_Cancel_Click;
			// 
			// skelFileListBox
			// 
			resources.ApplyResources(skelFileListBox, "skelFileListBox");
			tableLayoutPanel1.SetColumnSpan(skelFileListBox, 2);
			skelFileListBox.Name = "skelFileListBox";
			// 
			// BatchOpenSpineDialog
			// 
			AcceptButton = button_Ok;
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = button_Cancel;
			Controls.Add(panel);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "BatchOpenSpineDialog";
			ShowInTaskbar = false;
			panel.ResumeLayout(false);
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			tableLayoutPanel2.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private Panel panel;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button_Ok;
        private Button button_Cancel;
        private Label label3;
        private ComboBox comboBox_Version;
        private Label label4;
        private Controls.SkelFileListBox skelFileListBox;
    }
}