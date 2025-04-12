using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpineViewer.Utils;
using SpineViewer.Spine.SpineView;

namespace SpineViewer.Controls
{
    public partial class SpineViewPropertyGrid : UserControl
    {
        public SpineViewPropertyGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置选中的对象列表, 可以赋值 null 来清空选中, 行为与 PropertyGrid.SelectedObjects 类似
        /// </summary>
        public SpineObjectProperty[] SelectedSpines
        {
            get => selectedSpines ?? [];
            set
            {
                if (value is null || value.Length <= 0)
                {
                    selectedSpines = null;
                    propertyGrid_BaseInfo.SelectedObject = null;
                    propertyGrid_Render.SelectedObject = null;
                    propertyGrid_Transform.SelectedObject = null;
                    propertyGrid_Skin.SelectedObject = null;
                    propertyGrid_Animation.SelectedObject = null;
                    propertyGrid_Debug.SelectedObject = null;
                }
                else
                {
                    selectedSpines = value;
                    propertyGrid_BaseInfo.SelectedObjects = value.Select(e => e.BaseInfo).ToArray();
                    propertyGrid_Render.SelectedObjects = value.Select(e => e.Render).ToArray();
                    propertyGrid_Transform.SelectedObjects = value.Select(e => e.Transform).ToArray();
                    propertyGrid_Skin.SelectedObjects = value.Select(e => e.Skin).ToArray();
                    propertyGrid_Animation.SelectedObjects = value.Select(e => e.Animation).ToArray();
                    propertyGrid_Debug.SelectedObjects = value.Select(e => e.Debug).ToArray();
                }
            }
        }
        private SpineObjectProperty[]? selectedSpines = null;

        private void contextMenuStrip_Skin_Opening(object sender, CancelEventArgs e)
        {
            if (selectedSpines?.Length == 1)
            {
                toolStripMenuItem_AddSkin.Enabled = true;
                toolStripMenuItem_RemoveSkin.Enabled = propertyGrid_Skin.SelectedGridItem.Value is SkinNameProperty;
            }
            else
            {
                toolStripMenuItem_AddSkin.Enabled = false;
                toolStripMenuItem_RemoveSkin.Enabled = false;
            }
        }

        private void contextMenuStrip_Animation_Opening(object sender, CancelEventArgs e)
        {
            if (selectedSpines?.Length == 1)
            {
                toolStripMenuItem_AddAnimation.Enabled = true;
                toolStripMenuItem_RemoveAnimation.Enabled = propertyGrid_Animation.SelectedGridItem.Value is TrackAnimationProperty;
            }
            else
            {
                toolStripMenuItem_AddAnimation.Enabled = false;
                toolStripMenuItem_RemoveAnimation.Enabled = false;
            }
        }

        private void toolStripMenuItem_AddSkin_Click(object sender, EventArgs e)
        {
            if (selectedSpines?.Length != 1) return;

            var spine = selectedSpines[0].Skin.Spine;

            if (spine.SkinNames.Count <= 0)
            {
                MessagePopup.Info("没有可用的皮肤");
                return;
            }

            spine.LoadSkin(spine.SkinNames[0]);
            propertyGrid_Skin.Refresh();
        }

        private void toolStripMenuItem_RemoveSkin_Click(object sender, EventArgs e)
        {
            if (selectedSpines?.Length != 1) return;

            if (propertyGrid_Skin.SelectedGridItem.Value is SkinNameProperty wrapper)
            {
                selectedSpines[0].Skin.Spine.UnloadSkin(wrapper.Index);
                propertyGrid_Skin.Refresh();
            }
        }

        private void toolStripMenuItem_AddAnimation_Click(object sender, EventArgs e)
        {
            if (selectedSpines?.Length != 1) return;

            var spine = selectedSpines[0].Animation.Spine;
            spine.SetAnimation(spine.GetTrackIndices().Max() + 1, spine.AnimationNames[0]);
            propertyGrid_Animation.Refresh();
        }

        private void toolStripMenuItem_RemoveAnimation_Click(object sender, EventArgs e)
        {
            if (selectedSpines?.Length != 1) return;

            if (propertyGrid_Animation.SelectedGridItem.Value is TrackAnimationProperty wrapper)
            {
                selectedSpines[0].Animation.Spine.ClearTrack(wrapper.Index);
                propertyGrid_Animation.Refresh();
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            propertyGrid_BaseInfo.Refresh();
            propertyGrid_Render.Refresh();
            propertyGrid_Transform.Refresh();
            propertyGrid_Skin.Refresh();
            propertyGrid_Animation.Refresh();
            propertyGrid_Debug.Refresh();
        }
    }
}
