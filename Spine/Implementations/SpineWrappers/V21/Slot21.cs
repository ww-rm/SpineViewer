using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Utils;
using Spine.SpineWrappers;
using SpineRuntime21;

namespace Spine.Implementations.SpineWrappers.V21
{
    internal sealed class Slot21 : ISlot
    {
        private readonly Slot _o;
        private readonly SpineObjectData21 _data;

        private readonly Bone21 _bone;
        private readonly SFML.Graphics.BlendMode _blendMode;

        public Slot21(Slot innerObject, SpineObjectData21 data, Bone21 bone)
        {
            _o = innerObject;
            _data = data;

            _bone = bone;
            _blendMode = _o.Data.AdditiveBlending ? SFMLBlendMode.AdditivePma : SFMLBlendMode.NormalPma; // NOTE: 2.1 没有完整的 BlendMode
        }

        public Slot InnerObject => _o;

        public string Name => _o.Data.Name;
        public int Index => _o.Data.Index;
        public SFML.Graphics.BlendMode Blend => _blendMode;

        public float R { get => _o.R; set => _o.R = value; }
        public float G { get => _o.G; set => _o.G = value; }
        public float B { get => _o.B; set => _o.B = value; }
        public float A { get => _o.A; set => _o.A = value; }
        public IBone Bone => _bone;

        public Spine.SpineWrappers.Attachments.IAttachment? Attachment
        { 
            get
            {
                if (_o.Attachment is Attachment att)
                {
                    return _data.SlotAttachments[Name][att.Name];
                }
                return null;
            }

            set
            {
                if (value is null)
                {
                    _o.Attachment = null;
                    return;
                }
                if (value is Attachments.Attachment21 att)
                {
                    _o.Attachment = att.InnerObject;
                    return;
                }
                throw new ArgumentException($"Received {value.GetType().Name}", nameof(value));
            }
        }

        public override string ToString() => _o.ToString();
    }
}
