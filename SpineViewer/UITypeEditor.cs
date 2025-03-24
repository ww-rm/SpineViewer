using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace SpineViewer
{
    /// <summary>
    /// 使用 FolderBrowserDialog 的文件夹路径编辑器
    /// </summary>
    public class FolderNameEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
        {
            // 指定编辑风格为 Modal 对话框, 提供右边用来点击的按钮
            return UITypeEditorEditStyle.Modal;
        }

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            // 重写 EditValue 方法，提供自定义的文件夹选择对话框逻辑
            using var dialog = new FolderBrowserDialog();

            // 如果当前值为有效路径，则设置为初始选中路径
            if (value is string currentPath && Directory.Exists(currentPath))
                dialog.SelectedPath = currentPath;

            if (dialog.ShowDialog() == DialogResult.OK)
                value = dialog.SelectedPath;

            return value;
        }
    }
}
