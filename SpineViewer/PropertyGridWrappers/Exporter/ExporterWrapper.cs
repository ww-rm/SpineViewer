using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class ExporterWrapper(SpineViewer.Exporter.Exporter exporter)
    {
        [Browsable(false)]
        public virtual SpineViewer.Exporter.Exporter Exporter { get; } = exporter;

        /// <summary>
        /// 输出文件夹
        /// </summary>
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("[0] 导出"), DisplayName("输出文件夹"), Description("逐个导出时可以留空，将逐个导出到模型自身所在目录")]
        public string? OutputDir { get => Exporter.OutputDir; set => Exporter.OutputDir = value; }

        /// <summary>
        /// 导出单个
        /// </summary>
        [Category("[0] 导出"), DisplayName("导出单个"), Description("是否将模型在同一个画面上导出单个文件，否则逐个导出模型")]
        public bool IsExportSingle { get => Exporter.IsExportSingle; set => Exporter.IsExportSingle = value; }

        /// <summary>
        /// 画面分辨率
        /// </summary>
        [TypeConverter(typeof(SizeConverter))]
        [Category("[0] 导出"), DisplayName("分辨率"), Description("画面的宽高像素大小，请在预览画面参数面板进行调整")]
        public Size Resolution { get => Exporter.Resolution; }

        /// <summary>
        /// 渲染视窗
        /// </summary>
        [Category("[0] 导出"), DisplayName("视图"), Description("画面的视图参数，请在预览画面参数面板进行调整")]
        public SFML.Graphics.View View { get => Exporter.View; }

        /// <summary>
        /// 是否仅渲染选中
        /// </summary>
        [Category("[0] 导出"), DisplayName("仅渲染选中"), Description("是否仅导出选中的模型，请在预览画面参数面板进行调整")]
        public bool RenderSelectedOnly { get => Exporter.RenderSelectedOnly; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        [Editor(typeof(SFMLColorEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(SFMLColorConverter))]
        [Category("[0] 导出"), DisplayName("背景颜色"), Description("要使用的背景色, 格式为 #RRGGBBAA")]
        public SFML.Graphics.Color BackgroundColor { get => Exporter.BackgroundColor; set => Exporter.BackgroundColor = value; }
    }
}
