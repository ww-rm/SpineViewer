using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers;
using SpineRuntime21;

namespace Spine.Implementations.SpineWrappers.V21
{
    internal sealed class Bone21(Bone innerObject, Bone21? parent = null) : IBone
    {
        private readonly Bone _o = innerObject;
        private readonly Bone21? _parent = parent;

        public Bone InnerObject => _o;

        public string Name => _o.Data.Name;
        public int Index => _o.Data.Index;

        public IBone? Parent => _parent;
        public bool Active => true; // NOTE: 3.7 及以下没有 Active 属性, 此处总是返回 true
        public float Length => _o.Data.Length;
        public float WorldX => _o.WorldX;
        public float WorldY => _o.WorldY;
        public float A => _o.M00;
        public float B => _o.M01;
        public float C => _o.M10;
        public float D => _o.M11;

        public override string ToString() => _o.ToString();
    }
}
