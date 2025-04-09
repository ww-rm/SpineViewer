using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Exporter
{
    public class VideoExporterWrapper(VideoExporter exporter) : ExporterWrapper(exporter)
    {
        [Browsable(false)]
        public override VideoExporter Exporter => (VideoExporter)base.Exporter;

        /// <summary>
        /// 导出时长
        /// </summary>
        [Category("[1] 视频参数"), DisplayName("时长"), Description("可以从模型列表查看动画时长, 如果小于 0, 则在逐个导出时每个模型使用各自的所有轨道动画时长最大值")]
        public float Duration { get => Exporter.Duration; set => Exporter.Duration = value; }

        /// <summary>
        /// 帧率
        /// </summary>
        [Category("[1] 视频参数"), DisplayName("帧率"), Description("每秒画面数")]
        public float FPS { get => Exporter.FPS; set => Exporter.FPS = value; }

        /// <summary>
        /// 保留最后一帧
        /// </summary>
        [Category("[1] 视频参数"), DisplayName("保留最后一帧"), Description("当设置保留最后一帧时, 动图会更为连贯, 但是帧数可能比预期帧数多 1")]
        public bool KeepLast { get => Exporter.KeepLast; set => Exporter.KeepLast = value; }
    }
}
