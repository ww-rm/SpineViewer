using SpineViewer.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace SpineViewer.Spine
{
    /// <summary>
    /// skel 文件路径编辑器
    /// </summary>
    public class SkelFileNameEditor : FileNameEditor
    {
        protected override void InitializeDialog(OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);
            openFileDialog.Title = "选择 skel 文件";
            openFileDialog.AddExtension = false;
            openFileDialog.Filter = "skel 文件 (*.skel; *.json)|*.skel;*.json|二进制文件 (*.skel)|*.skel|文本文件 (*.json)|*.json|所有文件 (*.*)|*.*";
        }
    }

    /// <summary>
    /// atlas 文件路径编辑器
    /// </summary>
    public class AtlasFileNameEditor : FileNameEditor
    {
        protected override void InitializeDialog(OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);
            openFileDialog.Title = "选择 atlas 文件";
            openFileDialog.AddExtension = false;
            openFileDialog.Filter = "atlas 文件 (*.atlas)|*.atlas|所有文件 (*.*)|*.*";
        }
    }

    /// <summary>
    /// 多轨道动画编辑器
    /// </summary>
    public class AnimationTracksEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) => UITypeEditorEditStyle.Modal;

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            if (provider == null || context == null || context.Instance is not Spine)
                return value;

            IWindowsFormsEditorService editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (editorService == null)
                return value;

            using (var dialog = new AnimationTracksEditorDialog((Spine)context.Instance))
                editorService.ShowDialog(dialog);

            TypeDescriptor.Refresh(context.Instance);
            return value;
        }
    }
}
