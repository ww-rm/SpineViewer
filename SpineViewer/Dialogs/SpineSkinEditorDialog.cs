using SpineViewer.PropertyGridWrappers.Spine;
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
    public partial class SpineSkinEditorDialog : Form
    {
        private readonly Spine.Spine spine;
        public SpineSkinEditorDialog(Spine.Spine spine)
        {
            InitializeComponent();
            this.spine = spine;
            propertyGrid_SkinManager.SelectedObject = new SpineSkinWrapper(spine); // TODO: 去掉对话框
        }

        private void button_Add_Click(object sender, EventArgs e)
        {
            if (spine.SkinNames.Count <= 0)
            {
                MessageBox.Info($"{spine.Name} 没有可用的皮肤");
                return;
            }
            spine.LoadSkin(spine.SkinNames[0]);
            propertyGrid_SkinManager.Refresh();
        }

        private void button_Delete_Click(object sender, EventArgs e)
        {
            if (propertyGrid_SkinManager.SelectedGridItem?.Value is SkinWrapper sk)
                spine.UnloadSkin(sk.Index);
            propertyGrid_SkinManager.Refresh();

            if (propertyGrid_SkinManager.SelectedGridItem?.Parent?.GridItems?.Cast<GridItem>().Last() is GridItem gt)
                propertyGrid_SkinManager.SelectedGridItem = gt;
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
