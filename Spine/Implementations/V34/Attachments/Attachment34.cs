using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Interfaces;
using Spine.Interfaces.Attachments;
using SpineRuntime34;

namespace Spine.Implementations.V34.Attachments
{
    internal abstract class Attachment34(Attachment innerObject) : IAttachment
    {
        private readonly Attachment _o = innerObject;

        public virtual Attachment InnerObject => _o;

        public string Name => _o.Name;

        public abstract int ComputeWorldVertices(ISlot slot, ref float[] worldVertices);

        public override string ToString() => _o.ToString();
    }
}
