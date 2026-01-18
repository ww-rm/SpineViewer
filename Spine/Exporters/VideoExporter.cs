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
        private readonly object _frameOutputLock = new();
        private SFMLImageVideoFrame? _frameOutput;

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

        public float Speed
        {
            get => _speed;
            set
            {
                if (_speed <= 0)
                {
                    _logger.Warn("Omit invalid speed: {0}", value);
                    return;
                }
                _speed = value;
            }
        }
        protected float _speed = 1f;

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
            bool hasFinal = _keepLast && deltaFinal > 1e-3;

            var frameCount = 1 + total + (hasFinal ? 1 : 0); // 所有帧的数量 = 起始帧 + 完整帧 + 最后一帧
            return frameCount;
        }

        /// <summary>
        /// 生成帧序列, 用于导出帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(SpineObject[] spines)
        {
            float delta = 1f / _fps;
            int total = (int)(_duration * _fps); // 完整帧的数量
            var deltaFinal = _duration - delta * total; // 最后一帧时长
            bool hasFinal = _keepLast && deltaFinal > 1e-3;

            // 导出首帧
            var firstFrame = GetFrame(spines);
            yield return firstFrame;

            // 导出完整帧
            for (int i = 0; i < total; i++)
            {
                foreach (var spine in spines) spine.Update(delta * _speed);
                yield return GetFrame(spines);
            }

            // 导出最后一帧
            if (hasFinal)
            {
                foreach (var spine in spines) spine.Update(deltaFinal * _speed); 
                yield return GetFrame(spines);
            }
        }

        /// <summary>
        /// 帧渲染任务, 用于保证每一帧的渲染都在同一个线程里完成
        /// </summary>
        private void GetFramesTask(SpineObject[] spines, CancellationToken ct)
        {
            // XXX: 也许和 SFML 多线程或者 FFmpeg 调用有关, GetFrame 无法在异步调用中连续使用, 会导致画面帧丢失或者卡死等异常现象
            // 因此把帧生成包在一个子线程中连续调用, 通过成员变量和锁来输出帧数据
            foreach (var frame in GetFrames(spines))
            {
                while (!ct.IsCancellationRequested)
                {
                    // 等待之前的数据被取走
                    lock (_frameOutputLock)
                    {
                        if (_frameOutput is null)
                            break;
                    }
                    Thread.Sleep(10);
                }
                if (ct.IsCancellationRequested)
                {
                    frame.Dispose();
                    break;
                }
                _frameOutput = frame;
            }
        }

        /// <summary>
        /// 生成帧序列, 支持中途取消和进度输出, 用于动图视频等单个文件输出
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(SpineObject[] spines, string output, CancellationToken ct)
        {
            int frameCount = GetFrameCount();
            int frameIdx = 0;
            SFMLImageVideoFrame frame = null;
            
            using var getFramesTask = Task.Run(() => GetFramesTask(spines, ct), ct);

            _progressReporter?.Invoke(frameCount, 0, $"[0/{frameCount}] {output}");
            while (frameIdx < frameCount)
            {
                while (!ct.IsCancellationRequested)
                {
                    // 等待新帧的生成
                    lock (_frameOutputLock)
                    {
                        if (_frameOutput is not null)
                        {
                            frame = _frameOutput;
                            _frameOutput = null;
                            break;
                        }
                    }

                    Thread.Sleep(10);
                }

                if (ct.IsCancellationRequested)
                {
                    _logger.Info("Export cancelled");
                    frame?.Dispose();
                    break;
                }

                _progressReporter?.Invoke(frameCount, frameIdx + 1, $"[{frameIdx + 1}/{frameCount}] {output}");
                yield return frame;
                frame = null;
                frameIdx++;
            }

            getFramesTask.Wait(CancellationToken.None); // 等待结束 (正常结束或者被取消)
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
