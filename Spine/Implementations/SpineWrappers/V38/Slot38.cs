using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.Utils;
using Spine.SpineWrappers;
using SpineRuntime38;
using SpineRuntime38.Attachments;

namespace Spine.Implementations.SpineWrappers.V38
{
    internal sealed class Slot38 : ISlot
    {
        private readonly Slot _o;
        private readonly SpineObjectData38 _data;

        private readonly Bone38 _bone;
        private readonly SFML.Graphics.BlendMode _blendMode;

        public Slot38(Slot innerObject, SpineObjectData38 data, Bone38 bone)
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
                if (value is Attachments.Attachment38 att)
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
