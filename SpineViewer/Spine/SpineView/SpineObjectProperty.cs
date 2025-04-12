using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineViewer.Spine;

namespace SpineViewer.Spine.SpineView
{
    public class SpineObjectProperty(SpineObject spine)
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        [DisplayName("基本信息")]
        public SpineBaseInfoProperty BaseInfo { get; } = new(spine);

        [DisplayName("渲染")]
        public SpineRenderProperty Render { get; } = new(spine);

        [DisplayName("变换")]
        public SpineTransformProperty Transform { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("皮肤")]
        public SpineSkinProperty Skin { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("动画")]
        public SpineAnimationProperty Animation { get; } = new(spine);

        [DisplayName("调试")]
        public SpineDebugProperty Debug { get; } = new(spine);
    }
}
