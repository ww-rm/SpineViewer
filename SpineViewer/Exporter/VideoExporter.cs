﻿using SpineViewer.Spine;
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

        /// <summary>
        /// 是否保留最后一帧
        /// </summary>
        public bool KeepLast { get; set; } = true;

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
            int total = (int)(duration * FPS); // 完整帧的数量

            float deltaFinal = duration - delta * total; // 最后一帧时长
            int final = (KeepLast && (deltaFinal > 1e-3)) ? 1 : 0;

            int frameCount = 1 + total + final; // 所有帧的数量 = 起始帧 + 完整帧 + 最后一帧

            worker?.ReportProgress(0, $"{spine.Name} 已处理 0/{frameCount} 帧");

            // 导出首帧
            var firstFrame = GetFrame(spine);
            worker?.ReportProgress(1 * 100 / frameCount, $"{spine.Name} 已处理 1/{frameCount} 帧");
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
                worker?.ReportProgress((1 + i + 1) * 100 / frameCount, $"{spine.Name} 已处理 {1 + i + 1}/{frameCount} 帧");
                yield return frame;
            }

            // 导出最后一帧
            if (final > 0)
            {
                spine.Update(deltaFinal);
                var finalFrame = GetFrame(spine);
                worker?.ReportProgress(100, $"{spine.Name} 已处理 {frameCount}/{frameCount} 帧");
                yield return finalFrame;
            }
        }

        /// <summary>
        /// 生成多个模型的帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null)
        {
            // 导出单个时必须根据 Duration 决定导出时长
            var duration = Duration;

            float delta = 1f / FPS;
            int total = (int)(duration * FPS); // 完整帧的数量

            float deltaFinal = duration - delta * total; // 最后一帧时长
            int final = (KeepLast && (deltaFinal > 1e-3)) ? 1 : 0;

            int frameCount = 1 + total + final; // 所有帧的数量 = 起始帧 + 完整帧 + 最后一帧

            worker?.ReportProgress(0, $"已处理 0/{frameCount} 帧");

            // 导出首帧
            var firstFrame = GetFrame(spinesToRender);
            worker?.ReportProgress(1 * 100 / frameCount, $"已处理 1/{frameCount} 帧");
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
                worker?.ReportProgress((1 + i + 1) * 100 / frameCount, $"已处理 {1 + i + 1}/{frameCount} 帧");
                yield return frame;
            }

            // 导出最后一帧
            if (final > 0)
            {
                foreach (var spine in spinesToRender) spine.Update(delta);
                var finalFrame = GetFrame(spinesToRender);
                worker?.ReportProgress(100, $"已处理 {frameCount}/{frameCount} 帧");
                yield return finalFrame;
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
