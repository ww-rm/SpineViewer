using Spine.SpineWrappers;
using Spine.SpineWrappers.Attachments;
using Spine.Utils;
using SpineRuntime21;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Implementations.SpineWrappers.V21
{
    internal sealed class SkeletonClipping21 : ISkeletonClipping
    {
        public bool IsClipping => false;

        public float[] ClippedVertices { get; private set; } = [];

        public int ClippedVerticesLength { get; private set; } = 0;

        public int[] ClippedTriangles { get; private set; } = [];

        public int ClippedTrianglesLength { get; private set; } = 0;

        public float[] ClippedUVs { get; private set; } = [];

        public void ClipEnd(ISlot slot) { }

        public void ClipEnd() { }

        public void ClipStart(ISlot slot, IClippingAttachment clippingAttachment) { }

        public void ClipTriangles(float[] vertices, int verticesLength, int[] triangles, int trianglesLength, float[] uvs)
        {
            ClippedVertices = vertices.ToArray();
            ClippedVerticesLength = verticesLength;
            ClippedTriangles = triangles.ToArray();
            ClippedTrianglesLength = trianglesLength;
            ClippedUVs = uvs.ToArray();
        }
    }
}
