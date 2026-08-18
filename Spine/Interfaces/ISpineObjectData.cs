using Spine.Interfaces.Attachments;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Interfaces
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
        /// 是否成功加载了纹理对象
        /// </summary>
        public bool IsAtlasLoaded { get; }

        /// <summary>
        /// 所有皮肤
        /// </summary>
        public ImmutableArray<ISkin> Skins { get; }

        /// <summary>
        /// 所有皮肤按名称
        /// </summary>
        public FrozenDictionary<string, ISkin> SkinsByName { get; }

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
