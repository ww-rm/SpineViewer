using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.ExportArgs
{
    /// <summary>
    /// 视频导出参数基类
    /// </summary>
    public abstract class VideoExportArgs : SpineViewer.Exporter.ExportArgs
    {
        public VideoExportArgs(Size resolution, SFML.Graphics.View view, bool renderSelectedOnly) : base(resolution, view, renderSelectedOnly) { }

        /// <summary>
        /// 导出时长
        /// </summary>
        [Category("视频参数"), DisplayName("时长"), Description("可以从模型列表查看动画时长")]
        public float Duration { get => duration; set => duration = Math.Max(0, value); }
        private float duration = 1;

        /// <summary>
        /// 帧率
        /// </summary>
        [Category("视频参数"), DisplayName("帧率"), Description("每秒画面数")]
        public float FPS { get; set; } = 60;
    }
}
