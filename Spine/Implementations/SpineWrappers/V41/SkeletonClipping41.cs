using Spine.SpineWrappers;
using Spine.SpineWrappers.Attachments;
using Spine.Utils;
using SpineRuntime41;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Implementations.SpineWrappers.V41
{
    internal sealed class SkeletonClipping41 : ISkeletonClipping
    {
        private readonly SkeletonClipping _o = new();

        public bool IsClipping => _o.IsClipping;

        public float[] ClippedVertices => _o.ClippedVertices.Items;

        public int ClippedVerticesLength => _o.ClippedVertices.Count;

        public int[] ClippedTriangles => _o.ClippedTriangles.Items;

        public int ClippedTrianglesLength => _o.ClippedTriangles.Count;

        public float[] ClippedUVs => _o.ClippedUVs.Items;

        public void ClipTriangles(float[] vertices, int verticesLength, int[] triangles, int trianglesLength, float[] uvs)
            => _o.ClipTriangles(vertices, verticesLength, triangles, trianglesLength, uvs);

        public void ClipStart(ISlot slot, IClippingAttachment clippingAttachment)
        {
            if (slot is Slot41 st && clippingAttachment is Attachments.ClippingAttachment41 att)
            {
                _o.ClipStart(st.InnerObject, att.InnerObject);
                return;
            }
            throw new ArgumentException($"Received {slot.GetType().Name} {clippingAttachment.GetType().Name}");
        }

        public void ClipEnd(ISlot slot)
        {
            if (slot is Slot41 st)
            {
                _o.ClipEnd(st.InnerObject);
                return;
            }
            throw new ArgumentException($"Received {slot.GetType().Name}", nameof(slot));
        }

        public void ClipEnd() => _o.ClipEnd();

        public override string ToString() => _o.ToString();
    }
}
