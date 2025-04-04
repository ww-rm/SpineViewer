using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SpineViewer.Exporter
{
    /// <summary>
    /// 导出参数基类
    /// </summary>
    public abstract class ExportArgs : ImplementationResolver<ExportArgs, ExportImplementationAttribute, ExportType>, IDisposable
    {
        /// <summary>
        /// 创建指定类型导出参数
        /// </summary>
        /// <param name="exportType">导出类型</param>
        /// <param name="resolution">分辨率</param>
        /// <param name="view">导出视图</param>
        /// <param name="renderSelectedOnly">仅渲染选中</param>
        /// <returns>返回与指定 <paramref name="exportType"/> 匹配的导出参数实例</returns>
        public static ExportArgs New(ExportType exportType, Size resolution, SFML.Graphics.View view, bool renderSelectedOnly)
            => New(exportType, [resolution, view, renderSelectedOnly]);

        public ExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly)
        {
            Resolution = resolution;
            View = view;
            RenderSelectedOnly = renderSelectedOnly;
        }

        ~ExportArgs() { Dispose(false); }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing) { View?.Dispose(); }

        /// <summary>
        /// 输出文件夹
        /// </summary>
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("[0] 导出"), DisplayName("输出文件夹"), Description("逐个导出时可以留空，将逐个导出到模型自身所在目录")]
        public string? OutputDir { get; set; } = null;

        /// <summary>
        /// 导出单个
        /// </summary>
        [Category("[0] 导出"), DisplayName("导出单个"), Description("是否将模型在同一个画面上导出单个文件，否则逐个导出模型")]
        public bool ExportSingle { get; set; } = false;

        /// <summary>
        /// 画面分辨率
        /// </summary>
        [TypeConverter(typeof(SizeConverter))]
        [Category("[0] 导出"), DisplayName("分辨率"), Description("画面的宽高像素大小，请在预览画面参数面板进行调整")]
        public Size Resolution { get; }

        /// <summary>
        /// 渲染视窗
        /// </summary>
        [Category("[0] 导出"), DisplayName("视图"), Description("画面的视图参数，请在预览画面参数面板进行调整")]
        public SFML.Graphics.View View { get; }

        /// <summary>
        /// 是否仅渲染选中
        /// </summary>
        [Category("[0] 导出"), DisplayName("仅渲染选中"), Description("是否仅导出选中的模型，请在预览画面参数面板进行调整")]
        public bool RenderSelectedOnly { get; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        [Editor(typeof(SFMLColorEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(SFMLColorConverter))]
        [Category("[0] 导出"), DisplayName("背景颜色"), Description("要使用的背景色, 格式为 #RRGGBBAA")]
        public SFML.Graphics.Color BackgroundColor 
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                var bcPma = value;
                var a = bcPma.A / 255f;
                bcPma.R = (byte)(bcPma.R * a);
                bcPma.G = (byte)(bcPma.G * a);
                bcPma.B = (byte)(bcPma.B * a);
                BackgroundColorPma = bcPma;
            }
        }
        private SFML.Graphics.Color backgroundColor = SFML.Graphics.Color.Transparent;

        /// <summary>
        /// 预乘后的背景颜色
        /// </summary>
        [Browsable(false)]
        public SFML.Graphics.Color BackgroundColorPma { get; private set; } = SFML.Graphics.Color.Transparent;

        /// <summary>
        /// 检查参数是否合法并规范化参数值, 否则返回用户错误原因
        /// </summary>
        public virtual string? Validate()
        {
            if (!string.IsNullOrWhiteSpace(OutputDir) && File.Exists(OutputDir))
                return "输出文件夹无效";
            if (!string.IsNullOrWhiteSpace(OutputDir) && !Directory.Exists(OutputDir))
                return $"文件夹 {OutputDir} 不存在";
            if (ExportSingle && string.IsNullOrWhiteSpace(OutputDir))
                return "导出单个时必须提供输出文件夹";

            OutputDir = string.IsNullOrWhiteSpace(OutputDir) ? null : Path.GetFullPath(OutputDir);
            return null;
        }
    }
}
