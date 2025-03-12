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
            listBox_FilePath = new ListBox();
            button_SelectSkel = new Button();
            label_Tip = new Label();
            openFileDialog_Skel = new OpenFileDialog();
            panel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
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
            panel.Size = new Size(1126, 449);
            panel.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(label4, 0, 0);
            tableLayoutPanel1.Controls.Add(label3, 0, 3);
            tableLayoutPanel1.Controls.Add(comboBox_Version, 1, 3);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 4);
            tableLayoutPanel1.Controls.Add(listBox_FilePath, 0, 2);
            tableLayoutPanel1.Controls.Add(button_SelectSkel, 0, 1);
            tableLayoutPanel1.Controls.Add(label_Tip, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(50, 15);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1026, 424);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label4, 4);
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(15, 15);
            label4.Margin = new Padding(15);
            label4.Name = "label4";
            label4.Size = new Size(996, 24);
            label4.TabIndex = 14;
            label4.Text = "说明：批量导入只需要选择skel文件，atlas文件需要在同目录下并且与skel文件名相同";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(90, 307);
            label3.Name = "label3";
            label3.Size = new Size(50, 24);
            label3.TabIndex = 12;
            label3.Text = "版本:";
            // 
            // comboBox_Version
            // 
            comboBox_Version.Anchor = AnchorStyles.Left;
            comboBox_Version.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Version.FormattingEnabled = true;
            comboBox_Version.Location = new Point(146, 303);
            comboBox_Version.Name = "comboBox_Version";
            comboBox_Version.Size = new Size(182, 32);
            comboBox_Version.Sorted = true;
            comboBox_Version.TabIndex = 13;
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
            tableLayoutPanel2.Location = new Point(3, 381);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(1020, 40);
            tableLayoutPanel2.TabIndex = 11;
            // 
            // button_Ok
            // 
            button_Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_Ok.Location = new Point(368, 3);
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
            button_Cancel.Location = new Point(540, 3);
            button_Cancel.Margin = new Padding(30, 3, 3, 3);
            button_Cancel.Name = "button_Cancel";
            button_Cancel.Size = new Size(112, 34);
            button_Cancel.TabIndex = 8;
            button_Cancel.Text = "取消";
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += button_Cancel_Click;
            // 
            // listBox_FilePath
            // 
            tableLayoutPanel1.SetColumnSpan(listBox_FilePath, 2);
            listBox_FilePath.Dock = DockStyle.Fill;
            listBox_FilePath.FormattingEnabled = true;
            listBox_FilePath.HorizontalScrollbar = true;
            listBox_FilePath.ItemHeight = 24;
            listBox_FilePath.Location = new Point(3, 97);
            listBox_FilePath.Name = "listBox_FilePath";
            listBox_FilePath.Size = new Size(1020, 200);
            listBox_FilePath.TabIndex = 2;
            // 
            // button_SelectSkel
            // 
            button_SelectSkel.Anchor = AnchorStyles.None;
            button_SelectSkel.Location = new Point(3, 57);
            button_SelectSkel.Name = "button_SelectSkel";
            button_SelectSkel.Size = new Size(137, 34);
            button_SelectSkel.TabIndex = 1;
            button_SelectSkel.Text = "选择文件...";
            button_SelectSkel.UseVisualStyleBackColor = true;
            button_SelectSkel.Click += button_SelectSkel_Click;
            // 
            // label_Tip
            // 
            label_Tip.AutoSize = true;
            label_Tip.Dock = DockStyle.Fill;
            label_Tip.Location = new Point(146, 54);
            label_Tip.Name = "label_Tip";
            label_Tip.Size = new Size(877, 40);
            label_Tip.TabIndex = 0;
            label_Tip.Text = "已选择 0 个文件";
            label_Tip.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // openFileDialog_Skel
            // 
            openFileDialog_Skel.AddExtension = false;
            openFileDialog_Skel.AddToRecent = false;
            openFileDialog_Skel.Filter = "skel 文件 (*.skel; *.json)|*.skel;*.json|所有文件 (*.*)|*.*";
            openFileDialog_Skel.Multiselect = true;
            openFileDialog_Skel.Title = "批量选择skel文件";
            // 
            // BatchOpenSpineDialog
            // 
            AcceptButton = button_Ok;
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ClientSize = new Size(1126, 449);
            Controls.Add(panel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BatchOpenSpineDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "批量打开骨骼";
            Load += BatchOpenSpineDialog_Load;
            panel.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panel;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label_Tip;
        private ListBox listBox_FilePath;
        private Button button_SelectSkel;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button_Ok;
        private Button button_Cancel;
        private Label label3;
        private ComboBox comboBox_Version;
        private OpenFileDialog openFileDialog_Skel;
        private Label label4;
    }
}