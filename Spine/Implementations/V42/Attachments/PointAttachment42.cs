using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Implementations.V42;
using Spine.Interfaces;
using Spine.Interfaces.Attachments;
using SpineRuntime42;

namespace Spine.Implementations.V42.Attachments
{
    internal sealed class PointAttachment42(PointAttachment innerObject) : 
        Attachment42(innerObject), 
        IPointAttachment
    {
        private readonly PointAttachment _o = innerObject;

        public override PointAttachment InnerObject => _o;

        public override int ComputeWorldVertices(ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot42 st)
            {
                if (worldVertices.Length < 2) worldVertices = new float[2];
                _o.ComputeWorldPosition(st.InnerObject.Bone, out worldVertices[0], out worldVertices[1]);
                return 2;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot42)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}