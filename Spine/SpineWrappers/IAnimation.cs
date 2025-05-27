using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers
{
    public interface IAnimation
    {
        /// <summary>
        /// 动画名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 动画时长
        /// </summary>
        public float Duration { get; }
    }
}
