using SpineViewer.Exporter.Implementations.ExportArgs;
using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter.Implementations.Exporter
{
    /// <summary>
    /// 视频导出基类
    /// </summary>
    public abstract class VideoExporter : SpineViewer.Exporter.Exporter
    {
        public VideoExporter(VideoExportArgs exportArgs) : base(exportArgs) { }

        /// <summary>
        /// 生成单个模型的帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(Spine.Spine spine, BackgroundWorker? worker = null)
        {
            var args = (VideoExportArgs)ExportArgs;

            // 独立导出时如果 args.Duration 小于 0 则使用自己的动画时长
            var duration = args.Duration;
            if (duration < 0) duration = spine.GetAnimationDuration(spine.CurrentAnimation);

            float delta = 1f / args.FPS;
            int total = Math.Max(1, (int)(duration * args.FPS)); // 至少导出 1 帧

            worker?.ReportProgress(0, $"{spine.Name} 已处理 0/{total} 帧");
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    Program.Logger.Info("Export cancelled");
                    break;
                }

                var frame = GetFrame(spine);
                spine.Update(delta);
                worker?.ReportProgress((int)((i + 1) * 100.0) / total, $"{spine.Name} 已处理 {i + 1}/{total} 帧");
                yield return frame;
            }
        }

        /// <summary>
        /// 生成多个模型的帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            // 导出单个时必须根据 args.Duration 决定导出时长
            var args = (VideoExportArgs)ExportArgs;
            float delta = 1f / args.FPS;
            int total = Math.Max(1, (int)(args.Duration * args.FPS)); // 至少导出 1 帧

            worker?.ReportProgress(0, $"已处理 0/{total} 帧");
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    Program.Logger.Info("Export cancelled");
                    break;
                }

                var frame = GetFrame(spinesToRender);
                foreach (var spine in spinesToRender) spine.Update(delta);
                worker?.ReportProgress((int)((i + 1) * 100.0) / total, $"已处理 {i + 1}/{total} 帧");
                yield return frame;
            }
        }

        public override void Export(Spine.Spine[] spines, BackgroundWorker? worker = null)
        {
            // 导出视频格式需要把模型时间都重置到 0
            foreach (var spine in spines) spine.CurrentAnimation = spine.CurrentAnimation;
            base.Export(spines, worker);
        }
    }
}
