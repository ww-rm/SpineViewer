using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineViewer.Spine;
using SpineViewer.Utils;
using SpineViewer.Utils.Localize;

namespace SpineViewer.Spine.SpineView
{
    /// <summary>
    /// 用于在 PropertyGrid 上显示 Spine 空间变换的包装类
    /// </summary>
    public class SpineTransformProperty(SpineObject spine)
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

		/// <summary>
		/// 缩放比例
		/// </summary>
		[LocalizedDisplayName(typeof(Properties.Resources), "scale")]
        public float Scale { get => Spine.Scale; set => Spine.Scale = value; }

        /// <summary>
        /// 位置
        /// </summary>
        [TypeConverter(typeof(PointFConverter))]
		[LocalizedDisplayName(typeof(Properties.Resources), "position")]
		public PointF Position { get => Spine.Position; set => Spine.Position = value; }

		/// <summary>
		/// 水平翻转
		/// </summary>
		[LocalizedDisplayName(typeof(Properties.Resources), "flipX")]
		public bool FlipX { get => Spine.FlipX; set => Spine.FlipX = value; }

		/// <summary>
		/// 垂直翻转
		/// </summary>
		[LocalizedDisplayName(typeof(Properties.Resources), "flipY")]
		public bool FlipY { get => Spine.FlipY; set => Spine.FlipY = value; }
    }
}
