using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    class GifExporterWrapper(FFmpegVideoExporter exporter) : FFmpegVideoExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override GifExporter Exporter => (GifExporter)base.Exporter;

        /// <summary>
        /// 调色板最大颜色数量
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("调色板最大颜色数量"), Description("设置调色板使用的最大颜色数量, 越多则色彩保留程度越高")]
        public uint MaxColors { get => Exporter.MaxColors; set => Exporter.MaxColors = value; }

        /// <summary>
        /// 透明度阈值
        /// </summary>
        [Category("[3] 格式参数"), DisplayName("透明度阈值"), Description("小于该值的像素点会被认为是透明像素")]
        public byte AlphaThreshold { get => Exporter.AlphaThreshold; set => Exporter.AlphaThreshold = value; }
    }
}
