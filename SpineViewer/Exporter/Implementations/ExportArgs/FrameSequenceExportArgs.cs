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
    /// 帧序列导出参数
    /// </summary>
    [ExportImplementation(ExportType.FrameSequence)]
    public class FrameSequenceExportArgs : VideoExportArgs
    {
        public FrameSequenceExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly) { }

        /// <summary>
        /// 文件名后缀
        /// </summary>
        [TypeConverter(typeof(SFMLImageFileSuffixConverter))]
        [Category("[2] 帧序列参数"), DisplayName("文件名后缀"), Description("帧文件的后缀，同时决定帧图像格式")]
        public string FileSuffix { get; set; } = ".png";
    }
}
