using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers
{
    public interface IAnimationState
    {
        /// <summary>
        /// 事件方法签名
        /// </summary>
        /// <param name="trackEntry"></param>
        public delegate void TrackEntryDelegate(ITrackEntry trackEntry);

        /// <summary>
        /// Start 事件
        /// </summary>
        public event TrackEntryDelegate? Start;

        /// <summary>
        /// Interrupt 事件
        /// </summary>
        public event TrackEntryDelegate? Interrupt;

        /// <summary>
        /// End 事件
        /// </summary>
        public event TrackEntryDelegate? End;

        /// <summary>
        /// Complete 事件
        /// </summary>
        public event TrackEntryDelegate? Complete;

        /// <summary>
        /// Dispose 事件
        /// </summary>
        public event TrackEntryDelegate? Dispose;

        /// <summary>
        /// 速度因子
        /// </summary>
        public float TimeScale { get; set; }

        /// <summary>
        /// 遍历所有的轨道, 可能存在 null
        /// </summary>
        public IEnumerable<ITrackEntry?> IterTracks();

        /// <summary>
        /// 更新时间
        /// </summary>
        public void Update(float delta);

        /// <summary>
        /// 将动画应用到骨骼上
        /// </summary>
        public void Apply(ISkeleton skeleton);

        /// <summary>
        /// 获取指定轨道当前播放条目, 可能返回 null
        /// </summary>
        public ITrackEntry? GetCurrent(int index);

        /// <summary>
        /// 清除指定轨道
        /// </summary>
        public void ClearTrack(int index);

        /// <summary>
        /// 清除所有轨道
        /// </summary>
        public void ClearTracks();

        /// <summary>
        /// 使用动画名设置轨道动画
        /// </summary>
        public ITrackEntry SetAnimation(int trackIndex, string animationName, bool loop);

        /// <summary>
        /// 使用动画对象设置轨道动画
        /// </summary>
        public ITrackEntry SetAnimation(int trackIndex, IAnimation animation, bool loop);

        /// <summary>
        /// 设置轨道空动画
        /// </summary>
        public ITrackEntry SetEmptyAnimation(int trackIndex, float mixDuration);

        /// <summary>
        /// 设置所有轨道空动画
        /// </summary>
        public void SetEmptyAnimations(float mixDuration);

        /// <summary>
        /// 在指定轨道后添加动画
        /// </summary>
        public ITrackEntry AddAnimation(int trackIndex, string animationName, bool loop, float delay);

        /// <summary>
        /// 在指定轨道后添加动画
        /// </summary>
        public ITrackEntry AddAnimation(int trackIndex, IAnimation animation, bool loop, float delay);

        /// <summary>
        /// 在指定轨道后添加空动画
        /// </summary>
        public ITrackEntry AddEmptyAnimation(int trackIndex, float mixDuration, float delay);
    }
}
