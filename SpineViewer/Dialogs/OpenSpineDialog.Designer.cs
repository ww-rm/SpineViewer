namespace SpineViewer.Dialogs
{
    partial class OpenSpineDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenSpineDialog));
			panel1 = new Panel();
			tableLayoutPanel1 = new TableLayoutPanel();
			label4 = new Label();
			label1 = new Label();
			label2 = new Label();
			label3 = new Label();
			textBox_SkelPath = new TextBox();
			button_SelectSkel = new Button();
			button_SelectAtlas = new Button();
			comboBox_Version = new ComboBox();
			textBox_AtlasPath = new TextBox();
			tableLayoutPanel2 = new TableLayoutPanel();
			button_Ok = new Button();
			button_Cancel = new Button();
			openFileDialog_Skel = new OpenFileDialog();
			openFileDialog_Atlas = new OpenFileDialog();
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
			tableLayoutPanel1.Controls.Add(label4, 0, 0);
			tableLayoutPanel1.Controls.Add(label1, 0, 1);
			tableLayoutPanel1.Controls.Add(label2, 0, 2);
			tableLayoutPanel1.Controls.Add(label3, 0, 3);
			tableLayoutPanel1.Controls.Add(textBox_SkelPath, 1, 1);
			tableLayoutPanel1.Controls.Add(button_SelectSkel, 3, 1);
			tableLayoutPanel1.Controls.Add(button_SelectAtlas, 3, 2);
			tableLayoutPanel1.Controls.Add(comboBox_Version, 1, 3);
			tableLayoutPanel1.Controls.Add(textBox_AtlasPath, 1, 2);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 4);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// label4
			// 
			resources.ApplyResources(label4, "label4");
			tableLayoutPanel1.SetColumnSpan(label4, 4);
			label4.Name = "label4";
			// 
			// label1
			// 
			resources.ApplyResources(label1, "label1");
			label1.Name = "label1";
			// 
			// label2
			// 
			resources.ApplyResources(label2, "label2");
			label2.Name = "label2";
			// 
			// label3
			// 
			resources.ApplyResources(label3, "label3");
			label3.Name = "label3";
			// 
			// textBox_SkelPath
			// 
			resources.ApplyResources(textBox_SkelPath, "textBox_SkelPath");
			tableLayoutPanel1.SetColumnSpan(textBox_SkelPath, 2);
			textBox_SkelPath.Name = "textBox_SkelPath";
			// 
			// button_SelectSkel
			// 
			resources.ApplyResources(button_SelectSkel, "button_SelectSkel");
			button_SelectSkel.Name = "button_SelectSkel";
			button_SelectSkel.UseVisualStyleBackColor = true;
			button_SelectSkel.Click += button_SelectSkel_Click;
			// 
			// button_SelectAtlas
			// 
			resources.ApplyResources(button_SelectAtlas, "button_SelectAtlas");
			button_SelectAtlas.Name = "button_SelectAtlas";
			button_SelectAtlas.UseVisualStyleBackColor = true;
			button_SelectAtlas.Click += button_SelectAtlas_Click;
			// 
			// comboBox_Version
			// 
			resources.ApplyResources(comboBox_Version, "comboBox_Version");
			comboBox_Version.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_Version.FormattingEnabled = true;
			comboBox_Version.Name = "comboBox_Version";
			comboBox_Version.Sorted = true;
			// 
			// textBox_AtlasPath
			// 
			resources.ApplyResources(textBox_AtlasPath, "textBox_AtlasPath");
			tableLayoutPanel1.SetColumnSpan(textBox_AtlasPath, 2);
			textBox_AtlasPath.Name = "textBox_AtlasPath";
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
			// openFileDialog_Skel
			// 
			openFileDialog_Skel.AddExtension = false;
			openFileDialog_Skel.AddToRecent = false;
			resources.ApplyResources(openFileDialog_Skel, "openFileDialog_Skel");
			// 
			// openFileDialog_Atlas
			// 
			openFileDialog_Atlas.AddExtension = false;
			openFileDialog_Atlas.AddToRecent = false;
			resources.ApplyResources(openFileDialog_Atlas, "openFileDialog_Atlas");
			// 
			// OpenSpineDialog
			// 
			AcceptButton = button_Ok;
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = button_Cancel;
			Controls.Add(panel1);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "OpenSpineDialog";
			ShowInTaskbar = false;
			Load += OpenSpineDialog_Load;
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
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox textBox_SkelPath;
        private Button button_SelectSkel;
        private Button button_SelectAtlas;
        private Button button_Ok;
        private Button button_Cancel;
        private ComboBox comboBox_Version;
        private TextBox textBox_AtlasPath;
        private TableLayoutPanel tableLayoutPanel2;
        private OpenFileDialog openFileDialog_Skel;
        private OpenFileDialog openFileDialog_Atlas;
        private Label label4;
    }
}