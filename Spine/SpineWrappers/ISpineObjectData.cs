using Spine.SpineWrappers.Attachments;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers
{
    /// <summary>
    /// 对 SkeletonData 和 AnimationStateData 的访问封装
    /// </summary>
    public interface ISpineObjectData
    {
        /// <summary>
        /// skel 文件版本
        /// </summary>
        public string SkeletonVersion { get; }

        /// <summary>
        /// 所有皮肤
        /// </summary>
        public ImmutableArray<ISkin> Skins { get; }

        /// <summary>
        /// 所有皮肤按名称
        /// </summary>
        public FrozenDictionary<string, ISkin> SkinsByName { get; }

        /// <summary>
        /// 所有皮肤中所有插槽的可用附件集合, 并不保证所有插槽均在此处有键
        /// </summary>
        public FrozenDictionary<string, FrozenDictionary<string, IAttachment>> SlotAttachments { get; }

        /// <summary>
        /// 所有动画
        /// </summary>
        public ImmutableArray<IAnimation> Animations { get; }

        /// <summary>
        /// 所有动画按名称
        /// </summary>
        public FrozenDictionary<string, IAnimation> AnimationsByName { get; }

        /// <summary>
        /// 默认的动画过渡时长
        /// </summary>
        public float DefaultMix { get; set; }
    }
}
