using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class FrameSequenceExporterWrapper(VideoExporter exporter) : VideoExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override FrameSequenceExporter Exporter => (FrameSequenceExporter)base.Exporter;

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [TypeConverter(typeof(StringEnumConverter)), StringEnumConverter.StandardValues(".png", ".jpg", ".tga", ".bmp")]
        [Category("[2] 帧序列参数"), DisplayName("文件名后缀"), Description("帧文件的后缀，同时决定帧图像格式")]
        public string Suffix { get => Exporter.Suffix; set => Exporter.Suffix = value; }
    }
}
