using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Extensions
{
    public static class SpineObjectExtension
    {
        /// <summary>
        /// 获取一个对象副本, 继承所有状态
        /// </summary>
        public static SpineObject Copy(this SpineObject self, bool keepTrackTime = false)
        {
            var spineObject = new SpineObject(self, true);

            // 拷贝轨道动画, 但是仅拷贝第一个条目
            foreach (var tr in self.AnimationState.IterTracks().Where(t => t is not null))
            {
                var t = spineObject.AnimationState.SetAnimation(tr!.TrackIndex, tr.Animation, tr.Loop);
                if (keepTrackTime)
                    t.TrackTime = tr.TrackTime;
            }

            spineObject.Update(0);
            return spineObject;
        }

        /// <summary>
        /// 重置所有轨道的动画为第一个条目动画
        /// </summary>
        /// <param name="self"></param>
        public static void ResetAnimationsTime(this SpineObject self)
        {
            foreach (var e in self.AnimationState.IterTracks())
            {
                if (e is not null)
                    self.AnimationState.SetAnimation(e.TrackIndex, e.Animation, e.Loop);
            }
            self.Update(0);
        }

        /// <summary>
        /// 获取当前状态包围盒
        /// </summary>
        public static Rect GetCurrentBounds(this SpineObject self)
        {
            self.Skeleton.GetBounds(out var x, out var y, out var w, out var h);
            return new(x, y, w, h);
        }

        /// <summary>
        /// 计算所有轨道第一个条目的动画时长最大值
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static float GetAnimationMaxDuration(this SpineObject self)
        {
            return self.AnimationState.IterTracks().Select(t => t?.Animation.Duration ?? 0).DefaultIfEmpty(0).Max();
        }

        /// <summary>
        /// 按给定的帧率获取所有轨道第一个条目动画全时长包围盒大小, 是一个耗时操作, 如果可能的话最好缓存结果
        /// </summary>
        public static Rect GetAnimationBounds(this SpineObject self, float fps = 10)
        {
            using var copy = self.Copy();
            var bounds = copy.GetCurrentBounds();
            var maxDuration = copy.GetAnimationMaxDuration();
            for (float tick = 0, delta = 1 / fps; tick < maxDuration; tick += delta)
            {
                bounds.Union(copy.GetCurrentBounds());
                copy.Update(delta);
            }
            return bounds;
        }
    }
}
