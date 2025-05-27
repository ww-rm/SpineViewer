using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers.Attachments;
using SpineRuntime21;

namespace Spine.Implementations.SpineWrappers.V21.Attachments
{
    internal sealed class BoundingBoxAttachment21(BoundingBoxAttachment innerObject) : 
        Attachment21(innerObject), 
        IBoundingBoxAttachment
    {
        private readonly BoundingBoxAttachment _o = innerObject;

        public override BoundingBoxAttachment InnerObject => _o;

        public override int ComputeWorldVertices(Spine.SpineWrappers.ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot21 st)
            {
                var length = _o.Vertices.Length;
                if (worldVertices.Length < length) worldVertices = new float[length];
                _o.ComputeWorldVertices(st.InnerObject.Bone, worldVertices);
                return length;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot21)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}
