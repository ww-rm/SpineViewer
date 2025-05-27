using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Utils;
using Spine.SpineWrappers;
using Spine.SpineWrappers.Attachments;
using SpineRuntime21;
using Spine.Implementations.SpineWrappers.V21.Attachments;

namespace Spine.Implementations.SpineWrappers.V21
{
    [SpineImplementation(2, 1)]
    internal sealed class SpineObjectData21 : SpineObjectData
    {
        private readonly Atlas _atlas;
        private readonly SkeletonData _skeletonData;
        private readonly AnimationStateData _animationStateData;

        private readonly ImmutableArray<ISkin> _skins;
        private readonly FrozenDictionary<string, ISkin> _skinsByName;
        private readonly FrozenDictionary<string, FrozenDictionary<string, IAttachment>> _slotAttachments;
        private readonly ImmutableArray<IAnimation> _animations;
        private readonly FrozenDictionary<string, IAnimation> _animationsByName;

        public SpineObjectData21(string skelPath, string atlasPath) : base(skelPath, atlasPath)
        {
            // 加载 atlas
            try { _atlas = new Atlas(atlasPath, _textureLoader); }
            catch (Exception ex) { throw new InvalidDataException($"Failed to load atlas '{atlasPath}'", ex); }

            try
            {
                if (Utf8Validator.IsUtf8(skelPath))
                    _skeletonData = new SkeletonJson(_atlas).ReadSkeletonData(skelPath);
                else
                    _skeletonData = new SkeletonBinary(_atlas).ReadSkeletonData(skelPath);
            }
            catch (Exception ex)
            {
                _atlas.Dispose();
                throw new InvalidDataException($"Failed to load skeleton file {skelPath}", ex);
            }

            // 加载动画数据
            _animationStateData = new AnimationStateData(_skeletonData);

            // 整理皮肤和附件
            Dictionary<string, Dictionary<string, IAttachment>> slotAttachments = [];
            List<ISkin> skins = [];
            Dictionary<string, ISkin> skinsByName = [];
            foreach (var s in _skeletonData.Skins)
            {
                var skin = new Skin21(s);
                skins.Add(skin);
                skinsByName[s.Name] = skin;
                foreach (var (k, att) in s.Attachments)
                {
                    var slotName = _skeletonData.Slots[k.Key].Name;
                    if (!slotAttachments.TryGetValue(slotName, out var attachments))
                        slotAttachments[slotName] = attachments = [];

                    attachments[att.Name] = att switch
                    {
                        RegionAttachment regionAtt => new RegionAttachment21(regionAtt),
                        MeshAttachment meshAtt => new MeshAttachment21(meshAtt),
                        SkinnedMeshAttachment skMeshAtt => new SkinnedMeshAttachment21(skMeshAtt),
                        BoundingBoxAttachment bbAtt => new BoundingBoxAttachment21(bbAtt),
                        _ => throw new InvalidOperationException($"Unrecognized attachment type {att.GetType().FullName}")
                    };
                }
            }
            _slotAttachments = slotAttachments.ToFrozenDictionary(it => it.Key, it => it.Value.ToFrozenDictionary());
            _skins = skins.ToImmutableArray();
            _skinsByName = skinsByName.ToFrozenDictionary();

            // 整理所有动画数据
            List<IAnimation> animations = [];
            Dictionary<string, IAnimation> animationsByName = [];
            foreach (var a in _skeletonData.Animations)
            {
                var anime = new Animation21(a);
                animations.Add(anime);
                animationsByName[anime.Name] = anime;
            }
            _animations = animations.ToImmutableArray();
            _animationsByName = animationsByName.ToFrozenDictionary();
        }

        public override string SkeletonVersion => _skeletonData.Version;

        public override ImmutableArray<ISkin> Skins => _skins;

        public override FrozenDictionary<string, ISkin> SkinsByName => _skinsByName;

        public override FrozenDictionary<string, FrozenDictionary<string, IAttachment>> SlotAttachments => _slotAttachments;

        public override float DefaultMix { get => _animationStateData.DefaultMix; set => _animationStateData.DefaultMix = value; }

        public override ImmutableArray<IAnimation> Animations => _animations;

        public override FrozenDictionary<string, IAnimation> AnimationsByName => _animationsByName;

        protected override void DisposeAtlas() => _atlas.Dispose();

        public override ISkeleton CreateSkeleton() => new Skeleton21(new(_skeletonData), this);

        public override IAnimationState CreateAnimationState() => new AnimationState21(new(_animationStateData), this);

        public override ISkeletonClipping CreateSkeletonClipping() => new SkeletonClipping21();

        public override ISkin CreateSkin(string name) => new Skin21(name);
    }
}
