using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Utils;
using SpineRuntime40;
using Spine.Implementations.V40.Attachments;
using Spine.Interfaces;
using Spine.Interfaces.Attachments;

namespace Spine.Implementations.V40
{
    [SpineImplementation(4, 0)]
    internal sealed class SpineObjectData40 : SpineObjectData
    {
        private readonly Atlas? _atlas;
        private readonly SkeletonData _skeletonData;
        private readonly AnimationStateData _animationStateData;

        private readonly ImmutableArray<ISkin> _skins;
        private readonly FrozenDictionary<string, ISkin> _skinsByName;
        private readonly FrozenDictionary<Attachment, IAttachment> _attachmentsMapping;
        private readonly ImmutableArray<IAnimation> _animations;
        private readonly FrozenDictionary<string, IAnimation> _animationsByName;

        public SpineObjectData40(string skelPath, string? atlasPath, TextureLoader textureLoader)
            : base(skelPath, atlasPath, textureLoader)
        {
            // 加载 atlas
            try
            {
                if (!DisableAtlasLoading && atlasPath is not null)
                    _atlas = new Atlas(atlasPath, textureLoader);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to load atlas '{0}'", atlasPath);
            }

            // 加载 skel, 允许上一步 atlas 加载为空
            try
            {
                _skeletonData = ReadSkeletonData(skelPath);
            }
            catch (Exception ex)
            {
                _atlas?.Dispose();
                throw new InvalidDataException($"Failed to load skeleton file {skelPath} --> {ex.Message}", ex);
            }

            // 加载动画数据
            _animationStateData = new AnimationStateData(_skeletonData);

            // 整理皮肤和附件
            Dictionary<Attachment, IAttachment> attachmentsMapping = [];
            List<ISkin> skins = [];
            Dictionary<string, ISkin> skinsByName = [];
            foreach (var s in _skeletonData.Skins)
            {
                var skin = new Skin40(s);
                skins.Add(skin);
                skinsByName[s.Name] = skin;
                foreach (var entry in s.Attachments)
                {
                    var att = entry.Attachment;
                    attachmentsMapping[att] = att switch
                    {
                        RegionAttachment regionAtt => new RegionAttachment40(regionAtt),
                        MeshAttachment meshAtt => new MeshAttachment40(meshAtt),
                        ClippingAttachment clipAtt => new ClippingAttachment40(clipAtt),
                        BoundingBoxAttachment bbAtt => new BoundingBoxAttachment40(bbAtt),
                        PathAttachment pathAtt => new PathAttachment40(pathAtt),
                        PointAttachment ptAtt => new PointAttachment40(ptAtt),
                        _ => throw new InvalidOperationException($"Unrecognized attachment type {att.GetType().FullName}")
                    };
                }
            }
            _attachmentsMapping = attachmentsMapping.ToFrozenDictionary();
            _skins = skins.ToImmutableArray();
            _skinsByName = skinsByName.ToFrozenDictionary();

            // 整理所有动画数据
            List<IAnimation> animations = [];
            Dictionary<string, IAnimation> animationsByName = [];
            foreach (var a in _skeletonData.Animations)
            {
                var anime = new Animation40(a);
                animations.Add(anime);
                animationsByName[anime.Name] = anime;
            }
            _animations = animations.ToImmutableArray();
            _animationsByName = animationsByName.ToFrozenDictionary();
        }

        private SkeletonData ReadSkeletonData(string skelPath)
        {
            if (Utf8Validator.IsUtf8(skelPath))
            {
                if (_atlas is null)
                {
                    var loader = EmptyAttachmentLoader.DefaultLoader;
                    try { return new SkeletonJson(loader).ReadSkeletonData(skelPath); }
                    catch (ArgumentException) { throw; }
                    catch { }
                    return new SkeletonBinary(loader).ReadSkeletonData(skelPath);
                }
                else
                {
                    try { return new SkeletonJson(_atlas).ReadSkeletonData(skelPath); }
                    catch (ArgumentException) { throw; }
                    catch { }
                    return new SkeletonBinary(_atlas).ReadSkeletonData(skelPath);
                }
            }
            else
            {
                if (_atlas is null)
                {
                    var loader = EmptyAttachmentLoader.DefaultLoader;
                    try { return new SkeletonBinary(loader).ReadSkeletonData(skelPath); }
                    catch (ArgumentException) { throw; }
                    catch { }
                    return new SkeletonJson(loader).ReadSkeletonData(skelPath);
                }
                else
                {
                    try { return new SkeletonBinary(_atlas).ReadSkeletonData(skelPath); }
                    catch (ArgumentException) { throw; }
                    catch { }
                    return new SkeletonJson(_atlas).ReadSkeletonData(skelPath);
                }
            }
        }

        public override string SkeletonVersion => _skeletonData.Version;

        public override bool IsAtlasLoaded => _atlas is not null;

        public override ImmutableArray<ISkin> Skins => _skins;

        public override FrozenDictionary<string, ISkin> SkinsByName => _skinsByName;

        public override float DefaultMix { get => _animationStateData.DefaultMix; set => _animationStateData.DefaultMix = value; }

        public override ImmutableArray<IAnimation> Animations => _animations;

        public override FrozenDictionary<string, IAnimation> AnimationsByName => _animationsByName;

        protected override void DisposeAtlas() => _atlas?.Dispose();

        public override ISkeleton CreateSkeleton() => new Skeleton40(new(_skeletonData), this);

        public override IAnimationState CreateAnimationState() => new AnimationState40(new(_animationStateData), this);

        public override ISkeletonClipping CreateSkeletonClipping() => new SkeletonClipping40();

        public override ISkin CreateSkin(string name) => new Skin40(name);

        public IAttachment GetAttachment(Attachment attachment) => _attachmentsMapping[attachment];
    }
}
