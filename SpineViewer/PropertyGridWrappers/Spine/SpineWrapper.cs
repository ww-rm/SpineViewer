using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Spine
{
    public class SpineWrapper(SpineViewer.Spine.Spine spine)
    {
        [Browsable(false)]
        public SpineViewer.Spine.Spine Spine { get; } = spine;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("基本信息")]
        public SpineBaseInfoWrapper BaseInfo { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("渲染")]
        public SpineRenderWrapper Render { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("变换")]
        public SpineTransformWrapper Transform { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("皮肤")]
        public SpineSkinWrapper Skin { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("动画")]
        public SpineAnimationWrapper Animation { get; } = new(spine);
    }
}
