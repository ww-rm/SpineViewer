using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineViewer.Spine;
using SpineViewer.Utils.Localize;

namespace SpineViewer.Spine.SpineView
{
    public class SpineObjectProperty(SpineObject spine)
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

		[LocalizedDisplayName(typeof(Properties.Resources), "basicInfo")]
        public SpineBaseInfoProperty BaseInfo { get; } = new(spine);

		[LocalizedDisplayName(typeof(Properties.Resources), "render")]
		public SpineRenderProperty Render { get; } = new(spine);

		[LocalizedDisplayName(typeof(Properties.Resources), "transform")]
		public SpineTransformProperty Transform { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
		[LocalizedDisplayName(typeof(Properties.Resources), "skin")]
		public SpineSkinProperty Skin { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
		[LocalizedDisplayName(typeof(Properties.Resources), "slot")]
		public SpineSlotProperty Slot { get; } = new(spine);

        [TypeConverter(typeof(ExpandableObjectConverter))]
		[LocalizedDisplayName(typeof(Properties.Resources), "animation")]
		public SpineAnimationProperty Animation { get; } = new(spine);

		[LocalizedDisplayName(typeof(Properties.Resources), "debug")]
		public SpineDebugProperty Debug { get; } = new(spine);
    }
}
