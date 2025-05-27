using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers
{
    public interface ITrackEntry
    {
        /// <summary>
        /// Start 事件
        /// </summary>
        public event IAnimationState.TrackEntryDelegate? Start;

        /// <summary>
        /// Interrupt 事件
        /// </summary>
        public event IAnimationState.TrackEntryDelegate? Interrupt;

        /// <summary>
        /// End 事件
        /// </summary>
        public event IAnimationState.TrackEntryDelegate? End;

        /// <summary>
        /// Complete 事件
        /// </summary>
        public event IAnimationState.TrackEntryDelegate? Complete;

        /// <summary>
        /// Dispose 事件
        /// </summary>
        public event IAnimationState.TrackEntryDelegate? Dispose;

        /// <summary>
        /// 所在轨道序号
        /// </summary>
        public int TrackIndex { get; }


        /// <summary>
        /// 播放的动画
        /// </summary>
        public IAnimation Animation { get; }

        /// <summary>
        /// 下一个条目, 形成播放链表
        /// </summary>
        public ITrackEntry? Next { get; }

        /// <summary>
        /// 动画是否循环
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// 轨道当前时间
        /// </summary>
        public float TrackTime { get; set; }

        /// <summary>
        /// 速度因子
        /// </summary>
        public float TimeScale { get; set; }

        /// <summary>
        /// 多轨道的 Alpha 混合
        /// </summary>
        public float Alpha { get; set; }

        /// <summary>
        /// 过渡到下一个条目的时长
        /// </summary>
        public float MixDuration { get; set; }
    }
}
