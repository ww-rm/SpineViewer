using NLog;
using SFML.System;
using SkiaSharp;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Exporters
{
    /// <summary>
    /// 多帧画面导出基类, 可以获取连续的帧序列
    /// </summary>
    public abstract class VideoExporter : BaseExporter
    {
        public VideoExporter(uint width, uint height) : base(width, height) { }
        public VideoExporter(Vector2u resolution) : base(resolution) { }

        /// <summary>
        /// 导出时长
        /// </summary>
        public float Duration 
        { 
            get => _duration; 
            set
            {
                if (value < 0)
                {
                    _logger.Warn("Omit invalid duration: {0}", value);
                    return;
                }
                _duration = value;
            }
        }
        protected float _duration = 0;

        /// <summary>
        /// 帧率
        /// </summary>
        public float Fps
        {
            get => _fps;
            set
            {
                if (value <= 0)
                {
                    _logger.Warn("Omit invalid fps: {0}", value);
                    return;
                }
                _fps = value;
            }
        }
        protected float _fps = 24;

        /// <summary>
        /// 是否保留最后一帧
        /// </summary>
        public bool KeepLast { get => _keepLast; set => _keepLast = value; }
        protected bool _keepLast = true;

        /// <summary>
        /// 获取总帧数
        /// </summary>
        public int GetFrameCount()
        {
            var delta = 1f / _fps;
            var total = (int)(_duration * _fps); // 完整帧的数量

            var deltaFinal = _duration - delta * total; // 最后一帧时长
            var final = _keepLast && deltaFinal > 1e-3 ? 1 : 0;

            var frameCount = 1 + total + final; // 所有帧的数量 = 起始帧 + 完整帧 + 最后一帧
            return frameCount;
        }

        /// <summary>
        /// 生成帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(SpineObject[] spines)
        {
            float delta = 1f / _fps;
            int total = (int)(_duration * _fps); // 完整帧的数量
            bool hasFinal = _keepLast && (_duration - delta * total) > 1e-3;

            // 导出首帧
            var firstFrame = GetFrame(spines);
            yield return firstFrame;

            // 导出完整帧
            for (int i = 0; i < total; i++)
            {
                foreach (var spine in spines) spine.Update(delta);
                yield return GetFrame(spines);
            }

            // 导出最后一帧
            if (hasFinal)
            {
                // XXX: 此处还是按照完整的一帧时长进行更新, 也许可以只更新准确的最后一帧时长
                foreach (var spine in spines) spine.Update(delta); 
                yield return GetFrame(spines);
            }
        }

        /// <summary>
        /// 生成帧序列, 支持中途取消和进度输出
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(SpineObject[] spines, string output, CancellationToken ct)
        {
            int frameCount = GetFrameCount();
            int frameIdx = 0;

            _progressReporter?.Invoke(frameCount, 0, $"[{frameIdx}/{frameCount}] {output}");
            foreach (var frame in GetFrames(spines))
            {
                if (ct.IsCancellationRequested)
                {
                    _logger.Info("Export cancelled");
                    frame.Dispose();
                    break;
                }

                _progressReporter?.Invoke(frameCount, frameIdx, $"[{frameIdx + 1}/{frameCount}] {output}");
                yield return frame;
                frameIdx++;
            }
        }

        public sealed override void Export(string output, params SpineObject[] spines) => Export(output, default, spines);

        /// <summary>
        /// 导出给定的模型, 从前往后对应从上往下的渲染顺序
        /// </summary>
        /// <param name="output">输出路径, 一般而言都是文件路径, 少数情况指定的是文件夹</param>
        /// <param name="ct">取消令牌</param>
        /// <param name="spines">要导出的模型, 从前往后对应从上往下的渲染顺序</param>
        public abstract void Export(string output, CancellationToken ct, params SpineObject[] spines);
    }
}
