using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Implementations.V38;
using Spine.Interfaces;
using Spine.Interfaces.Attachments;
using SpineRuntime38;
using SpineRuntime38.Attachments;

namespace Spine.Implementations.V38.Attachments
{
    internal sealed class PointAttachment38(PointAttachment innerObject) : 
        Attachment38(innerObject), 
        IPointAttachment
    {
        private readonly PointAttachment _o = innerObject;

        public override PointAttachment InnerObject => _o;

        public override int ComputeWorldVertices(ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot38 st)
            {
                if (worldVertices.Length < 2) worldVertices = new float[2];
                _o.ComputeWorldPosition(st.InnerObject.Bone, out worldVertices[0], out worldVertices[1]);
                return 2;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot38)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}