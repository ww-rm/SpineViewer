namespace SpineViewer.Dialogs
{
    partial class ExportPreviewDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportPreviewDialog));
            panel1 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            propertyGrid = new PropertyGrid();
            label4 = new Label();
            label1 = new Label();
            textBox_OutputDir = new TextBox();
            button_SelectOutputDir = new Button();
            tableLayoutPanel2 = new TableLayoutPanel();
            button_Ok = new Button();
            button_Cancel = new Button();
            folderBrowserDialog = new FolderBrowserDialog();
            panel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(50, 15, 50, 10);
            panel1.Size = new Size(914, 482);
            panel1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(propertyGrid, 0, 2);
            tableLayoutPanel1.Controls.Add(label4, 0, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            tableLayoutPanel1.Controls.Add(textBox_OutputDir, 1, 1);
            tableLayoutPanel1.Controls.Add(button_SelectOutputDir, 3, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(50, 15);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(814, 457);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // propertyGrid
            // 
            tableLayoutPanel1.SetColumnSpan(propertyGrid, 4);
            propertyGrid.Dock = DockStyle.Fill;
            propertyGrid.HelpVisible = false;
            propertyGrid.Location = new Point(3, 97);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.Size = new Size(808, 284);
            propertyGrid.TabIndex = 1;
            propertyGrid.ToolbarVisible = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label4, 4);
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(15, 15);
            label4.Margin = new Padding(15);
            label4.Name = "label4";
            label4.Size = new Size(784, 24);
            label4.TabIndex = 11;
            label4.Text = "说明：输出文件夹为可选项，留空则将预览图输出到每个骨骼文件所在目录";
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
            // textBox_OutputDir
            // 
            tableLayoutPanel1.SetColumnSpan(textBox_OutputDir, 2);
            textBox_OutputDir.Dock = DockStyle.Fill;
            textBox_OutputDir.Location = new Point(113, 57);
            textBox_OutputDir.Name = "textBox_OutputDir";
            textBox_OutputDir.Size = new Size(660, 30);
            textBox_OutputDir.TabIndex = 3;
            // 
            // button_SelectOutputDir
            // 
            button_SelectOutputDir.AutoSize = true;
            button_SelectOutputDir.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_SelectOutputDir.Location = new Point(779, 57);
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
            tableLayoutPanel2.Location = new Point(3, 414);
            tableLayoutPanel2.Margin = new Padding(3, 30, 3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(808, 40);
            tableLayoutPanel2.TabIndex = 10;
            // 
            // button_Ok
            // 
            button_Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_Ok.Location = new Point(262, 3);
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
            button_Cancel.Location = new Point(434, 3);
            button_Cancel.Margin = new Padding(30, 3, 3, 3);
            button_Cancel.Name = "button_Cancel";
            button_Cancel.Size = new Size(112, 34);
            button_Cancel.TabIndex = 8;
            button_Cancel.Text = "取消";
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += button_Cancel_Click;
            // 
            // folderBrowserDialog
            // 
            folderBrowserDialog.AddToRecent = false;
            // 
            // ExportPreviewDialog
            // 
            AcceptButton = button_Ok;
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ClientSize = new Size(914, 482);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExportPreviewDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "导出预览图";
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
        private Label label4;
        private Label label1;
        private TextBox textBox_OutputDir;
        private Button button_SelectOutputDir;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button_Ok;
        private Button button_Cancel;
        private FolderBrowserDialog folderBrowserDialog;
        private PropertyGrid propertyGrid;
    }
}