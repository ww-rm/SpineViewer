namespace SpineViewer.Controls
{
    partial class SpineViewPropertyGrid
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControl = new TabControl();
            tabPage_BaseInfo = new TabPage();
            propertyGrid_BaseInfo = new PropertyGrid();
            tabPage_Render = new TabPage();
            propertyGrid_Render = new PropertyGrid();
            tabPage_Transform = new TabPage();
            propertyGrid_Transform = new PropertyGrid();
            tabPage_Skin = new TabPage();
            propertyGrid_Skin = new PropertyGrid();
            contextMenuStrip_Skin = new ContextMenuStrip(components);
            toolStripMenuItem_ReloadSkins = new ToolStripMenuItem();
            tabPage_Slot = new TabPage();
            propertyGrid_Slot = new PropertyGrid();
            tabPage_Animation = new TabPage();
            propertyGrid_Animation = new PropertyGrid();
            contextMenuStrip_Animation = new ContextMenuStrip(components);
            toolStripMenuItem_AddAnimation = new ToolStripMenuItem();
            toolStripMenuItem_RemoveAnimation = new ToolStripMenuItem();
            tabPage_Debug = new TabPage();
            propertyGrid_Debug = new PropertyGrid();
            tabControl.SuspendLayout();
            tabPage_BaseInfo.SuspendLayout();
            tabPage_Render.SuspendLayout();
            tabPage_Transform.SuspendLayout();
            tabPage_Skin.SuspendLayout();
            contextMenuStrip_Skin.SuspendLayout();
            tabPage_Slot.SuspendLayout();
            tabPage_Animation.SuspendLayout();
            contextMenuStrip_Animation.SuspendLayout();
            tabPage_Debug.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Alignment = TabAlignment.Bottom;
            tabControl.Controls.Add(tabPage_BaseInfo);
            tabControl.Controls.Add(tabPage_Render);
            tabControl.Controls.Add(tabPage_Transform);
            tabControl.Controls.Add(tabPage_Skin);
            tabControl.Controls.Add(tabPage_Slot);
            tabControl.Controls.Add(tabPage_Animation);
            tabControl.Controls.Add(tabPage_Debug);
            tabControl.Dock = DockStyle.Fill;
            tabControl.ItemSize = new Size(90, 35);
            tabControl.Location = new Point(0, 0);
            tabControl.Multiline = true;
            tabControl.Name = "tabControl";
            tabControl.Padding = new Point(0, 0);
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(372, 448);
            tabControl.SizeMode = TabSizeMode.FillToRight;
            tabControl.TabIndex = 0;
            // 
            // tabPage_BaseInfo
            // 
            tabPage_BaseInfo.BackColor = SystemColors.Control;
            tabPage_BaseInfo.Controls.Add(propertyGrid_BaseInfo);
            tabPage_BaseInfo.Location = new Point(4, 4);
            tabPage_BaseInfo.Margin = new Padding(0);
            tabPage_BaseInfo.Name = "tabPage_BaseInfo";
            tabPage_BaseInfo.Size = new Size(364, 370);
            tabPage_BaseInfo.TabIndex = 0;
            tabPage_BaseInfo.Text = "基本信息";
            // 
            // propertyGrid_BaseInfo
            // 
            propertyGrid_BaseInfo.Dock = DockStyle.Fill;
            propertyGrid_BaseInfo.HelpVisible = false;
            propertyGrid_BaseInfo.Location = new Point(0, 0);
            propertyGrid_BaseInfo.Name = "propertyGrid_BaseInfo";
            propertyGrid_BaseInfo.PropertySort = PropertySort.Alphabetical;
            propertyGrid_BaseInfo.Size = new Size(364, 370);
            propertyGrid_BaseInfo.TabIndex = 0;
            propertyGrid_BaseInfo.ToolbarVisible = false;
            // 
            // tabPage_Render
            // 
            tabPage_Render.BackColor = SystemColors.Control;
            tabPage_Render.Controls.Add(propertyGrid_Render);
            tabPage_Render.Location = new Point(4, 4);
            tabPage_Render.Margin = new Padding(0);
            tabPage_Render.Name = "tabPage_Render";
            tabPage_Render.Size = new Size(364, 370);
            tabPage_Render.TabIndex = 1;
            tabPage_Render.Text = "渲染";
            // 
            // propertyGrid_Render
            // 
            propertyGrid_Render.Dock = DockStyle.Fill;
            propertyGrid_Render.HelpVisible = false;
            propertyGrid_Render.Location = new Point(0, 0);
            propertyGrid_Render.Name = "propertyGrid_Render";
            propertyGrid_Render.PropertySort = PropertySort.Alphabetical;
            propertyGrid_Render.Size = new Size(364, 370);
            propertyGrid_Render.TabIndex = 1;
            propertyGrid_Render.ToolbarVisible = false;
            // 
            // tabPage_Transform
            // 
            tabPage_Transform.BackColor = SystemColors.Control;
            tabPage_Transform.Controls.Add(propertyGrid_Transform);
            tabPage_Transform.Location = new Point(4, 4);
            tabPage_Transform.Margin = new Padding(0);
            tabPage_Transform.Name = "tabPage_Transform";
            tabPage_Transform.Size = new Size(364, 370);
            tabPage_Transform.TabIndex = 2;
            tabPage_Transform.Text = "变换";
            // 
            // propertyGrid_Transform
            // 
            propertyGrid_Transform.Dock = DockStyle.Fill;
            propertyGrid_Transform.HelpVisible = false;
            propertyGrid_Transform.Location = new Point(0, 0);
            propertyGrid_Transform.Name = "propertyGrid_Transform";
            propertyGrid_Transform.PropertySort = PropertySort.Alphabetical;
            propertyGrid_Transform.Size = new Size(364, 370);
            propertyGrid_Transform.TabIndex = 1;
            propertyGrid_Transform.ToolbarVisible = false;
            // 
            // tabPage_Skin
            // 
            tabPage_Skin.BackColor = SystemColors.Control;
            tabPage_Skin.Controls.Add(propertyGrid_Skin);
            tabPage_Skin.Location = new Point(4, 4);
            tabPage_Skin.Margin = new Padding(0);
            tabPage_Skin.Name = "tabPage_Skin";
            tabPage_Skin.Size = new Size(364, 370);
            tabPage_Skin.TabIndex = 3;
            tabPage_Skin.Text = "皮肤";
            // 
            // propertyGrid_Skin
            // 
            propertyGrid_Skin.ContextMenuStrip = contextMenuStrip_Skin;
            propertyGrid_Skin.Dock = DockStyle.Fill;
            propertyGrid_Skin.HelpVisible = false;
            propertyGrid_Skin.Location = new Point(0, 0);
            propertyGrid_Skin.Name = "propertyGrid_Skin";
            propertyGrid_Skin.PropertySort = PropertySort.NoSort;
            propertyGrid_Skin.Size = new Size(364, 370);
            propertyGrid_Skin.TabIndex = 1;
            propertyGrid_Skin.ToolbarVisible = false;
            // 
            // contextMenuStrip_Skin
            // 
            contextMenuStrip_Skin.ImageScalingSize = new Size(24, 24);
            contextMenuStrip_Skin.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_ReloadSkins });
            contextMenuStrip_Skin.Name = "contextMenuStrip1";
            contextMenuStrip_Skin.Size = new Size(225, 34);
            // 
            // toolStripMenuItem_ReloadSkins
            // 
            toolStripMenuItem_ReloadSkins.Name = "toolStripMenuItem_ReloadSkins";
            toolStripMenuItem_ReloadSkins.Size = new Size(224, 30);
            toolStripMenuItem_ReloadSkins.Text = "重新加载所选皮肤";
            toolStripMenuItem_ReloadSkins.Click += toolStripMenuItem_ReloadSkins_Click;
            // 
            // tabPage_Slot
            // 
            tabPage_Slot.BackColor = SystemColors.Control;
            tabPage_Slot.Controls.Add(propertyGrid_Slot);
            tabPage_Slot.Location = new Point(4, 4);
            tabPage_Slot.Margin = new Padding(0);
            tabPage_Slot.Name = "tabPage_Slot";
            tabPage_Slot.Size = new Size(364, 370);
            tabPage_Slot.TabIndex = 6;
            tabPage_Slot.Text = "槽位";
            // 
            // propertyGrid_Slot
            // 
            propertyGrid_Slot.Dock = DockStyle.Fill;
            propertyGrid_Slot.HelpVisible = false;
            propertyGrid_Slot.Location = new Point(0, 0);
            propertyGrid_Slot.Name = "propertyGrid_Slot";
            propertyGrid_Slot.PropertySort = PropertySort.Alphabetical;
            propertyGrid_Slot.Size = new Size(364, 370);
            propertyGrid_Slot.TabIndex = 2;
            propertyGrid_Slot.ToolbarVisible = false;
            // 
            // tabPage_Animation
            // 
            tabPage_Animation.BackColor = SystemColors.Control;
            tabPage_Animation.Controls.Add(propertyGrid_Animation);
            tabPage_Animation.Location = new Point(4, 4);
            tabPage_Animation.Margin = new Padding(0);
            tabPage_Animation.Name = "tabPage_Animation";
            tabPage_Animation.Size = new Size(364, 370);
            tabPage_Animation.TabIndex = 4;
            tabPage_Animation.Text = "动画";
            // 
            // propertyGrid_Animation
            // 
            propertyGrid_Animation.ContextMenuStrip = contextMenuStrip_Animation;
            propertyGrid_Animation.Dock = DockStyle.Fill;
            propertyGrid_Animation.HelpVisible = false;
            propertyGrid_Animation.Location = new Point(0, 0);
            propertyGrid_Animation.Name = "propertyGrid_Animation";
            propertyGrid_Animation.PropertySort = PropertySort.NoSort;
            propertyGrid_Animation.Size = new Size(364, 370);
            propertyGrid_Animation.TabIndex = 1;
            propertyGrid_Animation.ToolbarVisible = false;
            // 
            // contextMenuStrip_Animation
            // 
            contextMenuStrip_Animation.ImageScalingSize = new Size(24, 24);
            contextMenuStrip_Animation.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_AddAnimation, toolStripMenuItem_RemoveAnimation });
            contextMenuStrip_Animation.Name = "contextMenuStrip1";
            contextMenuStrip_Animation.Size = new Size(117, 64);
            contextMenuStrip_Animation.Opening += contextMenuStrip_Animation_Opening;
            // 
            // toolStripMenuItem_AddAnimation
            // 
            toolStripMenuItem_AddAnimation.Name = "toolStripMenuItem_AddAnimation";
            toolStripMenuItem_AddAnimation.Size = new Size(116, 30);
            toolStripMenuItem_AddAnimation.Text = "添加";
            toolStripMenuItem_AddAnimation.Click += toolStripMenuItem_AddAnimation_Click;
            // 
            // toolStripMenuItem_RemoveAnimation
            // 
            toolStripMenuItem_RemoveAnimation.Name = "toolStripMenuItem_RemoveAnimation";
            toolStripMenuItem_RemoveAnimation.Size = new Size(116, 30);
            toolStripMenuItem_RemoveAnimation.Text = "移除";
            toolStripMenuItem_RemoveAnimation.Click += toolStripMenuItem_RemoveAnimation_Click;
            // 
            // tabPage_Debug
            // 
            tabPage_Debug.BackColor = SystemColors.Control;
            tabPage_Debug.Controls.Add(propertyGrid_Debug);
            tabPage_Debug.Location = new Point(4, 4);
            tabPage_Debug.Name = "tabPage_Debug";
            tabPage_Debug.Size = new Size(364, 370);
            tabPage_Debug.TabIndex = 5;
            tabPage_Debug.Text = "调试";
            // 
            // propertyGrid_Debug
            // 
            propertyGrid_Debug.Dock = DockStyle.Fill;
            propertyGrid_Debug.HelpVisible = false;
            propertyGrid_Debug.Location = new Point(0, 0);
            propertyGrid_Debug.Name = "propertyGrid_Debug";
            propertyGrid_Debug.PropertySort = PropertySort.NoSort;
            propertyGrid_Debug.Size = new Size(364, 370);
            propertyGrid_Debug.TabIndex = 2;
            propertyGrid_Debug.ToolbarVisible = false;
            // 
            // SpineViewPropertyGrid
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabControl);
            Name = "SpineViewPropertyGrid";
            Size = new Size(372, 448);
            tabControl.ResumeLayout(false);
            tabPage_BaseInfo.ResumeLayout(false);
            tabPage_Render.ResumeLayout(false);
            tabPage_Transform.ResumeLayout(false);
            tabPage_Skin.ResumeLayout(false);
            contextMenuStrip_Skin.ResumeLayout(false);
            tabPage_Slot.ResumeLayout(false);
            tabPage_Animation.ResumeLayout(false);
            contextMenuStrip_Animation.ResumeLayout(false);
            tabPage_Debug.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabPage_BaseInfo;
        private TabPage tabPage_Render;
        private TabPage tabPage_Transform;
        private TabPage tabPage_Skin;
        private TabPage tabPage_Animation;
        private PropertyGrid propertyGrid_BaseInfo;
        private PropertyGrid propertyGrid_Render;
        private PropertyGrid propertyGrid_Transform;
        private PropertyGrid propertyGrid_Skin;
        private PropertyGrid propertyGrid_Animation;
        private ContextMenuStrip contextMenuStrip_Skin;
        private ContextMenuStrip contextMenuStrip_Animation;
        private ToolStripMenuItem toolStripMenuItem_ReloadSkins;
        private ToolStripMenuItem toolStripMenuItem_AddAnimation;
        private ToolStripMenuItem toolStripMenuItem_RemoveAnimation;
        private TabPage tabPage_Debug;
        private PropertyGrid propertyGrid_Debug;
        private TabPage tabPage_Slot;
        private PropertyGrid propertyGrid_Slot;
    }
}
