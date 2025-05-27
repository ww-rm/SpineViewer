using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Frozen;
using System.Collections.Immutable;
using Spine.SpineWrappers;
using SpineRuntime42;

namespace Spine.Implementations.SpineWrappers.V42
{
    internal sealed class Skeleton42 : ISkeleton
    {
        private static readonly FrozenDictionary<ISkeleton.Physics, Skeleton.Physics> _physicsMapping = new Dictionary<ISkeleton.Physics, Skeleton.Physics>()
        {
            [ISkeleton.Physics.None] = Skeleton.Physics.None,
            [ISkeleton.Physics.Reset] = Skeleton.Physics.Reset,
            [ISkeleton.Physics.Update] = Skeleton.Physics.Update,
            [ISkeleton.Physics.Pose] = Skeleton.Physics.Pose,
        }.ToFrozenDictionary();

        private readonly Skeleton _o;
        private readonly SpineObjectData42 _data;

        private readonly ImmutableArray<IBone> _bones;
        private readonly FrozenDictionary<string, IBone> _bonesByName;
        private readonly ImmutableArray<ISlot> _slots;
        private readonly FrozenDictionary<string, ISlot> _slotsByName;

        private Skin42? _skin;

        public Skeleton42(Skeleton innerObject, SpineObjectData42 data)
        {
            _o = innerObject;
            _data = data;

            List<Bone42> bones = [];
            Dictionary<string, IBone> bonesByName = [];
            foreach (var b in _o.Bones)
            {
                var bone = new Bone42(b, b.Parent is null ? null : bones[b.Parent.Data.Index]);
                bones.Add(bone);
                bonesByName[bone.Name] = bone;
            }
            _bones = bones.Cast<IBone>().ToImmutableArray();
            _bonesByName = bonesByName.ToFrozenDictionary();

            List<Slot42> slots = [];
            Dictionary<string, ISlot> slotsByName = [];
            foreach (var s in _o.Slots)
            {
                var slot = new Slot42(s, _data, bones[s.Bone.Data.Index]);
                slots.Add(slot);
                slotsByName[slot.Name] = slot;
            }
            _slots = slots.Cast<ISlot>().ToImmutableArray();
            _slotsByName = slotsByName.ToFrozenDictionary();
        }

        public Skeleton InnerObject => _o;

        public float R { get => _o.R; set => _o.R = value; }
        public float G { get => _o.G; set => _o.G = value; }
        public float B { get => _o.B; set => _o.B = value; }
        public float A { get => _o.A; set => _o.A = value; }
        public float X { get => _o.X; set => _o.X = value; }
        public float Y { get => _o.Y; set => _o.Y = value; }
        public float ScaleX { get => _o.ScaleX; set => _o.ScaleX = value; }
        public float ScaleY { get => _o.ScaleY; set => _o.ScaleY = value; }

        public ImmutableArray<IBone> Bones => _bones;
        public FrozenDictionary<string, IBone> BonesByName => _bonesByName;
        public ImmutableArray<ISlot> Slots => _slots;
        public FrozenDictionary<string, ISlot> SlotsByName => _slotsByName;

        public ISkin? Skin 
        {
            get => _skin;
            set
            {
                if (value is null)
                {
                    _o.Skin = null;
                    _skin = null;
                    return;
                }
                if (value is Skin42 sk)
                {
                    _o.Skin = sk.InnerObject;
                    _skin = sk;
                    return;
                }
                throw new ArgumentException($"Received {value.GetType().Name}", nameof(value));
            }
        }

        public IEnumerable<ISlot> IterDrawOrder() => _o.DrawOrder.Select(s => _slots[s.Data.Index]);
        public void UpdateCache() => _o.UpdateCache();
        public void UpdateWorldTransform(ISkeleton.Physics physics) => _o.UpdateWorldTransform(_physicsMapping[physics]);
        public void SetToSetupPose() => _o.SetToSetupPose();
        public void SetBonesToSetupPose() => _o.SetBonesToSetupPose();
        public void SetSlotsToSetupPose() => _o.SetSlotsToSetupPose();
        public void Update(float delta) => _o.Update(delta);

        public void GetBounds(out float x, out float y, out float w, out float h)
        {
            float[] _ = [];
            _o.GetBounds(out x, out y, out w, out h, ref _);
        }

        public override string ToString() => _o.ToString();
    }
}
