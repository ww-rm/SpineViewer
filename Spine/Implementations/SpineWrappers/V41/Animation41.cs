using Spine.SpineWrappers;
using SpineRuntime41;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Implementations.SpineWrappers.V41
{
    internal sealed class Animation41(Animation innerObject) : IAnimation
    {
        private readonly Animation _o = innerObject;

        public Animation InnerObject => _o;

        public string Name => _o.Name;

        public float Duration => _o.Duration;

        public override string ToString() => _o.ToString();
    }
}
