using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineViewer.Spine;
using SpineViewer.Utils;

namespace SpineViewer.PropertyGridWrappers.Spine
{
    /// <summary>
    /// 用于在 PropertyGrid 上显示 Spine 空间变换的包装类
    /// </summary>
    public class SpineTransformWrapper(SpineObject spine)
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>
        /// 缩放比例
        /// </summary>
        [DisplayName("缩放比例")]
        public float Scale { get => Spine.Scale; set => Spine.Scale = value; }

        /// <summary>
        /// 位置
        /// </summary>
        [TypeConverter(typeof(PointFConverter))]
        [DisplayName("位置")]
        public PointF Position { get => Spine.Position; set => Spine.Position = value; }

        /// <summary>
        /// 水平翻转
        /// </summary>
        [DisplayName("水平翻转")]
        public bool FlipX { get => Spine.FlipX; set => Spine.FlipX = value; }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        [DisplayName("垂直翻转")]
        public bool FlipY { get => Spine.FlipY; set => Spine.FlipY = value; }
    }
}
