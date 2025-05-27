using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers;
using Spine.SpineWrappers.Attachments;
using SpineRuntime21;

namespace Spine.Implementations.SpineWrappers.V21.Attachments
{
    internal abstract class Attachment21(Attachment innerObject) : IAttachment
    {
        private readonly Attachment _o = innerObject;

        public virtual Attachment InnerObject => _o;

        public string Name => _o.Name;

        public abstract int ComputeWorldVertices(ISlot slot, ref float[] worldVertices);

        public override string ToString() => _o.ToString();
    }
}
