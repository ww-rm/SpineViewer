using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    /// <summary>
    /// 支持的 Spine 版本
    /// </summary>
    public enum SpineVersion
    {
        [Description("<Auto>")] Auto = 0x0000,
        [Description("2.1.x")] V21 = 0x0201,
        [Description("3.6.x")] V36 = 0x0306,
        [Description("3.7.x")] V37 = 0x0307,
        [Description("3.8.x")] V38 = 0x0308,
        [Description("4.0.x")] V40 = 0x0400,
        [Description("4.1.x")] V41 = 0x0401,
        [Description("4.2.x")] V42 = 0x0402,
        [Description("4.3.x")] V43 = 0x0403,
    }

    /// <summary>
    /// Spine 实现类标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SpineImplementationAttribute(SpineVersion version) : Attribute, IImplementationKey<SpineVersion>
    {
        public SpineVersion ImplementationKey { get; private set; } = version;
    }

    /// <summary>
    /// Spine 版本静态辅助类
    /// </summary>
    public static class SpineHelper
    {
        /// <summary>
        /// 版本名称
        /// </summary>
        public static readonly ReadOnlyDictionary<SpineVersion, string> Names;
        private static readonly Dictionary<SpineVersion, string> names = [];

        /// <summary>
        /// Runtime 版本字符串
        /// </summary>
        private static readonly Dictionary<SpineVersion, string> runtimes = [];

        static SpineHelper()
        {
            // 初始化缓存
            foreach (var value in Enum.GetValues(typeof(SpineVersion)))
            {
                var field = typeof(SpineVersion).GetField(value.ToString());
                var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
                names[(SpineVersion)value] = attribute?.Description ?? value.ToString();
            }
            Names = names.AsReadOnly();

            runtimes[SpineVersion.V21] = Assembly.GetAssembly(typeof(SpineRuntime21.Skeleton)).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            runtimes[SpineVersion.V36] = Assembly.GetAssembly(typeof(SpineRuntime36.Skeleton)).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            runtimes[SpineVersion.V37] = Assembly.GetAssembly(typeof(SpineRuntime37.Skeleton)).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            runtimes[SpineVersion.V38] = Assembly.GetAssembly(typeof(SpineRuntime38.Skeleton)).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            runtimes[SpineVersion.V40] = Assembly.GetAssembly(typeof(SpineRuntime40.Skeleton)).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            runtimes[SpineVersion.V41] = Assembly.GetAssembly(typeof(SpineRuntime41.Skeleton)).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            runtimes[SpineVersion.V42] = Assembly.GetAssembly(typeof(SpineRuntime42.Skeleton)).GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        }

        /// <summary>
        /// 版本字符串名称
        /// </summary>
        public static string GetName(this SpineVersion version)
        {
            return Names.TryGetValue(version, out var val) ? val : version.ToString();
        }

        /// <summary>
        /// Runtime 版本字符串名称
        /// </summary>
        public static string GetRuntime(this SpineVersion version)
        {
            return runtimes.TryGetValue(version, out var val) ? val : GetName(version);
        }

        /// <summary>
        /// 获取字符串对应的版本号
        /// </summary>
        /// <param name="versionString"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static SpineVersion GetVersion(string versionString)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(versionString);
            if (versionString.StartsWith("2.1.")) return SpineVersion.V21;
            else if (versionString.StartsWith("3.6.")) return SpineVersion.V36;
            else if (versionString.StartsWith("3.7.")) return SpineVersion.V37;
            else if (versionString.StartsWith("3.8.")) return SpineVersion.V38;
            else if (versionString.StartsWith("4.0.")) return SpineVersion.V40;
            else if (versionString.StartsWith("4.1.")) return SpineVersion.V41;
            else if (versionString.StartsWith("4.2.")) return SpineVersion.V42;
            else if (versionString.StartsWith("4.3.")) return SpineVersion.V43;
            else throw new InvalidDataException($"Unknown verison: {versionString}");
        }
    }
}
