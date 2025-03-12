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
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(50, 15, 50, 10);
            panel1.Size = new Size(907, 286);
            panel1.TabIndex = 0;
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
            tableLayoutPanel1.Controls.Add(textBox_SkelPath, 1, 1);
            tableLayoutPanel1.Controls.Add(button_SelectSkel, 3, 1);
            tableLayoutPanel1.Controls.Add(button_SelectAtlas, 3, 2);
            tableLayoutPanel1.Controls.Add(comboBox_Version, 1, 3);
            tableLayoutPanel1.Controls.Add(textBox_AtlasPath, 1, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 4);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(50, 15);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(807, 261);
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
            label4.Size = new Size(777, 24);
            label4.TabIndex = 11;
            label4.Text = "说明：如果没有选择atlas，则会自动读取与skel同目录下同名的atlas文件";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(10, 62);
            label1.Name = "label1";
            label1.Size = new Size(119, 24);
            label1.TabIndex = 0;
            label1.Text = "skel文件路径:";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(3, 102);
            label2.Name = "label2";
            label2.Size = new Size(126, 24);
            label2.TabIndex = 1;
            label2.Text = "atlas文件路径:";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(79, 141);
            label3.Name = "label3";
            label3.Size = new Size(50, 24);
            label3.TabIndex = 2;
            label3.Text = "版本:";
            // 
            // textBox_SkelPath
            // 
            tableLayoutPanel1.SetColumnSpan(textBox_SkelPath, 2);
            textBox_SkelPath.Dock = DockStyle.Fill;
            textBox_SkelPath.Location = new Point(135, 57);
            textBox_SkelPath.Name = "textBox_SkelPath";
            textBox_SkelPath.Size = new Size(630, 30);
            textBox_SkelPath.TabIndex = 3;
            // 
            // button_SelectSkel
            // 
            button_SelectSkel.AutoSize = true;
            button_SelectSkel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_SelectSkel.Location = new Point(771, 57);
            button_SelectSkel.Name = "button_SelectSkel";
            button_SelectSkel.Size = new Size(32, 34);
            button_SelectSkel.TabIndex = 5;
            button_SelectSkel.Text = "...";
            button_SelectSkel.UseVisualStyleBackColor = true;
            button_SelectSkel.Click += button_SelectSkel_Click;
            // 
            // button_SelectAtlas
            // 
            button_SelectAtlas.AutoSize = true;
            button_SelectAtlas.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button_SelectAtlas.Location = new Point(771, 97);
            button_SelectAtlas.Name = "button_SelectAtlas";
            button_SelectAtlas.Size = new Size(32, 34);
            button_SelectAtlas.TabIndex = 6;
            button_SelectAtlas.Text = "...";
            button_SelectAtlas.UseVisualStyleBackColor = true;
            button_SelectAtlas.Click += button_SelectAtlas_Click;
            // 
            // comboBox_Version
            // 
            comboBox_Version.Anchor = AnchorStyles.Left;
            comboBox_Version.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Version.FormattingEnabled = true;
            comboBox_Version.Location = new Point(135, 137);
            comboBox_Version.Name = "comboBox_Version";
            comboBox_Version.Size = new Size(182, 32);
            comboBox_Version.Sorted = true;
            comboBox_Version.TabIndex = 9;
            // 
            // textBox_AtlasPath
            // 
            tableLayoutPanel1.SetColumnSpan(textBox_AtlasPath, 2);
            textBox_AtlasPath.Dock = DockStyle.Fill;
            textBox_AtlasPath.Location = new Point(135, 97);
            textBox_AtlasPath.Name = "textBox_AtlasPath";
            textBox_AtlasPath.Size = new Size(630, 30);
            textBox_AtlasPath.TabIndex = 4;
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
            tableLayoutPanel2.Location = new Point(3, 218);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(801, 40);
            tableLayoutPanel2.TabIndex = 10;
            // 
            // button_Ok
            // 
            button_Ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_Ok.Location = new Point(258, 3);
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
            button_Cancel.Location = new Point(430, 3);
            button_Cancel.Margin = new Padding(30, 3, 3, 3);
            button_Cancel.Name = "button_Cancel";
            button_Cancel.Size = new Size(112, 34);
            button_Cancel.TabIndex = 8;
            button_Cancel.Text = "取消";
            button_Cancel.UseVisualStyleBackColor = true;
            button_Cancel.Click += button_Cancel_Click;
            // 
            // openFileDialog_Skel
            // 
            openFileDialog_Skel.AddExtension = false;
            openFileDialog_Skel.AddToRecent = false;
            openFileDialog_Skel.Filter = "skel 文件 (*.skel; *.json)|*.skel;*.json|所有文件 (*.*)|*.*";
            openFileDialog_Skel.Title = "选择skel文件";
            // 
            // openFileDialog_Atlas
            // 
            openFileDialog_Atlas.AddExtension = false;
            openFileDialog_Atlas.AddToRecent = false;
            openFileDialog_Atlas.Filter = "atlas 文件 (*.atlas)|*.atlas|所有文件 (*.*)|*.*";
            openFileDialog_Atlas.Title = "选择atlas文件";
            // 
            // OpenSpineDialog
            // 
            AcceptButton = button_Ok;
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = button_Cancel;
            ClientSize = new Size(907, 286);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OpenSpineDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "打开骨骼";
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