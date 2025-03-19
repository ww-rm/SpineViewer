namespace SpineViewer.Dialogs
{
    partial class ExportPngDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportPngDialog));
            panel1 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            label4 = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            textBox_OutputDir = new TextBox();
            button_SelectOutputDir = new Button();
            tableLayoutPanel2 = new TableLayoutPanel();
            button_Ok = new Button();
            button_Cancel = new Button();
            numericUpDown_Duration = new NumericUpDown();
            numericUpDown_Fps = new NumericUpDown();
            folderBrowserDialog = new FolderBrowserDialog();
            panel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_Duration).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_Fps).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(50, 15, 50, 10);
            panel1.Size = new Size(919, 276);
            panel1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(label4, 0, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            tableLayoutPanel1.Controls.Add(label2, 0, 2);
            tableLayoutPanel1.Controls.Add(label3, 0, 3);
            tableLayoutPanel1.Controls.Add(textBox_OutputDir, 1, 1);
            tableLayoutPanel1.Controls.Add(button_SelectOutputDir, 3, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 4);
            tableLayoutPanel1.Controls.Add(numericUpDown_Duration, 1, 2);
            tableLayoutPanel1.Controls.Add(numericUpDown_Fps, 1, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(50, 15);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(819, 251);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label4, 4);
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(15, 15);
            label4.Margin = new Padding(15);
            label4.Name = "label4";
            label4.Size = new Size(789, 24);
            label4.TabIndex = 11;
            label4.Text = "说明：时长不足一帧时仅导出第一帧";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(3, 62);
            label1.Name = "label1";
            label1.Size = new Size(104, 24);
            label1.TabIndex = 0;
            label1.Text = "输出文件夹:";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(57, 100);
            label2.Name = "label2";
            label2.Size = new Size(50, 24);
            label2.TabIndex = 1;
            label2.Text = "时长:";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(57, 136);
            label3.Name = "label3";
            label3.Size = new Size(50, 24);
            label3.TabIndex = 2;
            label3.Text = "帧率:";
            // 
            // textBox_OutputDir
            // 
            tableLayoutPanel1.SetColumnSpan(textBox_OutputDir, 2);
            textBox_OutputDir.Dock = DockStyle.Fill;
            textBox_OutputDir.Location = new Point(113, 57);
            textBox_OutputDir.Name = "textBox_OutputDir";
            textBox_OutputDir.Size = new Size(664, 30);
            textBox_OutputDir.TabIndex = 3;
            // 
            // button_SelectOutputDir
            // 
            button_SelectOutputDir.AutoSize = true;
            button_SelectOutputDir.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_SelectOutputDir.Location = new Point(783, 57);
            button_SelectOutputDir.Name = "button_SelectOutputDir";
            button_SelectOutputDir.Size = new Size(32, 34);
            button_SelectOutputDir.TabIndex = 5;
            button_SelectOutputDir.Text = "...";
            button_SelectOutputDir.UseVisualStyleBackColor = true;
            button_SelectOutputDir.Click += button_SelectOutputDir_Click;
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
            tableLayoutPanel2.Dock = DockStyle.Bottom;
            tableLayoutPanel2.Location = new Point(3, 208);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(813, 40);
            tableLayoutPanel2.TabIndex = 10;
            // 
            // button_Ok
            // 
            button_Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_Ok.Location = new Point(264, 3);
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
            button_Cancel.Location = new Point(436, 3);
            button_Cancel.Margin = new Padding(30, 3, 3, 3);
            button_Cancel.Name = "button_Cancel";
            button_Cancel.Size = new Size(112, 34);
            button_Cancel.TabIndex = 8;
            button_Cancel.Text = "取消";
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += button_Cancel_Click;
            // 
            // numericUpDown_Duration
            // 
            numericUpDown_Duration.Anchor = AnchorStyles.Left;
            numericUpDown_Duration.DecimalPlaces = 3;
            numericUpDown_Duration.Location = new Point(113, 97);
            numericUpDown_Duration.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numericUpDown_Duration.Name = "numericUpDown_Duration";
            numericUpDown_Duration.Size = new Size(180, 30);
            numericUpDown_Duration.TabIndex = 12;
            numericUpDown_Duration.TextAlign = HorizontalAlignment.Right;
            numericUpDown_Duration.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // numericUpDown_Fps
            // 
            numericUpDown_Fps.Anchor = AnchorStyles.Left;
            numericUpDown_Fps.Location = new Point(113, 133);
            numericUpDown_Fps.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numericUpDown_Fps.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown_Fps.Name = "numericUpDown_Fps";
            numericUpDown_Fps.Size = new Size(180, 30);
            numericUpDown_Fps.TabIndex = 13;
            numericUpDown_Fps.TextAlign = HorizontalAlignment.Right;
            numericUpDown_Fps.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // folderBrowserDialog
            // 
            folderBrowserDialog.AddToRecent = false;
            // 
            // ExportPngDialog
            // 
            AcceptButton = button_Ok;
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ClientSize = new Size(919, 276);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExportPngDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "导出PNG序列";
            Load += ExportPngDialog_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numericUpDown_Duration).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown_Fps).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label4;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox textBox_OutputDir;
        private Button button_SelectOutputDir;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button_Ok;
        private Button button_Cancel;
        private NumericUpDown numericUpDown_Duration;
        private NumericUpDown numericUpDown_Fps;
        private FolderBrowserDialog folderBrowserDialog;
    }
}