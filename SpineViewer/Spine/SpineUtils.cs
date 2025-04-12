using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    /// <summary>
    /// Spine 版本静态辅助类
    /// </summary>
    public static class SpineUtils
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

        static SpineUtils()
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
        /// 常规骨骼文件后缀集合
        /// </summary>
        public static readonly ImmutableHashSet<string> CommonSkelSuffix = [".skel", ".json"];

        /// <summary>
        /// 尝试检测骨骼文件版本
        /// </summary>
        /// <param name="skelPath"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static SpineVersion GetVersion(string skelPath)
        {
            string versionString = null;
            using var input = File.OpenRead(skelPath);
            var reader = new SkeletonConverter.BinaryReader(input);

            // try json format
            try
            {
                if (JsonNode.Parse(input) is JsonObject root && root.TryGetPropertyValue("skeleton", out var node) &&
                    node is JsonObject _skeleton && _skeleton.TryGetPropertyValue("spine", out var _version))
                    versionString = (string)_version;
            }
            catch { }

            // try v4 binary format
            if (versionString is null)
            {
                try
                {
                    input.Position = 0;
                    var hash = reader.ReadLong();
                    var versionPosition = input.Position;
                    var versionByteCount = reader.ReadVarInt();
                    input.Position = versionPosition;
                    if (versionByteCount <= 13)
                        versionString = reader.ReadString();
                }
                catch { }
            }

            // try v3 binary format
            if (versionString is null)
            {
                try
                {
                    input.Position = 0;
                    var hash = reader.ReadString();
                    versionString = reader.ReadString();
                }
                catch { }
            }

            if (versionString is null)
                throw new InvalidDataException($"No verison detected: {skelPath}");

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
