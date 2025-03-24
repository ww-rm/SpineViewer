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
            openFileDialog_Skel = new OpenFileDialog();
            panel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel_TargetFormat.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel
            // 
            panel.Controls.Add(tableLayoutPanel1);
            panel.Dock = DockStyle.Fill;
            panel.Location = new Point(0, 0);
            panel.Name = "panel";
            panel.Padding = new Padding(50, 15, 50, 10);
            panel.Size = new Size(1051, 538);
            panel.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(comboBox_TargetVersion, 1, 3);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel_TargetFormat, 1, 4);
            tableLayoutPanel1.Controls.Add(label1, 0, 3);
            tableLayoutPanel1.Controls.Add(label4, 0, 0);
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            tableLayoutPanel1.Controls.Add(comboBox_SourceVersion, 1, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 5);
            tableLayoutPanel1.Controls.Add(label2, 0, 4);
            tableLayoutPanel1.Controls.Add(skelFileListBox, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(50, 15);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(951, 513);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // comboBox_TargetVersion
            // 
            comboBox_TargetVersion.Anchor = AnchorStyles.Left;
            comboBox_TargetVersion.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_TargetVersion.FormattingEnabled = true;
            comboBox_TargetVersion.Location = new Point(95, 365);
            comboBox_TargetVersion.Name = "comboBox_TargetVersion";
            comboBox_TargetVersion.Size = new Size(182, 32);
            comboBox_TargetVersion.Sorted = true;
            comboBox_TargetVersion.TabIndex = 21;
            // 
            // flowLayoutPanel_TargetFormat
            // 
            flowLayoutPanel_TargetFormat.AutoSize = true;
            flowLayoutPanel_TargetFormat.Controls.Add(radioButton_BinaryTarget);
            flowLayoutPanel_TargetFormat.Controls.Add(radioButton_JsonTarget);
            flowLayoutPanel_TargetFormat.Dock = DockStyle.Fill;
            flowLayoutPanel_TargetFormat.Location = new Point(95, 403);
            flowLayoutPanel_TargetFormat.Name = "flowLayoutPanel_TargetFormat";
            flowLayoutPanel_TargetFormat.Size = new Size(853, 34);
            flowLayoutPanel_TargetFormat.TabIndex = 19;
            // 
            // radioButton_BinaryTarget
            // 
            radioButton_BinaryTarget.AutoSize = true;
            radioButton_BinaryTarget.Location = new Point(3, 3);
            radioButton_BinaryTarget.Name = "radioButton_BinaryTarget";
            radioButton_BinaryTarget.Size = new Size(151, 28);
            radioButton_BinaryTarget.TabIndex = 17;
            radioButton_BinaryTarget.Text = "二进制 (*.skel)";
            radioButton_BinaryTarget.UseVisualStyleBackColor = true;
            // 
            // radioButton_JsonTarget
            // 
            radioButton_JsonTarget.AutoSize = true;
            radioButton_JsonTarget.Checked = true;
            radioButton_JsonTarget.Location = new Point(160, 3);
            radioButton_JsonTarget.Name = "radioButton_JsonTarget";
            radioButton_JsonTarget.Size = new Size(135, 28);
            radioButton_JsonTarget.TabIndex = 18;
            radioButton_JsonTarget.TabStop = true;
            radioButton_JsonTarget.Text = "文本 (*.json)";
            radioButton_JsonTarget.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(3, 369);
            label1.Name = "label1";
            label1.Size = new Size(86, 24);
            label1.TabIndex = 15;
            label1.Text = "目标版本:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label4, 4);
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(15, 15);
            label4.Margin = new Padding(15);
            label4.Name = "label4";
            label4.Size = new Size(921, 24);
            label4.TabIndex = 14;
            label4.Text = "说明：将在每个文件同级目录下生成目标格式后缀的文件，会覆盖已存在文件";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(21, 331);
            label3.Name = "label3";
            label3.Size = new Size(68, 24);
            label3.TabIndex = 12;
            label3.Text = "源版本:";
            // 
            // comboBox_SourceVersion
            // 
            comboBox_SourceVersion.Anchor = AnchorStyles.Left;
            comboBox_SourceVersion.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_SourceVersion.FormattingEnabled = true;
            comboBox_SourceVersion.Location = new Point(95, 327);
            comboBox_SourceVersion.Name = "comboBox_SourceVersion";
            comboBox_SourceVersion.Size = new Size(182, 32);
            comboBox_SourceVersion.Sorted = true;
            comboBox_SourceVersion.TabIndex = 13;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel1.SetColumnSpan(tableLayoutPanel2, 4);
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(button_Ok, 0, 0);
            tableLayoutPanel2.Controls.Add(button_Cancel, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 470);
            tableLayoutPanel2.Margin = new Padding(3, 30, 3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(945, 40);
            tableLayoutPanel2.TabIndex = 11;
            // 
            // button_Ok
            // 
            button_Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_Ok.Location = new Point(330, 3);
            button_Ok.Margin = new Padding(3, 3, 30, 3);
            button_Ok.Name = "button_Ok";
            button_Ok.Size = new Size(112, 34);
            button_Ok.TabIndex = 7;
            button_Ok.Text = "确认";
            button_Ok.UseVisualStyleBackColor = true;
            button_Ok.Click += button_Ok_Click;
            // 
            // button_Cancel
            // 
            button_Cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button_Cancel.Location = new Point(502, 3);
            button_Cancel.Margin = new Padding(30, 3, 3, 3);
            button_Cancel.Name = "button_Cancel";
            button_Cancel.Size = new Size(112, 34);
            button_Cancel.TabIndex = 8;
            button_Cancel.Text = "取消";
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += button_Cancel_Click;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(3, 408);
            label2.Name = "label2";
            label2.Size = new Size(86, 24);
            label2.TabIndex = 16;
            label2.Text = "目标格式:";
            // 
            // skelFileListBox
            // 
            tableLayoutPanel1.SetColumnSpan(skelFileListBox, 2);
            skelFileListBox.Dock = DockStyle.Fill;
            skelFileListBox.Location = new Point(3, 57);
            skelFileListBox.Name = "skelFileListBox";
            skelFileListBox.Size = new Size(945, 264);
            skelFileListBox.TabIndex = 20;
            // 
            // openFileDialog_Skel
            // 
            openFileDialog_Skel.AddExtension = false;
            openFileDialog_Skel.AddToRecent = false;
            openFileDialog_Skel.Filter = "skel 文件 (*.skel; *.json)|*.skel;*.json|二进制文件 (*.skel)|*.skel|文本文件 (*.json)|*.json|所有文件 (*.*)|*.*";
            openFileDialog_Skel.Multiselect = true;
            openFileDialog_Skel.Title = "批量选择skel文件";
            // 
            // ConvertFileFormatDialog
            // 
            AcceptButton = button_Ok;
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ClientSize = new Size(1051, 538);
            Controls.Add(panel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConvertFileFormatDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "骨骼文件格式转换";
            panel.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel_TargetFormat.ResumeLayout(false);
            flowLayoutPanel_TargetFormat.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
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
        private OpenFileDialog openFileDialog_Skel;
        private Label label1;
        private Label label2;
        private FlowLayoutPanel flowLayoutPanel_TargetFormat;
        private RadioButton radioButton_BinaryTarget;
        private RadioButton radioButton_JsonTarget;
        private Controls.SkelFileListBox skelFileListBox;
        private ComboBox comboBox_TargetVersion;
    }
}