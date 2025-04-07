using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Spine
{
    /// <summary>
    /// 用于在 PropertyGrid 上显示 Spine 调试属性的包装类
    /// </summary>
    public class SpineDebugWrapper(SpineViewer.Spine.Spine spine)
    {
        [Browsable(false)]
        public SpineViewer.Spine.Spine Spine { get; } = spine;

        /// <summary>
        /// 显示纹理
        /// </summary>
        [DisplayName("纹理")]
        public bool DebugTexture { get => Spine.DebugTexture; set => Spine.DebugTexture = value; }

        /// <summary>
        /// 显示包围盒
        /// </summary>
        [DisplayName("包围盒")]
        public bool DebugBounds { get => Spine.DebugBounds; set => Spine.DebugBounds = value; }

        /// <summary>
        /// 显示骨骼
        /// </summary>
        [DisplayName("骨架")]
        public bool DebugBones { get => Spine.DebugBones; set => Spine.DebugBones = value; }
    }
}
