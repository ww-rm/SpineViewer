using Spine.SpineWrappers;
using SpineRuntime38;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Implementations.SpineWrappers.V38
{
    internal sealed class TrackEntry38(TrackEntry innerObject, AnimationState38 animationState, SpineObjectData38 data): ITrackEntry
    {
        private readonly TrackEntry _o = innerObject;
        private readonly AnimationState38 _animationState = animationState;
        private readonly SpineObjectData38 _data = data;

        private readonly Dictionary<IAnimationState.TrackEntryDelegate, AnimationState.TrackEntryDelegate> _eventMapping = [];
        private readonly Dictionary<IAnimationState.TrackEntryDelegate, int> _eventCount = [];

        public TrackEntry InnerObject => _o;

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

        public event IAnimationState.TrackEntryDelegate? Interrupt
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(_animationState.GetTrackEntry(t));
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

        public event IAnimationState.TrackEntryDelegate? Dispose
        {
            add
            {
                if (value is null) return;
                if (!_eventMapping.TryGetValue(value, out var f))
                {
                    _eventMapping[value] = f = (TrackEntry t) => value(_animationState.GetTrackEntry(t));
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

        public int TrackIndex { get => _o.TrackIndex; }

        public IAnimation Animation { get => _data.AnimationsByName[_o.Animation.Name]; }

        public ITrackEntry? Next { get { var t = _o.Next; return t is null ? null : _animationState.GetTrackEntry(t); } }

        public bool Loop { get => _o.Loop; set => _o.Loop = value; }

        public float TrackTime { get => _o.TrackTime; set => _o.TrackTime = value; }

        public float TimeScale { get => _o.TimeScale; set => _o.TimeScale = value; }

        public float Alpha { get => _o.Alpha; set => _o.Alpha = value; }

        public float MixDuration { get => _o.MixDuration; set => _o.MixDuration = value; }

        public override string ToString() => _o.ToString();
    }
}
