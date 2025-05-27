using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.SpineWrappers
{
    public interface ISkin
    {
        /// <summary>
        /// 皮肤名字
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 添加其他皮肤
        /// </summary>
        public void AddSkin(ISkin skin);

        /// <summary>
        /// 清空皮肤内容
        /// </summary>
        public void Clear();
    }
}
