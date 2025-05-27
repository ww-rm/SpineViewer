using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers.Attachments;
using SpineRuntime41;

namespace Spine.Implementations.SpineWrappers.V41.Attachments
{
    internal sealed class RegionAttachment41(RegionAttachment innerObject) : 
        Attachment41(innerObject), 
        IRegionAttachment
    {
        private readonly RegionAttachment _o = innerObject;

        public override RegionAttachment InnerObject => _o;

        public override int ComputeWorldVertices(Spine.SpineWrappers.ISlot slot, ref float[] worldVertices)
        {
            if (slot is Slot41 st)
            {
                if (worldVertices.Length < 8) worldVertices = new float[8];
                _o.ComputeWorldVertices(st.InnerObject, worldVertices, 0);
                return 8;
            }
            throw new ArgumentException($"Invalid slot type. Expected {nameof(Slot41)}, but received {slot.GetType().Name}", nameof(slot));
        }

        public float R { get => _o.R; set => _o.R = value; }
        public float G { get => _o.G; set => _o.G = value; }
        public float B { get => _o.B; set => _o.B = value; }
        public float A { get => _o.A; set => _o.A = value; }

        public SFML.Graphics.Texture RendererObject => (SFML.Graphics.Texture)((AtlasRegion)_o.Region).page.rendererObject;

        public float[] UVs => _o.UVs;
    }
}
