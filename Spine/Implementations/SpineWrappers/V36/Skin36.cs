using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers;
using SpineRuntime36;

namespace Spine.Implementations.SpineWrappers.V36
{
    internal sealed class Skin36 : ISkin
    {
        private readonly Skin _o;

        /// <summary>
        /// 使用指定名字创建空皮肤
        /// </summary>
        public Skin36(string name) => _o = new(name);

        /// <summary>
        /// 包装已有皮肤对象
        /// </summary>
        public Skin36(Skin innerObject) => _o = innerObject;

        public Skin InnerObject => _o;

        public string Name => _o.Name;

        public void AddSkin(ISkin skin)
        {
            if (skin is Skin36 sk)
            {
                // NOTE: 3.7 及以下不支持 AddSkin
                foreach (var (k, v) in sk._o.Attachments)
                    _o.AddAttachment(k.slotIndex, k.name, v);
                return;
            }
            throw new ArgumentException($"Received {skin.GetType().Name}", nameof(skin));
        }

        public void Clear() => _o.Attachments.Clear();

        public override string ToString() => _o.ToString();
    }
}
