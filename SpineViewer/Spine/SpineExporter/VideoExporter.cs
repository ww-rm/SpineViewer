using SpineViewer.Spine;
using SpineViewer.Utils.Localize;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.SpineExporter
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

        /// <summary>
        /// 是否保留最后一帧
        /// </summary>
        public bool KeepLast { get; set; } = true;

        public override string? Validate()
        {
            if (base.Validate() is string error)
                return error;
            if (IsExportSingle && Duration < 0)
                return Properties.Resources.negativeDuration;
            return null;
        }

        /// <summary>
        /// 生成单个模型的帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(SpineObject spine, BackgroundWorker? worker = null)
        {
            // 独立导出时如果 Duration 小于 0 则使用所有轨道上动画时长最大值
            var duration = Duration;
            if (duration < 0) duration = spine.GetTrackIndices().Select(i => spine.GetAnimationDuration(spine.GetAnimation(i))).Max();

            float delta = 1f / FPS;
            int total = (int)(duration * FPS); // 完整帧的数量

            float deltaFinal = duration - delta * total; // 最后一帧时长
            int final = KeepLast && deltaFinal > 1e-3 ? 1 : 0;

            int frameCount = 1 + total + final; // 所有帧的数量 = 起始帧 + 完整帧 + 最后一帧

            worker?.ReportProgress(0, $"{spine.Name} {Properties.Resources.process} 0/{frameCount} {Properties.Resources.frame}");

            // 导出首帧
            var firstFrame = GetFrame(spine);
            worker?.ReportProgress(1 * 100 / frameCount, $"{spine.Name} {Properties.Resources.process} 1/{frameCount} {Properties.Resources.frame}");
            yield return firstFrame;

            // 导出完整帧
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    logger.Info("Export cancelled");
                    break;
                }

                spine.Update(delta);
                var frame = GetFrame(spine);
                worker?.ReportProgress((1 + i + 1) * 100 / frameCount, $"{spine.Name} {Properties.Resources.process} {1 + i + 1}/{frameCount} {Properties.Resources.frame}");
                yield return frame;
            }

            // 导出最后一帧
            if (final > 0)
            {
                spine.Update(deltaFinal);
                var finalFrame = GetFrame(spine);
                worker?.ReportProgress(100, $"{spine.Name} {Properties.Resources.process} {frameCount}/{frameCount} {Properties.Resources.frame}");
                yield return finalFrame;
            }
        }

        /// <summary>
        /// 生成多个模型的帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(SpineObject[] spinesToRender, BackgroundWorker? worker = null)
        {
            // 导出单个时必须根据 Duration 决定导出时长
            var duration = Duration;

            float delta = 1f / FPS;
            int total = (int)(duration * FPS); // 完整帧的数量

            float deltaFinal = duration - delta * total; // 最后一帧时长
            int final = KeepLast && deltaFinal > 1e-3 ? 1 : 0;

            int frameCount = 1 + total + final; // 所有帧的数量 = 起始帧 + 完整帧 + 最后一帧

            worker?.ReportProgress(0, $"{Properties.Resources.process} 0/{frameCount} {Properties.Resources.frame}");

            // 导出首帧
            var firstFrame = GetFrame(spinesToRender);
            worker?.ReportProgress(1 * 100 / frameCount, $"{Properties.Resources.process} 1/{frameCount} {Properties.Resources.frame}");
            yield return firstFrame;

            // 导出完整帧
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    logger.Info("Export cancelled");
                    break;
                }

                foreach (var spine in spinesToRender) spine.Update(delta);
                var frame = GetFrame(spinesToRender);
                worker?.ReportProgress((1 + i + 1) * 100 / frameCount, $"{Properties.Resources.process} {1 + i + 1}/{frameCount} {Properties.Resources.frame}");
                yield return frame;
            }

            // 导出最后一帧
            if (final > 0)
            {
                foreach (var spine in spinesToRender) spine.Update(delta);
                var finalFrame = GetFrame(spinesToRender);
                worker?.ReportProgress(100, $"{Properties.Resources.process} {frameCount}/{frameCount} {Properties.Resources.frame}");
                yield return finalFrame;
            }
        }

        public override void Export(SpineObject[] spines, BackgroundWorker? worker = null)
        {
            // 导出视频格式需要把模型时间都重置到 0
            foreach (var spine in spines) spine.ResetAnimationsTime();
            base.Export(spines, worker);
        }
    }

    public class VideoExporterProperty(VideoExporter exporter) : ExporterProperty(exporter)
    {
        [Browsable(false)]
        public override VideoExporter Exporter => (VideoExporter)base.Exporter;

		/// <summary>
		/// 导出时长
		/// </summary>
		[LocalizedCategory(typeof(Properties.Resources), "categoryVideoParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "duration")]
		[LocalizedDescription(typeof(Properties.Resources), "descDuration")]
        public float Duration { get => Exporter.Duration; set => Exporter.Duration = value; }

		/// <summary>
		/// 帧率
		/// </summary>
		[LocalizedCategory(typeof(Properties.Resources), "categoryVideoParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayFPS")]
		[LocalizedDescription(typeof(Properties.Resources), "descFPS")]
		public float FPS { get => Exporter.FPS; set => Exporter.FPS = value; }

		/// <summary>
		/// 保留最后一帧
		/// </summary>
		[LocalizedCategory(typeof(Properties.Resources), "categoryVideoParameters")]
		[LocalizedDisplayName(typeof(Properties.Resources), "displayKeepLastFrame")]
		[LocalizedDescription(typeof(Properties.Resources), "descKeepLastFrame")]
		public bool KeepLast { get => Exporter.KeepLast; set => Exporter.KeepLast = value; }
    }
}
