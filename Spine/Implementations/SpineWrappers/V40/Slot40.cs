using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Utils;
using Spine.SpineWrappers;
using SpineRuntime40;

namespace Spine.Implementations.SpineWrappers.V40
{
    internal sealed class Slot40 : ISlot
    {
        private readonly Slot _o;
        private readonly SpineObjectData40 _data;

        private readonly Bone40 _bone;
        private readonly SFML.Graphics.BlendMode _blendMode;

        public Slot40(Slot innerObject, SpineObjectData40 data, Bone40 bone)
        {
            _o = innerObject;
            _data = data;

            _bone = bone;
            _blendMode = _o.Data.BlendMode switch
            {
                BlendMode.Normal => SFMLBlendMode.NormalPma,
                BlendMode.Additive => SFMLBlendMode.AdditivePma,
                BlendMode.Multiply => SFMLBlendMode.MultiplyPma,
                BlendMode.Screen => SFMLBlendMode.ScreenPma,
                _ => throw new NotImplementedException($"{_o.Data.BlendMode}"),
            };
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
                if (value is Attachments.Attachment40 att)
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
