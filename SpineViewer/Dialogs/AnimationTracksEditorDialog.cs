using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Dialogs
{
    public partial class AnimationTracksEditorDialog : Form
    {
        private readonly Spine.Spine spine;
        public AnimationTracksEditorDialog(Spine.Spine spine)
        {
            InitializeComponent();
            this.spine = spine;
            propertyGrid_AnimationTracks.SelectedObject = spine.AnimationTracks;
        }

        private void button_Add_Click(object sender, EventArgs e)
        {
            spine.SetAnimation(spine.GetTrackIndices().Max() + 1, spine.AnimationNames[0]);
            propertyGrid_AnimationTracks.Refresh();
        }

        private void button_Delete_Click(object sender, EventArgs e)
        {
            if (propertyGrid_AnimationTracks.SelectedGridItem?.Value is TrackWrapper tr)
            {
                if (tr.Index == 0)
                    MessageBox.Info("必须保留轨道 0");
                else
                    spine.ClearTrack(tr.Index);
            }
            propertyGrid_AnimationTracks.Refresh();
            propertyGrid_AnimationTracks.SelectedGridItem = propertyGrid_AnimationTracks.SelectedGridItem?.Parent?.GridItems?.Cast<GridItem>().Last();
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
