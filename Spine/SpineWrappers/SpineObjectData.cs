using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers.Attachments;

namespace Spine.SpineWrappers
{
    /// <summary>
    /// 应当继承该类实现多版本, 子类需要提供签名为 <c><see cref="new(string, string)"/></c> 的构造函数
    /// </summary>
    public abstract class SpineObjectData : 
        Utils.ImplementationResolver<SpineObjectData, Utils.SpineImplementationAttribute, string>, 
        ISpineObjectData, 
        IDisposable
    {
        /// <summary>
        /// 构建版本对象
        /// </summary>
        public static SpineObjectData New(SpineVersion version, string skelPath, string atlasPath) => CreateInstance(version.Tag, skelPath, atlasPath);

        /// <summary>
        /// 纹理加载器, 可以设置一些预置参数
        /// </summary>
        public static TextureLoader TextureLoader => _textureLoader;
        protected static readonly TextureLoader _textureLoader = new();

        /// <summary>
        /// 构造函数, 继承的子类应当实现一个相同签名的构造函数
        /// </summary>
        public SpineObjectData(string skelPath, string atlasPath) { }

        public abstract string SkeletonVersion { get; }

        public abstract ImmutableArray<ISkin> Skins { get; }

        public abstract FrozenDictionary<string, ISkin> SkinsByName { get; }

        public abstract FrozenDictionary<string, FrozenDictionary<string, IAttachment>> SlotAttachments { get; }

        public abstract ImmutableArray<IAnimation> Animations { get; }

        public abstract FrozenDictionary<string, IAnimation> AnimationsByName { get; }

        public abstract float DefaultMix { get; set; }

        /// <summary>
        /// 释放纹理资源
        /// </summary>
        protected abstract void DisposeAtlas();

        /// <summary>
        /// 创建 skeleton
        /// </summary>
        /// <returns></returns>
        public abstract ISkeleton CreateSkeleton();

        /// <summary>
        /// 创建 animationState
        /// </summary>
        public abstract IAnimationState CreateAnimationState();

        /// <summary>
        /// 创建 skeletonClipping
        /// </summary>
        public abstract ISkeletonClipping CreateSkeletonClipping();

        /// <summary>
        /// 创建空皮肤
        /// </summary>
        public abstract ISkin CreateSkin(string name);

        #region IDispose 接口实现

        /// <summary>
        /// 引用计数, 初始为 1, 每调用一次 Dispose 会减 1, 由于可能被多个实例同时引用, 因此直到小于等于 0 才会真正释放资源
        /// </summary>
        private int _refCount = 1;

        /// <summary>
        /// 增加引用计数, 当使用同一份数据创建实例副本时, 调用方负责使用该方法增加引用计数, 并且使用 Dispose 或者 Finalize 减少引用计数, 初始为 1
        /// </summary>
        public void IncRef() => _refCount++;

        protected virtual void Dispose(bool disposing)
        {
            if (_refCount <= 0) return;

            _refCount--;
            if (_refCount <= 0)
            {
                if (disposing)
                {
                    DisposeAtlas();
                }
            }
        }

        ~SpineObjectData()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            if (_refCount <= 0)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
