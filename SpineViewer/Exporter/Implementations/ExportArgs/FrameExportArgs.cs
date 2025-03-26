using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.ExportArgs
{
    /// <summary>
    /// 单帧画面导出参数
    /// </summary>
    [ExportImplementation(ExportType.Frame)]
    public class FrameExportArgs : SpineViewer.Exporter.ExportArgs
    {
        public FrameExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly) { }

        /// <summary>
        /// 单帧画面格式
        /// </summary>
        [TypeConverter(typeof(ImageFormatConverter))]
        [Category("[1] 单帧画面"), DisplayName("图像格式")]
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
        [Category("[1] 单帧画面"), DisplayName("文件名后缀"), Description("与图像格式匹配的文件名后缀")]
        public string FileSuffix { get => imageFormat.GetSuffix(); }

        /// <summary>
        /// DPI
        /// </summary>
        [TypeConverter(typeof(SizeFConverter))]
        [Category("[1] 单帧画面"), DisplayName("DPI"), Description("导出图像的每英寸像素数，用于调整图像的物理尺寸")]
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
    }
}
