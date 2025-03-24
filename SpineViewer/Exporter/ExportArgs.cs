using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
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
        /// 输出文件夹
        /// </summary>
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("导出"), DisplayName("输出文件夹"), Description("逐个导出时可以留空，将逐个导出到模型自身所在目录")]
        public string? OutputDir { get; set; } = null;

        /// <summary>
        /// 逐个导出
        /// </summary>
        [Category("导出"), DisplayName("导出单个"), Description("是否将模型在同一个画面上导出单个文件，否则逐个导出模型")]
        public bool ExportSingle { get; set; } = false;

        /// <summary>
        /// 画面分辨率
        /// </summary>
        [ReadOnly(true)]
        [TypeConverter(typeof(SizeConverter))]
        [Category("导出"), DisplayName("分辨率"), Description("画面的宽高像素大小")]
        public required Size Resolution { get; init; }

        /// <summary>
        /// 渲染视窗
        /// </summary>
        [Browsable(false)]
        public required SFML.Graphics.View View { get; init; }

        /// <summary>
        /// 是否仅渲染选中
        /// </summary>
        [ReadOnly(true)]
        [Category("导出"), DisplayName("仅渲染选中"), Description("是否仅导出选中的模型")]
        public required bool RenderSelectedOnly { get; init; }

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

    /// <summary>
    /// 画面帧导出参数
    /// </summary>
    public class ExportFrameArgs : ExportArgs
    {
        /// <summary>
        /// 名称后缀
        /// </summary>
        [Category("画面帧"), DisplayName("名称后缀"), Description("逐个导出时必须提供该值，否则存在文件覆盖风险")]
        public string NameSuffix { get; set; } = "(preview)";

        /// <summary>
        /// 画面帧格式
        /// </summary>
        [TypeConverter(typeof(ImageFormatConverter))]
        [Category("画面帧"), DisplayName("图像格式")]
        public ImageFormat ImageFormat
        {
            get => imageFormat;
            set
            {
                if (value == ImageFormat.MemoryBmp) value = ImageFormat.Bmp;
                imageFormat = value;
            }
        }
        private ImageFormat imageFormat = ImageFormat.Png;

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("画面帧"), DisplayName("文件名后缀"), Description("与图像格式匹配的文件名后缀")]
        public string FileSuffix { get => imageFormat.GetSuffix(); }

        /// <summary>
        /// 四周填充像素值
        /// </summary>
        [TypeConverter(typeof(PaddingConverter))]
        [Category("画面帧"), DisplayName("四周填充像素值"), Description("在图内四周留出来的透明像素区域, 画面内容的可用范围是分辨率裁去填充区域")]
        public Padding Padding
        {
            get => padding;
            set
            {
                if (value.Left < 0) value.Left = 0;
                if (value.Right < 0) value.Right = 0;
                if (value.Top < 0) value.Top = 0;
                if (value.Bottom < 0) value.Bottom = 0;
                padding = value;
            }
        }
        private Padding padding = new(1);

        /// <summary>
        /// DPI
        /// </summary>
        [TypeConverter(typeof(SizeFConverter))]
        [Category("画面帧"), DisplayName("DPI"), Description("导出图像的每英寸像素数，用于调整图像的物理尺寸")]
        public SizeF DPI
        {
            get => dpi;
            set
            {
                if (value.Width <= 0) value.Width = 144;
                if (value.Height <= 0) value.Height = 144;
                dpi = value;
            }
        }
        private SizeF dpi = new(144, 144);

        public override string? Validate()
        {
            if (base.Validate() is string error)
                return error;
            if (string.IsNullOrEmpty(OutputDir) && string.IsNullOrEmpty(NameSuffix))
                return "输出文件夹和名称后缀不可同时为空，存在文件覆盖风险";
            return null;
        }
    }

    /// <summary>
    /// 视频导出参数基类
    /// </summary>
    public abstract class ExportVideoArgs:ExportArgs
    {

    }
}
