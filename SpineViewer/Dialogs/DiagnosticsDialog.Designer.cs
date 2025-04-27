namespace SpineViewer.Dialogs
{
    partial class DiagnosticsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagnosticsDialog));
			panel1 = new Panel();
			tableLayoutPanel1 = new TableLayoutPanel();
			button_Copy = new Button();
			propertyGrid = new PropertyGrid();
			panel1.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();
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
			tableLayoutPanel1.Controls.Add(button_Copy, 0, 1);
			tableLayoutPanel1.Controls.Add(propertyGrid, 0, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// button_Copy
			// 
			resources.ApplyResources(button_Copy, "button_Copy");
			button_Copy.Name = "button_Copy";
			button_Copy.UseVisualStyleBackColor = true;
			button_Copy.Click += button_Copy_Click;
			// 
			// propertyGrid
			// 
			resources.ApplyResources(propertyGrid, "propertyGrid");
			propertyGrid.Name = "propertyGrid";
			propertyGrid.ToolbarVisible = false;
			// 
			// DiagnosticsDialog
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(panel1);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "DiagnosticsDialog";
			ShowInTaskbar = false;
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Panel panel1;
        private TableLayoutPanel tableLayoutPanel1;
        private Button button_Copy;
        private PropertyGrid propertyGrid;
    }
}