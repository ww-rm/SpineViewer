using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Implementations.V36;
using Spine.Interfaces;
using Spine.Interfaces.Attachments;
using SpineRuntime36;

namespace Spine.Implementations.V36.Attachments
{
    internal sealed class ClippingAttachment36(ClippingAttachment innerObject) : 
        Attachment36(innerObject), 
        IClippingAttachment
    {
        private readonly ClippingAttachment _o = innerObject;

        public override ClippingAttachment InnerObject => _o;

        public override int ComputeWorldVertices(ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot36 st)
            {
                var length = _o.WorldVerticesLength;
                if (worldVertices.Length < length) worldVertices = new float[length];
                _o.ComputeWorldVertices(st.InnerObject, worldVertices);
                return length;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot36)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}
