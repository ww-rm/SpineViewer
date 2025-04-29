namespace SpineViewer.Dialogs
{
    partial class ExportDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportDialog));
			panel1 = new Panel();
			tableLayoutPanel1 = new TableLayoutPanel();
			propertyGrid_ExportArgs = new PropertyGrid();
			tableLayoutPanel2 = new TableLayoutPanel();
			button_Ok = new Button();
			button_Cancel = new Button();
			panel1.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();
			tableLayoutPanel2.SuspendLayout();
			SuspendLayout();
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
			tableLayoutPanel1.Controls.Add(propertyGrid_ExportArgs, 0, 0);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// propertyGrid_ExportArgs
			// 
			resources.ApplyResources(propertyGrid_ExportArgs, "propertyGrid_ExportArgs");
			propertyGrid_ExportArgs.Name = "propertyGrid_ExportArgs";
			propertyGrid_ExportArgs.PropertySort = PropertySort.Categorized;
			propertyGrid_ExportArgs.ToolbarVisible = false;
			// 
			// tableLayoutPanel2
			// 
			resources.ApplyResources(tableLayoutPanel2, "tableLayoutPanel2");
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
			// ExportDialog
			// 
			AcceptButton = button_Ok;
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = button_Cancel;
			Controls.Add(panel1);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ExportDialog";
			ShowInTaskbar = false;
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			tableLayoutPanel2.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Panel panel1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button_Ok;
        private Button button_Cancel;
        private PropertyGrid propertyGrid_ExportArgs;
    }
}