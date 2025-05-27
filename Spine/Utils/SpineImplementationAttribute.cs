using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Utils
{
    /// <summary>
    /// Spine 实现类标记
    /// </summary>
    /// <param name="major">主版本号</param>
    /// <param name="minor">次版本号</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SpineImplementationAttribute(int major, int minor) : Attribute, IImplementationKeyAttribute<string>
    {
        /// <summary>
        /// 与 <c><see cref="SpineVersion.Tag"/></c> 相同格式的字符串
        /// </summary>
        public string ImplementationKey { get; } = $"{major}.{minor}";
    }
}
