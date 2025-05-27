using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers.Attachments;
using SpineRuntime37;

namespace Spine.Implementations.SpineWrappers.V37.Attachments
{
    internal sealed class PointAttachment37(PointAttachment innerObject) : 
        Attachment37(innerObject), 
        IPointAttachment
    {
        private readonly PointAttachment _o = innerObject;

        public override PointAttachment InnerObject => _o;

        public override int ComputeWorldVertices(Spine.SpineWrappers.ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot37 st)
            {
                if (worldVertices.Length < 2) worldVertices = new float[2];
                _o.ComputeWorldPosition(st.InnerObject.Bone, out worldVertices[0], out worldVertices[1]);
                return 2;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot37)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}