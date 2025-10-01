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
    internal sealed class PointAttachment36(PointAttachment innerObject) : 
        Attachment36(innerObject), 
        IPointAttachment
    {
        private readonly PointAttachment _o = innerObject;

        public override PointAttachment InnerObject => _o;

        public override int ComputeWorldVertices(ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot36 st)
            {
                if (worldVertices.Length < 2) worldVertices = new float[2];
                _o.ComputeWorldPosition(st.InnerObject.Bone, out worldVertices[0], out worldVertices[1]);
                return 2;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot36)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}