using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers.Attachments
{
    public interface IRegionAttachment : IAttachment
    {
        /// <summary>
        /// 总是将 Region 附件矩形区域切分成两个这样的三角形
        /// </summary>
        private static readonly int[] _trangles = [0, 1, 2, 2, 3, 0];

        /// <summary>
        /// R
        /// </summary>
        public float R { get; set; }

        /// <summary>
        /// G
        /// </summary>
        public float G { get; set; }

        /// <summary>
        /// B
        /// </summary>
        public float B { get; set; }

        /// <summary>
        /// A
        /// </summary>
        public float A { get; set; }

        /// <summary>
        /// 用于渲染的纹理对象
        /// </summary>
        public SFML.Graphics.Texture RendererObject { get; }

        /// <summary>
        /// 顶点纹理坐标, 每个坐标有 u 和 v 两个数, 有效长度和 <see cref="IAttachment.ComputeWorldVertices(ISlot, ref float[])"/> 返回值一致
        /// </summary>
        public float[] UVs { get; }

        /// <summary>
        /// 三角形索引顶点数组, 每 3 个为一组, 指向顶点的下标 (不是顶点坐标数组下标)
        /// </summary>
        public int[] Triangles => _trangles;
    }
}
