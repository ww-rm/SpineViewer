using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers.Attachments
{
    public interface IAttachment
    {
        /// <summary>
        /// 附件的唯一名字
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 计算世界顶点数组
        /// </summary>
        /// <param name="slot">装载的插槽</param>
        /// <param name="worldVertices">顶点缓冲数组, 如果大小不足会重分配到合适的大小, 实际长度需要通过返回值获取</param>
        /// <returns><paramref name="worldVertices"/> 的实际长度, 顶点数是长度除以 2</returns>
        public int ComputeWorldVertices(ISlot slot, ref float[] worldVertices);
    }
}
