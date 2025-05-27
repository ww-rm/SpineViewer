using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers;
using SpineRuntime37;

namespace Spine.Implementations.SpineWrappers.V37
{
    internal sealed class Bone37(Bone innerObject, Bone37? parent = null) : IBone
    {
        private readonly Bone _o = innerObject;
        private readonly Bone37? _parent = parent;

        public Bone InnerObject => _o;

        public string Name => _o.Data.Name;
        public int Index => _o.Data.Index;

        public IBone? Parent => _parent;
        public bool Active => true; // NOTE: 3.7 及以下没有 Active 属性, 此处总是返回 true
        public float Length => _o.Data.Length;
        public float WorldX => _o.WorldX;
        public float WorldY => _o.WorldY;
        public float A => _o.A;
        public float B => _o.B;
        public float C => _o.C;
        public float D => _o.D;

        public override string ToString() => _o.ToString();
    }
}
