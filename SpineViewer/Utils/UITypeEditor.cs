using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace SpineViewer.Utils
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

    ///// <summary>
    ///// skel 文件路径编辑器
    ///// </summary>
    //public class SkelFileNameEditor : FileNameEditor
    //{
    //    protected override void InitializeDialog(OpenFileDialog openFileDialog)
    //    {
    //        base.InitializeDialog(openFileDialog);
    //        openFileDialog.Title = "选择 skel 文件";
    //        openFileDialog.AddExtension = false;
    //        openFileDialog.Filter = "skel 文件 (*.skel; *.json)|*.skel;*.json|二进制文件 (*.skel)|*.skel|文本文件 (*.json)|*.json|所有文件 (*.*)|*.*";
    //    }
    //}

    ///// <summary>
    ///// atlas 文件路径编辑器
    ///// </summary>
    //public class AtlasFileNameEditor : FileNameEditor
    //{
    //    protected override void InitializeDialog(OpenFileDialog openFileDialog)
    //    {
    //        base.InitializeDialog(openFileDialog);
    //        openFileDialog.Title = "选择 atlas 文件";
    //        openFileDialog.AddExtension = false;
    //        openFileDialog.Filter = "atlas 文件 (*.atlas)|*.atlas|所有文件 (*.*)|*.*";
    //    }
    //}

    class SFMLColorEditor : UITypeEditor
    {
        public override bool GetPaintValueSupported(ITypeDescriptorContext? context) => true;

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is SFML.Graphics.Color color)
            {
                // 定义颜色和透明度的绘制区域
                var colorBox = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width / 2, e.Bounds.Height);
                var alphaBox = new Rectangle(e.Bounds.X + e.Bounds.Width / 2, e.Bounds.Y, e.Bounds.Width / 2, e.Bounds.Height);

                // 转换为 System.Drawing.Color
                var drawColor = Color.FromArgb(color.A, color.R, color.G, color.B);

                // 绘制纯颜色（RGB 部分）
                using (var brush = new SolidBrush(Color.FromArgb(color.R, color.G, color.B)))
                {
                    e.Graphics.FillRectangle(brush, colorBox);
                    e.Graphics.DrawRectangle(Pens.Black, colorBox);
                }

                // 绘制带透明度效果的颜色
                using (var checkerBrush = CreateTransparencyBrush())
                {
                    e.Graphics.FillRectangle(checkerBrush, alphaBox); // 背景棋盘格
                }
                using (var brush = new SolidBrush(drawColor))
                {
                    e.Graphics.FillRectangle(brush, alphaBox); // 叠加透明颜色
                    e.Graphics.DrawRectangle(Pens.Black, alphaBox);
                }
            }
            else
            {
                base.PaintValue(e);
            }
        }

        // 创建一个透明背景的棋盘格图案画刷
        private static TextureBrush CreateTransparencyBrush()
        {
            var bitmap = new Bitmap(8, 8);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                using (var grayBrush = new SolidBrush(Color.LightGray))
                {
                    g.FillRectangle(grayBrush, 0, 0, 4, 4);
                    g.FillRectangle(grayBrush, 4, 4, 4, 4);
                }
            }
            return new TextureBrush(bitmap);
        }
    }
}
