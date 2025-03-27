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

namespace SpineViewer.Exporter
{
    /// <summary>
    /// 导出参数基类
    /// </summary>
    public abstract class ExportArgs
    {
        /// <summary>
        /// 实现类缓存
        /// </summary>
        private static readonly Dictionary<ExportType, Type> ImplementationTypes = [];

        static ExportArgs()
        {
            var impTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ExportArgs).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in impTypes)
            {
                var attr = type.GetCustomAttribute<ExportImplementationAttribute>();
                if (attr is not null)
                {
                    if (ImplementationTypes.ContainsKey(attr.ExportType))
                        throw new InvalidOperationException($"Multiple implementations found: {attr.ExportType}");
                    ImplementationTypes[attr.ExportType] = type;
                }
            }
            Program.Logger.Debug("Find export args implementations: [{}]", string.Join(", ", ImplementationTypes.Keys));
        }

        /// <summary>
        /// 创建指定类型导出参数
        /// </summary>
        public static ExportArgs New(ExportType exportType, Size resolution, SFML.Graphics.View view, bool renderSelectedOnly)
        {
            if (!ImplementationTypes.TryGetValue(exportType, out var type))
            {
                throw new NotImplementedException($"Not implemented type: {exportType}");
            }
            return (ExportArgs)Activator.CreateInstance(type, resolution, view, renderSelectedOnly);
        }

        public ExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly)
        {
            Resolution = resolution;
            View = view;
            RenderSelectedOnly = renderSelectedOnly;
        }

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
        /// 背景颜色 TODO: 提供颜色编辑
        /// </summary>
        [Category("[0] 导出"), DisplayName("背景颜色"), Description("要使用的背景色, 格式为 #RRGGBBAA")]
        public SFML.Graphics.Color BackgroundColor { get; set; } = SFML.Graphics.Color.Transparent;

        /// <summary>
        /// 检查参数是否合法并规范化参数值, 否则返回用户错误原因
        /// </summary>
        public virtual string? Validate()
        {
            if (!string.IsNullOrEmpty(OutputDir) && File.Exists(OutputDir))
                return "输出文件夹无效";
            if (!string.IsNullOrEmpty(OutputDir) && !Directory.Exists(OutputDir))
                return $"文件夹 {OutputDir} 不存在";
            if (ExportSingle && string.IsNullOrEmpty(OutputDir))
                return "导出单个时必须提供输出文件夹";

            OutputDir = string.IsNullOrEmpty(OutputDir) ? null : Path.GetFullPath(OutputDir);
            return null;
        }
    }
}
