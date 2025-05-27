using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers.Attachments;
using SpineRuntime42;

namespace Spine.Implementations.SpineWrappers.V42.Attachments
{
    internal sealed class MeshAttachment42(MeshAttachment innerObject) : 
        Attachment42(innerObject), 
        IMeshAttachment
    {
        private readonly MeshAttachment _o = innerObject;

        public override MeshAttachment InnerObject => _o;

        public override int ComputeWorldVertices(Spine.SpineWrappers.ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot42 st)
            {
                var length = _o.WorldVerticesLength;
                if (worldVertices.Length < length) worldVertices = new float[length];
                _o.ComputeWorldVertices(st.InnerObject, worldVertices);
                return length;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot42)}, but received {slot.GetType().Name}", nameof(slot));
        }

        public float R { get => _o.R; set => _o.R = value; }
        public float G { get => _o.G; set => _o.G = value; }
        public float B { get => _o.B; set => _o.B = value; }
        public float A { get => _o.A; set => _o.A = value; }

        public SFML.Graphics.Texture RendererObject => (SFML.Graphics.Texture)((AtlasRegion)_o.Region).page.rendererObject;

        public float[] UVs => _o.UVs;

        public int[] Triangles => _o.Triangles;

        public int HullLength => _o.HullLength;
    }
}
