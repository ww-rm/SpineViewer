using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Implementations.V40;
using Spine.Interfaces;
using Spine.Interfaces.Attachments;
using SpineRuntime40;

namespace Spine.Implementations.V40.Attachments
{
    internal sealed class ClippingAttachment40(ClippingAttachment innerObject) : 
        Attachment40(innerObject), 
        IClippingAttachment
    {
        private readonly ClippingAttachment _o = innerObject;

        public override ClippingAttachment InnerObject => _o;

        public override int ComputeWorldVertices(ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot40 st)
            {
                var length = _o.WorldVerticesLength;
                if (worldVertices.Length < length) worldVertices = new float[length];
                _o.ComputeWorldVertices(st.InnerObject, worldVertices);
                return length;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot40)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}
