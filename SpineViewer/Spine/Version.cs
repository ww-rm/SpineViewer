using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    public static class VersionHelper
    {
        /// <summary>
        /// 描述缓存
        /// </summary>
        public static readonly Dictionary<Version, string> Versions = [];

        static VersionHelper()
        {
            // 初始化缓存
            foreach (var value in Enum.GetValues(typeof(Version)))
            {
                var field = typeof(Version).GetField(value.ToString());
                var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
                Versions[(Version)value] = attribute?.Description ?? value.ToString();
            }
        }

        /// <summary>
        /// 版本号字符串
        /// </summary>
        public static string String(this Version version)
        {
            return Versions.TryGetValue(version, out var description) ? description : version.ToString();
        }
    }

    /// <summary>
    /// 支持的 Spine 版本
    /// </summary>
    public enum Version
    {
        [Description("2.1.x")] V21 = 0x0201,
        [Description("3.6.x")] V36 = 0x0306,
        [Description("3.7.x")] V37 = 0x0307,
        [Description("3.8.x")] V38 = 0x0308,
        [Description("4.0.x")] V40 = 0x0400,
        [Description("4.1.x")] V41 = 0x0401,
        [Description("4.2.x")] V42 = 0x0402,
        [Description("4.3.x")] V43 = 0x0403,
    }
}
