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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpineViewPropertyGrid));
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
			resources.ApplyResources(tabControl, "tabControl");
			tabControl.Controls.Add(tabPage_BaseInfo);
			tabControl.Controls.Add(tabPage_Render);
			tabControl.Controls.Add(tabPage_Transform);
			tabControl.Controls.Add(tabPage_Skin);
			tabControl.Controls.Add(tabPage_Slot);
			tabControl.Controls.Add(tabPage_Animation);
			tabControl.Controls.Add(tabPage_Debug);
			tabControl.Multiline = true;
			tabControl.Name = "tabControl";
			tabControl.SelectedIndex = 0;
			tabControl.SizeMode = TabSizeMode.FillToRight;
			// 
			// tabPage_BaseInfo
			// 
			resources.ApplyResources(tabPage_BaseInfo, "tabPage_BaseInfo");
			tabPage_BaseInfo.BackColor = SystemColors.Control;
			tabPage_BaseInfo.Controls.Add(propertyGrid_BaseInfo);
			tabPage_BaseInfo.Name = "tabPage_BaseInfo";
			// 
			// propertyGrid_BaseInfo
			// 
			resources.ApplyResources(propertyGrid_BaseInfo, "propertyGrid_BaseInfo");
			propertyGrid_BaseInfo.Name = "propertyGrid_BaseInfo";
			propertyGrid_BaseInfo.PropertySort = PropertySort.Alphabetical;
			propertyGrid_BaseInfo.ToolbarVisible = false;
			// 
			// tabPage_Render
			// 
			resources.ApplyResources(tabPage_Render, "tabPage_Render");
			tabPage_Render.BackColor = SystemColors.Control;
			tabPage_Render.Controls.Add(propertyGrid_Render);
			tabPage_Render.Name = "tabPage_Render";
			// 
			// propertyGrid_Render
			// 
			resources.ApplyResources(propertyGrid_Render, "propertyGrid_Render");
			propertyGrid_Render.Name = "propertyGrid_Render";
			propertyGrid_Render.PropertySort = PropertySort.Alphabetical;
			propertyGrid_Render.ToolbarVisible = false;
			// 
			// tabPage_Transform
			// 
			resources.ApplyResources(tabPage_Transform, "tabPage_Transform");
			tabPage_Transform.BackColor = SystemColors.Control;
			tabPage_Transform.Controls.Add(propertyGrid_Transform);
			tabPage_Transform.Name = "tabPage_Transform";
			// 
			// propertyGrid_Transform
			// 
			resources.ApplyResources(propertyGrid_Transform, "propertyGrid_Transform");
			propertyGrid_Transform.Name = "propertyGrid_Transform";
			propertyGrid_Transform.PropertySort = PropertySort.Alphabetical;
			propertyGrid_Transform.ToolbarVisible = false;
			// 
			// tabPage_Skin
			// 
			resources.ApplyResources(tabPage_Skin, "tabPage_Skin");
			tabPage_Skin.BackColor = SystemColors.Control;
			tabPage_Skin.Controls.Add(propertyGrid_Skin);
			tabPage_Skin.Name = "tabPage_Skin";
			// 
			// propertyGrid_Skin
			// 
			resources.ApplyResources(propertyGrid_Skin, "propertyGrid_Skin");
			propertyGrid_Skin.ContextMenuStrip = contextMenuStrip_Skin;
			propertyGrid_Skin.Name = "propertyGrid_Skin";
			propertyGrid_Skin.PropertySort = PropertySort.NoSort;
			propertyGrid_Skin.ToolbarVisible = false;
			// 
			// contextMenuStrip_Skin
			// 
			resources.ApplyResources(contextMenuStrip_Skin, "contextMenuStrip_Skin");
			contextMenuStrip_Skin.ImageScalingSize = new Size(24, 24);
			contextMenuStrip_Skin.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_ReloadSkins });
			contextMenuStrip_Skin.Name = "contextMenuStrip1";
			// 
			// toolStripMenuItem_ReloadSkins
			// 
			resources.ApplyResources(toolStripMenuItem_ReloadSkins, "toolStripMenuItem_ReloadSkins");
			toolStripMenuItem_ReloadSkins.Name = "toolStripMenuItem_ReloadSkins";
			toolStripMenuItem_ReloadSkins.Click += toolStripMenuItem_ReloadSkins_Click;
			// 
			// tabPage_Slot
			// 
			resources.ApplyResources(tabPage_Slot, "tabPage_Slot");
			tabPage_Slot.BackColor = SystemColors.Control;
			tabPage_Slot.Controls.Add(propertyGrid_Slot);
			tabPage_Slot.Name = "tabPage_Slot";
			// 
			// propertyGrid_Slot
			// 
			resources.ApplyResources(propertyGrid_Slot, "propertyGrid_Slot");
			propertyGrid_Slot.Name = "propertyGrid_Slot";
			propertyGrid_Slot.PropertySort = PropertySort.Alphabetical;
			propertyGrid_Slot.ToolbarVisible = false;
			// 
			// tabPage_Animation
			// 
			resources.ApplyResources(tabPage_Animation, "tabPage_Animation");
			tabPage_Animation.BackColor = SystemColors.Control;
			tabPage_Animation.Controls.Add(propertyGrid_Animation);
			tabPage_Animation.Name = "tabPage_Animation";
			// 
			// propertyGrid_Animation
			// 
			resources.ApplyResources(propertyGrid_Animation, "propertyGrid_Animation");
			propertyGrid_Animation.ContextMenuStrip = contextMenuStrip_Animation;
			propertyGrid_Animation.Name = "propertyGrid_Animation";
			propertyGrid_Animation.PropertySort = PropertySort.NoSort;
			propertyGrid_Animation.ToolbarVisible = false;
			// 
			// contextMenuStrip_Animation
			// 
			resources.ApplyResources(contextMenuStrip_Animation, "contextMenuStrip_Animation");
			contextMenuStrip_Animation.ImageScalingSize = new Size(24, 24);
			contextMenuStrip_Animation.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_AddAnimation, toolStripMenuItem_RemoveAnimation });
			contextMenuStrip_Animation.Name = "contextMenuStrip1";
			contextMenuStrip_Animation.Opening += contextMenuStrip_Animation_Opening;
			// 
			// toolStripMenuItem_AddAnimation
			// 
			resources.ApplyResources(toolStripMenuItem_AddAnimation, "toolStripMenuItem_AddAnimation");
			toolStripMenuItem_AddAnimation.Name = "toolStripMenuItem_AddAnimation";
			toolStripMenuItem_AddAnimation.Click += toolStripMenuItem_AddAnimation_Click;
			// 
			// toolStripMenuItem_RemoveAnimation
			// 
			resources.ApplyResources(toolStripMenuItem_RemoveAnimation, "toolStripMenuItem_RemoveAnimation");
			toolStripMenuItem_RemoveAnimation.Name = "toolStripMenuItem_RemoveAnimation";
			toolStripMenuItem_RemoveAnimation.Click += toolStripMenuItem_RemoveAnimation_Click;
			// 
			// tabPage_Debug
			// 
			resources.ApplyResources(tabPage_Debug, "tabPage_Debug");
			tabPage_Debug.BackColor = SystemColors.Control;
			tabPage_Debug.Controls.Add(propertyGrid_Debug);
			tabPage_Debug.Name = "tabPage_Debug";
			// 
			// propertyGrid_Debug
			// 
			resources.ApplyResources(propertyGrid_Debug, "propertyGrid_Debug");
			propertyGrid_Debug.Name = "propertyGrid_Debug";
			propertyGrid_Debug.PropertySort = PropertySort.NoSort;
			propertyGrid_Debug.ToolbarVisible = false;
			// 
			// SpineViewPropertyGrid
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(tabControl);
			Name = "SpineViewPropertyGrid";
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
