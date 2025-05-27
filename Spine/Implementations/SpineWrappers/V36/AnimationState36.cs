using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers;
using SpineRuntime36;

namespace Spine.Implementations.SpineWrappers.V36
{
    internal sealed class AnimationState36(AnimationState innerObject, SpineObjectData36 data) : IAnimationState
    {
        private readonly AnimationState _o = innerObject;
        private readonly SpineObjectData36 _data = data;

        private readonly Dictionary<TrackEntry, TrackEntry36> _trackEntryPool = [];

        private readonly Dictionary<IAnimationState.TrackEntryDelegate, AnimationState.TrackEntryDelegate> _eventMapping = [];
        private readonly Dictionary<IAnimationState.TrackEntryDelegate, int> _eventCount = [];

        public AnimationState InnerObject => _o;

        public event IAnimationState.TrackEntryDelegate? Start
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(GetTrackEntry(t));
                    _eventCount[value] = 0;
                }
                _o.Start += f;
                _eventCount[value]++;
            }
            remove
            {
                if (value is null) return;
                if (_eventMapping.TryGetValue(value, out var f))
                {
                    _o.Start -= f;
                    _eventCount[value]--;
                    if (_eventCount[value] <= 0)
                    {
                        _eventMapping.Remove(value);
                        _eventCount.Remove(value);
                    }
                }
            }
        }

        public event IAnimationState.TrackEntryDelegate? Interrupt
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(GetTrackEntry(t));
                    _eventCount[value] = 0;
                }
                _o.Interrupt += f;
                _eventCount[value]++;
            }
            remove
            {
                if (value is null) return;
                if (_eventMapping.TryGetValue(value, out var f))
                {
                    _o.Interrupt -= f;
                    _eventCount[value]--;
                    if (_eventCount[value] <= 0)
                    {
                        _eventMapping.Remove(value);
                        _eventCount.Remove(value);
                    }
                }
            }
        }

        public event IAnimationState.TrackEntryDelegate? End
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(GetTrackEntry(t));
                    _eventCount[value] = 0;
                }
                _o.End += f;
                _eventCount[value]++;
            }
            remove
            {
                if (value is null) return;
                if (_eventMapping.TryGetValue(value, out var f))
                {
                    _o.End -= f;
                    _eventCount[value]--;
                    if (_eventCount[value] <= 0)
                    {
                        _eventMapping.Remove(value);
                        _eventCount.Remove(value);
                    }
                }
            }
        }

        public event IAnimationState.TrackEntryDelegate? Complete
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(GetTrackEntry(t));
                    _eventCount[value] = 0;
                }
                _o.Complete += f;
                _eventCount[value]++;
            }
            remove
            {
                if (value is null) return;
                if (_eventMapping.TryGetValue(value, out var f))
                {
                    _o.Complete -= f;
                    _eventCount[value]--;
                    if (_eventCount[value] <= 0)
                    {
                        _eventMapping.Remove(value);
                        _eventCount.Remove(value);
                    }
                }
            }
        }

        public event IAnimationState.TrackEntryDelegate? Dispose
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(GetTrackEntry(t));
                    _eventCount[value] = 0;
                }
                _o.Dispose += f;
                _eventCount[value]++;
            }
            remove
            {
                if (value is null) return;
                if (_eventMapping.TryGetValue(value, out var f))
                {
                    _o.Dispose -= f;
                    _eventCount[value]--;
                    if (_eventCount[value] <= 0)
                    {
                        _eventMapping.Remove(value);
                        _eventCount.Remove(value);
                    }
                }
            }
        }

        public float TimeScale { get => _o.TimeScale; set => _o.TimeScale = value; }

        public void Update(float delta) => _o.Update(delta);

        public void Apply(ISkeleton skeleton)
        {
            if (skeleton is Skeleton36 skel)
            {
                _o.Apply(skel.InnerObject);
                return;
            }
            throw new ArgumentException($"Received {skeleton.GetType().Name}", nameof(skeleton));
        }

        /// <summary>
        /// 获取 <see cref="ITrackEntry"/> 对象, 不存在则创建
        /// </summary>
        public ITrackEntry GetTrackEntry(TrackEntry trackEntry)
        {
            if (!_trackEntryPool.TryGetValue(trackEntry, out var tr))
                _trackEntryPool[trackEntry] = tr = new(trackEntry, this, _data);
            return tr;
        }

        public IEnumerable<ITrackEntry?> IterTracks() => _o.Tracks.Select(t => t is null ? null : GetTrackEntry(t));

        public ITrackEntry? GetCurrent(int index) { var t = _o.GetCurrent(index); return t is null ? null : GetTrackEntry(t); }

        public void ClearTrack(int index) => _o.ClearTrack(index);

        public void ClearTracks() => _o.ClearTracks();

        public ITrackEntry SetAnimation(int trackIndex, string animationName, bool loop)
            => GetTrackEntry(_o.SetAnimation(trackIndex, animationName, loop));

        public ITrackEntry SetAnimation(int trackIndex, IAnimation animation, bool loop)
        {
            if (animation is Animation36 anime) 
                return GetTrackEntry(_o.SetAnimation(trackIndex, anime.InnerObject, loop));
            throw new ArgumentException($"Received {animation.GetType().Name}", nameof(animation));
        }

        public ITrackEntry SetEmptyAnimation(int trackIndex, float mixDuration) => GetTrackEntry(_o.SetEmptyAnimation(trackIndex, mixDuration));

        public void SetEmptyAnimations(float mixDuration) => _o.SetEmptyAnimations(mixDuration);

        public ITrackEntry AddAnimation(int trackIndex, string animationName, bool loop, float delay)
            => GetTrackEntry(_o.AddAnimation(trackIndex, animationName, loop, delay));

        public ITrackEntry AddAnimation(int trackIndex, IAnimation animation, bool loop, float delay)
        {
            if (animation is Animation36 anime)
                return GetTrackEntry(_o.AddAnimation(trackIndex, anime.InnerObject, loop, delay));
            throw new ArgumentException($"Received {animation.GetType().Name}", nameof(animation));
        }

        public ITrackEntry AddEmptyAnimation(int trackIndex, float mixDuration, float delay)
            => GetTrackEntry(_o.AddEmptyAnimation(trackIndex, mixDuration, delay));

        public override string ToString() => _o.ToString();
    }
}
