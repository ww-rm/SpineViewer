using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spine.SpineWrappers.Attachments;

namespace Spine.SpineWrappers
{
    public interface ISlot
    {
        /// <summary>
        /// 插槽唯一名字
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 插槽唯一索引
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 混合模式
        /// </summary>
        public SFML.Graphics.BlendMode Blend { get; }

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
        /// 所在骨骼
        /// </summary>
        public IBone Bone { get; }

        /// <summary>
        /// 使用的附件, 可以设置为 null 清空附件
        /// </summary>
        public IAttachment? Attachment { get; set; }
    }
}
