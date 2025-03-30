using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.ExportArgs
{
    /// <summary>
    /// MP4 导出参数
    /// </summary>
    [ExportImplementation(ExportType.MP4)]
    public class Mp4ExportArgs : VideoExportArgs
    {
        public Mp4ExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly) 
        {
            // MP4 默认用绿幕
            BackgroundColor = new(0, 255, 0, 0);
        }

        /// <summary>
        /// CRF
        /// </summary>
        [Category("[2] MP4 参数"), DisplayName("CRF"), Description("Constant Rate Factor, 取值范围 0-63, 建议范围 18-28, 默认取值 23, 数值越小则输出质量越高")]
        public int CRF { get => crf; set => crf = Math.Clamp(value, 0, 63); }
        private int crf = 23;

        /// <summary>
        /// 编码器 TODO: 增加其他编码器
        /// </summary>
        [Category("[2] MP4 参数"), DisplayName("编码器"), Description("要使用的编码器")]
        public string Codec { get => "libx264"; }
    }
}
