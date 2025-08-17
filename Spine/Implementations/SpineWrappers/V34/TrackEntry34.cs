using Spine.SpineWrappers;
using SpineRuntime34;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Implementations.SpineWrappers.V34
{
    internal sealed class TrackEntry34(TrackEntry innerObject, AnimationState34 animationState, SpineObjectData34 data): ITrackEntry
    {
        private readonly TrackEntry _o = innerObject;
        private readonly AnimationState34 _animationState = animationState;
        private readonly SpineObjectData34 _data = data;

        private readonly Dictionary<IAnimationState.TrackEntryDelegate, AnimationState.TrackEntryDelegate> _eventMapping = [];
        private readonly Dictionary<IAnimationState.TrackEntryDelegate, int> _eventCount = [];

        public TrackEntry InnerObject => _o;

#pragma warning disable CS0067

        // 3.4 及以下没有这两个事件
        public event IAnimationState.TrackEntryDelegate? Interrupt;
        public event IAnimationState.TrackEntryDelegate? Dispose;

#pragma warning restore CS0067

        public event IAnimationState.TrackEntryDelegate? Start
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(_animationState.GetTrackEntry(t));
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

        public event IAnimationState.TrackEntryDelegate? End
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(_animationState.GetTrackEntry(t));
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
                    _eventMapping[value] = f = (TrackEntry t) => value(_animationState.GetTrackEntry(t));
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

        public int TrackIndex { get => _o.TrackIndex; }

        public IAnimation Animation { get => _data.AnimationsByName[_o.Animation.Name]; }

        public ITrackEntry? Next { get { var t = _o.Next; return t is null ? null : _animationState.GetTrackEntry(t); } }

        public bool Loop { get => _o.Loop; set => _o.Loop = value; }

        public float TrackTime { get => _o.Time; set => _o.Time = value; }

        public float TimeScale { get => _o.TimeScale; set => _o.TimeScale = value; }

        public float Alpha { get => _o.Mix; set => _o.Mix = value; }

        public float MixDuration { get => _o.MixDuration; set => _o.MixDuration = value; }

        public override string ToString() => _o.ToString();
    }
}
