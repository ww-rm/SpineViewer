using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers
{
    public interface IBone
    {
        /// <summary>
        /// 骨骼唯一名字
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 骨骼索引
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Active { get; }

        /// <summary>
        /// 父骨骼
        /// </summary>
        public IBone? Parent { get; }

        /// <summary>
        /// 骨骼长度
        /// </summary>
        public float Length { get; }

        /// <summary>
        /// 世界坐标 X
        /// </summary>
        public float WorldX { get; }

        /// <summary>
        /// 世界坐标 Y
        /// </summary>
        public float WorldY { get; }

        /// <summary>
        /// <c>Cos(theta)</c>
        /// </summary>
        public float A { get; }

        /// <summary>
        /// <c>-Sin(theta)</c>
        /// </summary>
        public float B { get; }

        /// <summary>
        /// <c>Sin(theta)</c>
        /// </summary>
        public float C { get; }

        /// <summary>
        /// <c>Cos(theta)</c>
        /// </summary>
        public float D { get; }
    }
}
