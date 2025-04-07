using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter
{
    /// <summary>
    /// 视频导出基类
    /// </summary>
    public abstract class VideoExporter : Exporter
    {
        /// <summary>
        /// 导出时长
        /// </summary>
        public float Duration { get => duration; set => duration = value < 0 ? -1 : value; }
        private float duration = -1;

        /// <summary>
        /// 帧率
        /// </summary>
        public float FPS { get; set; } = 60;

        public override string? Validate()
        {
            if (base.Validate() is string error)
                return error;
            if (IsExportSingle && Duration < 0)
                return "导出单个时导出时长不能为负数";
            return null;
        }

        /// <summary>
        /// 生成单个模型的帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(Spine.Spine spine, BackgroundWorker? worker = null)
        {
            // 独立导出时如果 Duration 小于 0 则使用所有轨道上动画时长最大值
            var duration = Duration;
            if (duration < 0) duration = spine.GetTrackIndices().Select(i => spine.GetAnimationDuration(spine.GetAnimation(i))).Max();

            float delta = 1f / FPS;
            int total = Math.Max(1, (int)(duration * FPS)); // 至少导出 1 帧

            worker?.ReportProgress(0, $"{spine.Name} 已处理 0/{total} 帧");
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    logger.Info("Export cancelled");
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
            // 导出单个时必须根据 Duration 决定导出时长
            float delta = 1f / FPS;
            int total = Math.Max(1, (int)(Duration * FPS)); // 至少导出 1 帧

            worker?.ReportProgress(0, $"已处理 0/{total} 帧");
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    logger.Info("Export cancelled");
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
            foreach (var spine in spines) spine.ResetAnimationsTime();
            base.Export(spines, worker);
        }
    }
}
