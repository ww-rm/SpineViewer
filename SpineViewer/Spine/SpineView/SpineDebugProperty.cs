using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineViewer.Spine;

namespace SpineViewer.Spine.SpineView
{
    /// <summary>
    /// 用于在 PropertyGrid 上显示 Spine 调试属性的包装类
    /// </summary>
    public class SpineDebugProperty(SpineObject spine)
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>
        /// 显示纹理
        /// </summary>
        [DisplayName("Texture")]
        public bool DebugTexture { get => Spine.DebugTexture; set => Spine.DebugTexture = value; }

        /// <summary>
        /// 显示包围盒
        /// </summary>
        [DisplayName("Bounds")]
        public bool DebugBounds { get => Spine.DebugBounds; set => Spine.DebugBounds = value; }

        /// <summary>
        /// 显示骨骼
        /// </summary>
        [DisplayName("Bones")]
        public bool DebugBones { get => Spine.DebugBones; set => Spine.DebugBones = value; }

        /// <summary>
        /// 显示区域附件边框线
        /// </summary>
        [DisplayName("Regions")]
        public bool DebugRegions { get => Spine.DebugRegions; set => Spine.DebugRegions = value; }

        /// <summary>
        /// 显示网格附件边框线
        /// </summary>
        [DisplayName("MeshHulls")]
        public bool DebugMeshHulls { get => Spine.DebugMeshHulls; set => Spine.DebugMeshHulls = value; }

        /// <summary>
        /// 显示网格附件网格线
        /// </summary>
        [DisplayName("Meshes")]
        public bool DebugMeshes { get => Spine.DebugMeshes; set => Spine.DebugMeshes = value; }

        ///// <summary>
        ///// 显示碰撞盒附件边框线
        ///// </summary>
        //[DisplayName("BoudingBoxes")]
        //public bool DebugBoundingBoxes { get => Spine.DebugBoundingBoxes; set => Spine.DebugBoundingBoxes = value; }

        ///// <summary>
        ///// 显示路径附件网格线
        ///// </summary>
        //[DisplayName("Paths")]
        //public bool DebugPaths { get => Spine.DebugPaths; set => Spine.DebugPaths = value; }

        /// <summary>
        /// 显示剪裁附件网格线
        /// </summary>
        [DisplayName("Clippings")]
        public bool DebugClippings { get => Spine.DebugClippings; set => Spine.DebugClippings = value; }
    }
}
