using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Implementations.V41;
using Spine.Interfaces;
using Spine.Interfaces.Attachments;
using SpineRuntime41;

namespace Spine.Implementations.V41.Attachments
{
    internal sealed class PathAttachment41(PathAttachment innerObject) : 
        Attachment41(innerObject), 
        IPathAttachment
    {
        private readonly PathAttachment _o = innerObject;

        public override PathAttachment InnerObject => _o;

        public override int ComputeWorldVertices(ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot41 st)
            {
                var length = _o.WorldVerticesLength;
                if (worldVertices.Length < length) worldVertices = new float[length];
                _o.ComputeWorldVertices(st.InnerObject, worldVertices);
                return length;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot41)}, but received {slot.GetType().Name}", nameof(slot));
        }
    }
}