namespace SpineViewer.Dialogs
{
    partial class ConvertFileFormatDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConvertFileFormatDialog));
            panel = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            label5 = new Label();
            comboBox_TargetVersion = new ComboBox();
            flowLayoutPanel_TargetFormat = new FlowLayoutPanel();
            radioButton_BinaryTarget = new RadioButton();
            radioButton_JsonTarget = new RadioButton();
            label1 = new Label();
            label4 = new Label();
            label3 = new Label();
            comboBox_SourceVersion = new ComboBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            button_Ok = new Button();
            button_Cancel = new Button();
            label2 = new Label();
            skelFileListBox = new SpineViewer.Controls.SkelFileListBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            textBox_OutputDir = new TextBox();
            button_SelectOutputDir = new Button();
            folderBrowserDialog_Output = new FolderBrowserDialog();
            panel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel_TargetFormat.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
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
            tableLayoutPanel1.Controls.Add(label5, 0, 2);
            tableLayoutPanel1.Controls.Add(comboBox_TargetVersion, 1, 4);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel_TargetFormat, 1, 5);
            tableLayoutPanel1.Controls.Add(label1, 0, 4);
            tableLayoutPanel1.Controls.Add(label4, 0, 0);
            tableLayoutPanel1.Controls.Add(label3, 0, 3);
            tableLayoutPanel1.Controls.Add(comboBox_SourceVersion, 1, 3);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 6);
            tableLayoutPanel1.Controls.Add(label2, 0, 5);
            tableLayoutPanel1.Controls.Add(skelFileListBox, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 1, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.Name = "label5";
            // 
            // comboBox_TargetVersion
            // 
            resources.ApplyResources(comboBox_TargetVersion, "comboBox_TargetVersion");
            comboBox_TargetVersion.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_TargetVersion.FormattingEnabled = true;
            comboBox_TargetVersion.Name = "comboBox_TargetVersion";
            comboBox_TargetVersion.Sorted = true;
            // 
            // flowLayoutPanel_TargetFormat
            // 
            resources.ApplyResources(flowLayoutPanel_TargetFormat, "flowLayoutPanel_TargetFormat");
            flowLayoutPanel_TargetFormat.Controls.Add(radioButton_BinaryTarget);
            flowLayoutPanel_TargetFormat.Controls.Add(radioButton_JsonTarget);
            flowLayoutPanel_TargetFormat.Name = "flowLayoutPanel_TargetFormat";
            // 
            // radioButton_BinaryTarget
            // 
            resources.ApplyResources(radioButton_BinaryTarget, "radioButton_BinaryTarget");
            radioButton_BinaryTarget.Name = "radioButton_BinaryTarget";
            radioButton_BinaryTarget.UseVisualStyleBackColor = true;
            // 
            // radioButton_JsonTarget
            // 
            resources.ApplyResources(radioButton_JsonTarget, "radioButton_JsonTarget");
            radioButton_JsonTarget.Checked = true;
            radioButton_JsonTarget.Name = "radioButton_JsonTarget";
            radioButton_JsonTarget.TabStop = true;
            radioButton_JsonTarget.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
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
            // comboBox_SourceVersion
            // 
            resources.ApplyResources(comboBox_SourceVersion, "comboBox_SourceVersion");
            comboBox_SourceVersion.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_SourceVersion.FormattingEnabled = true;
            comboBox_SourceVersion.Name = "comboBox_SourceVersion";
            comboBox_SourceVersion.Sorted = true;
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
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // skelFileListBox
            // 
            resources.ApplyResources(skelFileListBox, "skelFileListBox");
            tableLayoutPanel1.SetColumnSpan(skelFileListBox, 2);
            skelFileListBox.Name = "skelFileListBox";
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(tableLayoutPanel3, "tableLayoutPanel3");
            tableLayoutPanel3.Controls.Add(textBox_OutputDir, 1, 0);
            tableLayoutPanel3.Controls.Add(button_SelectOutputDir, 2, 0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // textBox_OutputDir
            // 
            resources.ApplyResources(textBox_OutputDir, "textBox_OutputDir");
            textBox_OutputDir.Name = "textBox_OutputDir";
            // 
            // button_SelectOutputDir
            // 
            resources.ApplyResources(button_SelectOutputDir, "button_SelectOutputDir");
            button_SelectOutputDir.Name = "button_SelectOutputDir";
            button_SelectOutputDir.UseVisualStyleBackColor = true;
            button_SelectOutputDir.Click += button_SelectOutputDir_Click;
            // 
            // folderBrowserDialog_Output
            // 
            resources.ApplyResources(folderBrowserDialog_Output, "folderBrowserDialog_Output");
            // 
            // ConvertFileFormatDialog
            // 
            AcceptButton = button_Ok;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            Controls.Add(panel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConvertFileFormatDialog";
            ShowInTaskbar = false;
            panel.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel_TargetFormat.ResumeLayout(false);
            flowLayoutPanel_TargetFormat.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label4;
        private Label label3;
        private ComboBox comboBox_SourceVersion;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button_Ok;
        private Button button_Cancel;
        private Label label1;
        private Label label2;
        private FlowLayoutPanel flowLayoutPanel_TargetFormat;
        private RadioButton radioButton_BinaryTarget;
        private RadioButton radioButton_JsonTarget;
        private Controls.SkelFileListBox skelFileListBox;
        private ComboBox comboBox_TargetVersion;
        private FolderBrowserDialog folderBrowserDialog_Output;
        private TableLayoutPanel tableLayoutPanel3;
        private TextBox textBox_OutputDir;
        private Button button_SelectOutputDir;
        private Label label5;
    }
}