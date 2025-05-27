using Spine.SpineWrappers.Attachments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers
{
    public interface ISkeletonClipping
    {
        /// <summary>
        /// 是否正处于裁剪
        /// </summary>
        public bool IsClipping { get; }

        /// <summary>
        /// 裁剪后的顶点数组
        /// </summary>
        public float[] ClippedVertices { get; }

        /// <summary>
        /// 裁剪后的顶点数组 <see cref="ClippedVertices"/> 长度
        /// </summary>
        public int ClippedVerticesLength { get; }

        /// <summary>
        /// 裁剪后的三角形索引数组
        /// </summary>
        public int[] ClippedTriangles { get; }

        /// <summary>
        /// 裁剪后的三角形索引数组 <see cref="ClippedTriangles"/> 长度
        /// </summary>
        public int ClippedTrianglesLength { get; }

        /// <summary>
        /// 裁剪后的 UV 数组
        /// </summary>
        public float[] ClippedUVs { get; }

        /// <summary>
        /// 进行裁剪, 裁剪后的结果通过属性获取
        /// </summary>
        public void ClipTriangles(float[] vertices, int verticesLength, int[] triangles, int trianglesLength, float[] uvs);

        /// <summary>
        /// 开始裁剪
        /// </summary>
        public void ClipStart(ISlot slot, IClippingAttachment clippingAttachment);

        /// <summary>
        /// 判断输入插槽是否需要结束裁剪并结束裁剪
        /// </summary>
        public void ClipEnd(ISlot slot);

        /// <summary>
        /// 结束裁剪
        /// </summary>
        public void ClipEnd();
    }
}
