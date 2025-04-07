using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class FrameExporterWrapper(FrameExporter exporter) : ExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override FrameExporter Exporter => (FrameExporter)base.Exporter;

        /// <summary>
        /// 单帧画面格式
        /// </summary>
        [TypeConverter(typeof(ImageFormatConverter))]
        [Category("[1] 单帧画面"), DisplayName("图像格式")]
        public ImageFormat ImageFormat { get => Exporter.ImageFormat; set => Exporter.ImageFormat = value; }

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [Category("[1] 单帧画面"), DisplayName("文件名后缀"), Description("与图像格式匹配的文件名后缀")]
        public string Suffix { get => Exporter.ImageFormat.GetSuffix(); }

        /// <summary>
        /// DPI
        /// </summary>
        [TypeConverter(typeof(SizeFConverter))]
        [Category("[1] 单帧画面"), DisplayName("DPI"), Description("导出图像的每英寸像素数，用于调整图像的物理尺寸")]
        public SizeF DPI { get => Exporter.DPI; set => Exporter.DPI = value; }
    }
}
