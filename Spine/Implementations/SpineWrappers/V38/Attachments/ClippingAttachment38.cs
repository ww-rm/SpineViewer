using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers.Attachments;
using SpineRuntime38;
using SpineRuntime38.Attachments;

namespace Spine.Implementations.SpineWrappers.V38.Attachments
{
    internal sealed class ClippingAttachment38(ClippingAttachment innerObject) : 
        Attachment38(innerObject), 
        IClippingAttachment
    {
        private readonly ClippingAttachment _o = innerObject;

        public override ClippingAttachment InnerObject => _o;

        public override int ComputeWorldVertices(Spine.SpineWrappers.ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot38 st)
            {
                var length = _o.WorldVerticesLength;
                if (worldVertices.Length < length) worldVertices = new float[length];
                _o.ComputeWorldVertices(st.InnerObject, worldVertices);
                return length;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot38)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}
