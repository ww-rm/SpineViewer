using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers;
using SpineRuntime42;

namespace Spine.Implementations.SpineWrappers.V42
{
    internal sealed class Skin42 : ISkin
    {
        private readonly Skin _o;

        /// <summary>
        /// 使用指定名字创建空皮肤
        /// </summary>
        public Skin42(string name) => _o = new(name);

        /// <summary>
        /// 包装已有皮肤对象
        /// </summary>
        public Skin42(Skin innerObject) => _o = innerObject;

        public Skin InnerObject => _o;

        public string Name => _o.Name;

        public void AddSkin(ISkin skin)
        {
            if (skin is Skin42 sk)
            {
                _o.AddSkin(sk._o);
                return;
            }
            throw new ArgumentException($"Received {skin.GetType().Name}", nameof(skin));
        }

        public void Clear() => _o.Clear();

        public override string ToString() => _o.ToString();
    }
}
