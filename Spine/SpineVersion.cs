using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Spine
{
    /// <summary>
    /// Spine 运行时版本标识类
    /// </summary>
    public sealed class SpineVersion : IEquatable<SpineVersion>, IComparable<SpineVersion>
    {
        public static readonly SpineVersion V21 = new(typeof(SpineRuntime21.Skeleton));
        public static readonly SpineVersion V36 = new(typeof(SpineRuntime36.Skeleton));
        public static readonly SpineVersion V37 = new(typeof(SpineRuntime37.Skeleton));
        public static readonly SpineVersion V38 = new(typeof(SpineRuntime38.Skeleton));
        public static readonly SpineVersion V40 = new(typeof(SpineRuntime40.Skeleton));
        public static readonly SpineVersion V41 = new(typeof(SpineRuntime41.Skeleton));
        public static readonly SpineVersion V42 = new(typeof(SpineRuntime42.Skeleton));

        /// <summary>
        /// 所有可用的版本
        /// </summary>
        public static readonly ImmutableArray<SpineVersion> RegisteredVersions;

        /// <summary>
        /// tag 到版本的映射
        /// </summary>
        private static readonly FrozenDictionary<string, SpineVersion> _tagToRegisteredVersion;

        static SpineVersion()
        {
            List<SpineVersion> registeredVersions = [];
            Dictionary<string, SpineVersion> tagToRegisteredVersion = [];

            // 通过反射获取所有 public static 字段，类型为 SpineVersion
            var props = typeof(SpineVersion).GetFields(BindingFlags.Public | BindingFlags.Static).Where(p => p.FieldType == typeof(SpineVersion));
            foreach (var prop in props)
            {
                if (prop.GetValue(null) is SpineVersion spVer)
                {
                    registeredVersions.Add(spVer);
                    tagToRegisteredVersion[spVer.Tag] = spVer;
                }
            }

            RegisteredVersions = registeredVersions.ToImmutableArray();
            _tagToRegisteredVersion = tagToRegisteredVersion.ToFrozenDictionary();
        }

        /// <summary>
        /// 从骨骼文件获取版本
        /// </summary>
        /// <param name="skelPath">文件路径</param>
        /// <exception cref="InvalidDataException">没有检测到有效的版本字符串</exception>
        /// <exception cref="ArgumentException">无效的字符串格式</exception>
        /// <exception cref="KeyNotFoundException">未注册的版本号</exception>
        public static SpineVersion GetVersion(string skelPath)
        {
            string? versionString = null;
            byte[] data = File.ReadAllBytes(skelPath);

            using var input = new MemoryStream(data);
            var binaryReader = new Utils.BinaryReader(input);
            var jsonReader = new Utf8JsonReader(data, true, default);

            // try json format
            if (Utils.Utf8Validator.IsUtf8(data))
            {
                try
                {
                    if (JsonDocument.TryParseValue(ref jsonReader, out var doc))
                    {
                        var root = doc.RootElement;
                        if (root.ValueKind == JsonValueKind.Object &&
                            root.TryGetProperty("skeleton", out var skeleton) &&
                            skeleton.ValueKind == JsonValueKind.Object &&
                            skeleton.TryGetProperty("spine", out var _version) &&
                            _version.ValueKind == JsonValueKind.String)
                        {
                            versionString = _version.GetString();
                        }
                    }
                }
                catch { }
            }

            // try v4 binary format
            if (versionString is null)
            {
                try
                {
                    input.Position = 0;
                    var hash = binaryReader.ReadLong();
                    var versionPosition = input.Position;
                    var versionByteCount = binaryReader.ReadVarInt();
                    input.Position = versionPosition;
                    if (versionByteCount <= 13)
                        versionString = binaryReader.ReadString();
                }
                catch { }
            }

            // try v3 binary format
            if (versionString is null)
            {
                try
                {
                    input.Position = 0;
                    var hash = binaryReader.ReadString();
                    versionString = binaryReader.ReadString();
                }
                catch { }
            }

            if (versionString is null)
                throw new InvalidDataException($"No verison detected: '{skelPath}'");

            var parts = versionString.Trim().Split(".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2) throw new ArgumentException($"Unknown verison: {versionString}");

            var tag = $"{parts[0]}.{parts[1]}";
            if (_tagToRegisteredVersion.TryGetValue(tag, out var version))
                return version;
            throw new KeyNotFoundException($"Unregistered verison: {versionString}");
        }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        /// <param name="skeletonType">运行时库的 <c>Skeleton</c> 类型对象, 用于查找程序集信息</param>
        private SpineVersion(Type skeletonType)
        {
            var version = skeletonType.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            ArgumentNullException.ThrowIfNull(version, skeletonType.AssemblyQualifiedName);

            // 此处假设自己填的版本号都是规范的 x.y.x
            var parts = version.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            Major = int.Parse(parts[0]);
            Minor = int.Parse(parts[1]);
            Tag = $"{Major}.{Minor}";
            RuntimeVersion = version;
        }

        /// <summary>
        /// 主版本号
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// 次版本号
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// 仅包含主次版本号的字符串, 例如 <c>3.8</c>
        /// </summary>
        public string Tag { get; }

        /// <summary>
        /// 带有补丁号的完整运行时版本号, 例如 <c>3.8.99</c>
        /// </summary>
        public string RuntimeVersion { get; }

        public override string ToString() => Tag;

        #region IEquatable 接口实现

        public bool Equals(SpineVersion? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return Major == other.Major && Minor == other.Minor;
        }

        public override bool Equals(object? obj) => obj is SpineVersion other && Equals(other);

        public override int GetHashCode() => Tag.GetHashCode();

        public static bool operator ==(SpineVersion a, SpineVersion b) => a is null ? b is null : a.Equals(b);

        public static bool operator !=(SpineVersion a, SpineVersion b) => !(a == b);

        #endregion

        #region IComparable 接口实现

        public int CompareTo(SpineVersion? other)
        {
            if (other is null) return 1;
            int majorDiff = Major.CompareTo(other.Major);
            return majorDiff != 0 ? majorDiff : Minor.CompareTo(other.Minor);
        }

        public static bool operator <(SpineVersion a, SpineVersion b) => a is null ? b is not null : a.CompareTo(b) < 0;

        public static bool operator >(SpineVersion a, SpineVersion b) => a is not null && a.CompareTo(b) > 0;

        public static bool operator <=(SpineVersion a, SpineVersion b) => a is null || a.CompareTo(b) <= 0;

        public static bool operator >=(SpineVersion a, SpineVersion b) => a is null ? b is null : a.CompareTo(b) >= 0;

        #endregion
    }
}