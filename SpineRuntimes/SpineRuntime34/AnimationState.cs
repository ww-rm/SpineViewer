/******************************************************************************
 * Spine Runtimes Software License v2.5
 * 
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace SpineRuntime34 {
	public class AnimationState {
        static readonly Animation EmptyAnimation = new Animation("<empty>", new ExposedList<Timeline>(), 0);
        private AnimationStateData data;
		private ExposedList<TrackEntry> tracks = new ExposedList<TrackEntry>();
		private ExposedList<Event> events = new ExposedList<Event>();
		private float timeScale = 1;

		public AnimationStateData Data { get { return data; } }
        private readonly Pool<TrackEntry> trackEntryPool = new Pool<TrackEntry>();

        /// <summary>A list of tracks that have animations, which may contain nulls.</summary>
        public ExposedList<TrackEntry> Tracks { get { return tracks; } }
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }

        public delegate void TrackEntryDelegate(TrackEntry trackEntry);
        public event TrackEntryDelegate Start, End, Complete;

        public delegate void EventDelegate (AnimationState state, int trackIndex, Event e);
		public event EventDelegate Event;

		public AnimationState (AnimationStateData data) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.data = data;
		}

		public void Update (float delta) {
			delta *= timeScale;
			for (int i = 0; i < tracks.Count; i++) {
				TrackEntry current = tracks.Items[i];
				if (current == null) continue;

				float trackDelta = delta * current.timeScale;
				float time = current.time + trackDelta;
				float endTime = current.endTime;

				current.time = time;
				if (current.previous != null) {
					current.previous.time += trackDelta;
					current.mixTime += trackDelta;
				}

				// Check if completed the animation or a loop iteration.
				if (current.loop ? (current.lastTime % endTime > time % endTime) : (current.lastTime < endTime && time >= endTime)) {
					int count = (int)(time / endTime);
					current.OnComplete();
					if (Complete != null) Complete(current);
				}

				TrackEntry next = current.next;
				if (next != null) {
					next.time = current.lastTime - next.delay;
					if (next.time >= 0) SetCurrent(i, next);
				} else {
					// End non-looping animation when it reaches its end time and there is no next entry.
					if (!current.loop && current.lastTime >= current.endTime) ClearTrack(i);
				}
			}
		}

		public void Apply (Skeleton skeleton) {
			ExposedList<Event> events = this.events;

			for (int i = 0; i < tracks.Count; i++) {
				TrackEntry current = tracks.Items[i];
				if (current == null) continue;

				events.Clear();

				float time = current.time;
				bool loop = current.loop;
				if (!loop && time > current.endTime) time = current.endTime;

				TrackEntry previous = current.previous;
				if (previous == null) {
					if (current.mix == 1)
						current.animation.Apply(skeleton, current.lastTime, time, loop, events);
					else
						current.animation.Mix(skeleton, current.lastTime, time, loop, events, current.mix);
				} else {
					float previousTime = previous.time;
					if (!previous.loop && previousTime > previous.endTime) previousTime = previous.endTime;
					previous.animation.Apply(skeleton, previous.lastTime, previousTime, previous.loop, null);
					// Remove the line above, and uncomment the line below, to allow previous animations to fire events during mixing.
					//previous.animation.Apply(skeleton, previous.lastTime, previousTime, previous.loop, events);
					previous.lastTime = previousTime;

					float alpha = current.mixTime / current.mixDuration * current.mix;
					if (alpha >= 1) {
						alpha = 1;
						current.previous = null;
					}
					current.animation.Mix(skeleton, current.lastTime, time, loop, events, alpha);
				}

				for (int ii = 0, nn = events.Count; ii < nn; ii++) {
					Event e = events.Items[ii];
					current.OnEvent(this, i, e);
					if (Event != null) Event(this, i, e);
				}

				current.lastTime = current.time;
			}
		}

		public void ClearTracks () {
			for (int i = 0, n = tracks.Count; i < n; i++)
				ClearTrack(i);
			tracks.Clear();
		}

		public void ClearTrack (int trackIndex) {
			if (trackIndex >= tracks.Count) return;
			TrackEntry current = tracks.Items[trackIndex];
			if (current == null) return;

			current.OnEnd();
			if (End != null) End(current);

			tracks.Items[trackIndex] = null;

            while (current is not null)
            {
                var tmp = current.next;
                trackEntryPool.Free(current);
                current = tmp;
            }
        }

		private TrackEntry ExpandToIndex (int index) {
			if (index < tracks.Count) return tracks.Items[index];
			while (index >= tracks.Count)
				tracks.Add(null);
			return null;
		}

		private void SetCurrent (int index, TrackEntry entry) {
			TrackEntry current = ExpandToIndex(index);
			if (current != null) {
				TrackEntry previous = current.previous;
				current.previous = null;

				current.OnEnd();
				if (End != null) End(current);

				entry.mixDuration = data.GetMix(current.animation, entry.animation);
				if (entry.mixDuration > 0) {
					entry.mixTime = 0;
					// If a mix is in progress, mix from the closest animation.
					if (previous != null && current.mixTime / current.mixDuration < 0.5f)
						entry.previous = previous;
					else
						entry.previous = current;
				}
			}

			tracks.Items[index] = entry;

            while (current is not null)
            {
                var tmp = current.next;
                trackEntryPool.Free(current);
                current = tmp;
            }

            entry.OnStart();
			if (Start != null) Start(entry);
		}

		/// <seealso cref="SetAnimation(int, Animation, bool)" />
		public TrackEntry SetAnimation (int trackIndex, String animationName, bool loop) {
			Animation animation = data.skeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName, "animationName");
			return SetAnimation(trackIndex, animation, loop);
		}

		/// <summary>Set the current animation. Any queued animations are cleared.</summary>
		public TrackEntry SetAnimation (int trackIndex, Animation animation, bool loop) {
			if (animation == null) throw new ArgumentNullException("animation", "animation cannot be null.");
            TrackEntry entry = trackEntryPool.Obtain();
            entry.trackIndex = trackIndex;
            entry.animation = animation;
			entry.loop = loop;
			entry.time = 0;
			entry.endTime = animation.Duration;
			SetCurrent(trackIndex, entry);
			return entry;
		}

		/// <seealso cref="AddAnimation(int, Animation, bool, float)" />
		public TrackEntry AddAnimation (int trackIndex, String animationName, bool loop, float delay) {
			Animation animation = data.skeletonData.FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("Animation not found: " + animationName, "animationName");
			return AddAnimation(trackIndex, animation, loop, delay);
		}

		/// <summary>Adds an animation to be played delay seconds after the current or last queued animation.</summary>
		/// <param name="delay">May be &lt;= 0 to use duration of previous animation minus any mix duration plus the negative delay.</param>
		public TrackEntry AddAnimation (int trackIndex, Animation animation, bool loop, float delay) {
			if (animation == null) throw new ArgumentNullException("animation", "animation cannot be null.");
            TrackEntry entry = trackEntryPool.Obtain();
            entry.trackIndex = trackIndex;
            entry.animation = animation;
            entry.animation = animation;
			entry.loop = loop;
			entry.time = 0;
			entry.endTime = animation.Duration;

			TrackEntry last = ExpandToIndex(trackIndex);
			if (last != null) {
				while (last.next != null)
					last = last.next;
				last.next = entry;
			} else
				tracks.Items[trackIndex] = entry;

			if (delay <= 0) {
				if (last != null)
					delay += last.endTime - data.GetMix(last.animation, animation);
				else
					delay = 0;
			}
			entry.delay = delay;

			return entry;
		}

        /// <summary>
        /// Sets an empty animation for a track, discarding any queued animations, and mixes to it over the specified mix duration.</summary>
        public TrackEntry SetEmptyAnimation(int trackIndex, float mixDuration)
        {
            TrackEntry entry = SetAnimation(trackIndex, AnimationState.EmptyAnimation, false);
            entry.mixDuration = mixDuration;
            entry.endTime = mixDuration;
            return entry;
        }

        /// <summary>
        /// Adds an empty animation to be played after the current or last queued animation for a track, and mixes to it over the
        /// specified mix duration.</summary>
        /// <returns>
        /// A track entry to allow further customization of animation playback. References to the track entry must not be kept after <see cref="AnimationState.Dispose"/>.
        /// </returns>
        /// <param name="trackIndex">Track number.</param>
        /// <param name="mixDuration">Mix duration.</param>
        /// <param name="delay">Seconds to begin this animation after the start of the previous animation. May be &lt;= 0 to use the animation
        /// duration of the previous track minus any mix duration plus the negative delay.</param>
        public TrackEntry AddEmptyAnimation(int trackIndex, float mixDuration, float delay)
        {
            if (delay <= 0) delay -= mixDuration;
            TrackEntry entry = AddAnimation(trackIndex, AnimationState.EmptyAnimation, false, delay);
            entry.mixDuration = mixDuration;
            entry.endTime = mixDuration;
            return entry;
        }

        /// <summary>
        /// Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix duration.</summary>
        public void SetEmptyAnimations(float mixDuration)
        {
            for (int i = 0, n = tracks.Count; i < n; i++)
            {
                TrackEntry current = tracks.Items[i];
                if (current != null) SetEmptyAnimation(i, mixDuration);
            }
        }

        /// <returns>May be null.</returns>
        public TrackEntry GetCurrent (int trackIndex) {
			if (trackIndex >= tracks.Count) return null;
			return tracks.Items[trackIndex];
		}

		override public String ToString () {
			StringBuilder buffer = new StringBuilder();
			for (int i = 0, n = tracks.Count; i < n; i++) {
				TrackEntry entry = tracks.Items[i];
				if (entry == null) continue;
				if (buffer.Length > 0) buffer.Append(", ");
				buffer.Append(entry.ToString());
			}
			if (buffer.Length == 0) return "<none>";
			return buffer.ToString();
		}
	}

	public class TrackEntry : Pool<TrackEntry>.IPoolable {
		internal TrackEntry next, previous;
        internal int trackIndex;
        internal Animation animation;
		internal bool loop;
		internal float delay, time, lastTime = -1, endTime, timeScale = 1;
		internal float mixTime, mixDuration, mix = 1;

        public int TrackIndex { get { return trackIndex; } }
        public Animation Animation { get { return animation; } }
        public float Delay { get { return delay; } set { delay = value; } }
		public float Time { get { return time; } set { time = value; } }
		public float LastTime { get { return lastTime; } set { lastTime = value; } }
		public float EndTime { get { return endTime; } set { endTime = value; } }
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }
		public float Mix { get { return mix; } set { mix = value; } }
		public bool Loop { get { return loop; } set { loop = value; } }

        /// <summary>
        /// Seconds for mixing from the previous animation to this animation. Defaults to the value provided by
        /// <see cref="AnimationStateData"/> based on the animation before this animation (if any).
        ///
        /// The mix duration can be set manually rather than use the value from AnimationStateData.GetMix.
        /// In that case, the mixDuration must be set before <see cref="AnimationState.Update(float)"/> is next called.
        /// <para>
        /// When using <seealso cref="AnimationState.AddAnimation(int, Animation, bool, float)"/> with a
        /// <code>delay</code> less than or equal to 0, note the <seealso cref="Delay"/> is set using the mix duration from the <see cref=" AnimationStateData"/>
        /// </para>
        ///
        /// </summary>
        public float MixDuration { get { return mixDuration; } set { mixDuration = value; } }

        /// <summary>
        /// The animation queued to start after this animation, or null.</summary>
        public TrackEntry Next { get { return next; } }

        public event AnimationState.TrackEntryDelegate Start, End, Complete;
        public event AnimationState.EventDelegate Event;

        // IPoolable.Reset()
        public void Reset()
        {
            next = null;
            previous = null;
            animation = null;

            Start = null;
            End = null;
            Complete = null;
            Event = null;
        }

        internal void OnStart() { if (Start != null) Start(this); }
        internal void OnEnd() { if (End != null) End(this); }
        internal void OnComplete() { if (Complete != null) Complete(this); }

        internal void OnEvent(AnimationState state, int index, Event e)
        {
            if (Event != null) Event(state, index, e);
        }

        override public String ToString () {
			return animation == null ? "<none>" : animation.name;
		}
	}

    public class Pool<T> where T : class, new()
    {
        public readonly int max;
        readonly Stack<T> freeObjects;

        public int Count { get { return freeObjects.Count; } }
        public int Peak { get; private set; }

        public Pool(int initialCapacity = 16, int max = int.MaxValue)
        {
            freeObjects = new Stack<T>(initialCapacity);
            this.max = max;
        }

        public T Obtain()
        {
            return freeObjects.Count == 0 ? new T() : freeObjects.Pop();
        }

        public void Free(T obj)
        {
            if (obj == null) throw new ArgumentNullException("obj", "obj cannot be null");
            if (freeObjects.Count < max)
            {
                freeObjects.Push(obj);
                Peak = Math.Max(Peak, freeObjects.Count);
            }
            Reset(obj);
        }

        //		protected void FreeAll (List<T> objects) {
        //			if (objects == null) throw new ArgumentNullException("objects", "objects cannot be null.");
        //			var freeObjects = this.freeObjects;
        //			int max = this.max;
        //			for (int i = 0; i < objects.Count; i++) {
        //				T obj = objects[i];
        //				if (obj == null) continue;
        //				if (freeObjects.Count < max) freeObjects.Push(obj);
        //				Reset(obj);
        //			}
        //			Peak = Math.Max(Peak, freeObjects.Count);
        //		}

        public void Clear()
        {
            freeObjects.Clear();
        }

        protected void Reset(T obj)
        {
            var poolable = obj as IPoolable;
            if (poolable != null) poolable.Reset();
        }

        public interface IPoolable
        {
            void Reset();
        }
    }
}
