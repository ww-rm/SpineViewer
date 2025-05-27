using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers.Attachments;
using SpineRuntime21;

namespace Spine.Implementations.SpineWrappers.V21.Attachments
{
    internal sealed class RegionAttachment21(RegionAttachment innerObject) : 
        Attachment21(innerObject), 
        IRegionAttachment
    {
        private readonly RegionAttachment _o = innerObject;

        public override RegionAttachment InnerObject => _o;

        public override int ComputeWorldVertices(Spine.SpineWrappers.ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot21 st)
            {
                if (worldVertices.Length < 8) worldVertices = new float[8];
                _o.ComputeWorldVertices(st.InnerObject.Bone, worldVertices);
                return 8;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot21)}, but received {slot.GetType().Name}", nameof(slot));
        }

        public float R { get => _o.R; set => _o.R = value; }
        public float G { get => _o.G; set => _o.G = value; }
        public float B { get => _o.B; set => _o.B = value; }
        public float A { get => _o.A; set => _o.A = value; }

        public SFML.Graphics.Texture RendererObject => (SFML.Graphics.Texture)((AtlasRegion)_o.RendererObject).page.rendererObject;

        public float[] UVs => _o.UVs;
    }
}
