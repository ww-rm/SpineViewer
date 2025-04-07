namespace SpineViewer.Dialogs
{
    partial class SpineAnimationEditorDialog
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
            panel = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            button_Add = new Button();
            button_Delete = new Button();
            button_Ok = new Button();
            propertyGrid_AnimationTracks = new PropertyGrid();
            panel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel
            // 
            panel.Controls.Add(tableLayoutPanel1);
            panel.Dock = DockStyle.Fill;
            panel.Location = new Point(0, 0);
            panel.Name = "panel";
            panel.Padding = new Padding(50, 15, 50, 10);
            panel.Size = new Size(666, 483);
            panel.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
            tableLayoutPanel1.Controls.Add(propertyGrid_AnimationTracks, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(50, 15);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(566, 458);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(button_Add);
            flowLayoutPanel1.Controls.Add(button_Delete);
            flowLayoutPanel1.Controls.Add(button_Ok);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 415);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(560, 40);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // button_Add
            // 
            button_Add.Location = new Point(3, 3);
            button_Add.Name = "button_Add";
            button_Add.Size = new Size(112, 34);
            button_Add.TabIndex = 0;
            button_Add.Text = "添加";
            button_Add.UseVisualStyleBackColor = true;
            button_Add.Click += button_Add_Click;
            // 
            // button_Delete
            // 
            button_Delete.Location = new Point(121, 3);
            button_Delete.Name = "button_Delete";
            button_Delete.Size = new Size(112, 34);
            button_Delete.TabIndex = 1;
            button_Delete.Text = "删除";
            button_Delete.UseVisualStyleBackColor = true;
            button_Delete.Click += button_Delete_Click;
            // 
            // button_Ok
            // 
            button_Ok.Location = new Point(239, 3);
            button_Ok.Name = "button_Ok";
            button_Ok.Size = new Size(112, 34);
            button_Ok.TabIndex = 2;
            button_Ok.Text = "确定";
            button_Ok.UseVisualStyleBackColor = true;
            button_Ok.Click += button_Ok_Click;
            // 
            // propertyGrid_AnimationTracks
            // 
            propertyGrid_AnimationTracks.Dock = DockStyle.Fill;
            propertyGrid_AnimationTracks.HelpVisible = false;
            propertyGrid_AnimationTracks.Location = new Point(3, 3);
            propertyGrid_AnimationTracks.Name = "propertyGrid_AnimationTracks";
            propertyGrid_AnimationTracks.PropertySort = PropertySort.NoSort;
            propertyGrid_AnimationTracks.Size = new Size(560, 406);
            propertyGrid_AnimationTracks.TabIndex = 1;
            propertyGrid_AnimationTracks.ToolbarVisible = false;
            // 
            // AnimationTracksEditorDialog
            // 
            AcceptButton = button_Ok;
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(666, 483);
            Controls.Add(panel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AnimationTracksEditorDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "多轨道动画实时编辑器";
            panel.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button button_Add;
        private Button button_Delete;
        private PropertyGrid propertyGrid_AnimationTracks;
        private Button button_Ok;
    }
}