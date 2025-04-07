using SpineViewer.PropertyGridWrappers.Spine;
using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers
{
    public class PointFConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is PointF point)
            {
                return $"{point.X}, {point.Y}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string str)
            {
                var parts = str.Split(',');
                if (parts.Length == 2 &&
                    float.TryParse(parts[0], out var x) &&
                    float.TryParse(parts[1], out var y))
                {
                    return new PointF(x, y);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class StringEnumConverter : StringConverter
    {
        /// <summary>
        /// 字符串标准值列表属性
        /// </summary>
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
        public class StandardValuesAttribute : Attribute
        {
            /// <summary>
            /// 标准值列表
            /// </summary>
            public ReadOnlyCollection<string> StandardValues { get; private set; }
            private readonly List<string> standardValues = [];

            /// <summary>
            /// 是否允许用户自定义
            /// </summary>
            public bool Customizable { get; set; } = false;

            /// <summary>
            /// 字符串标准值列表
            /// </summary>
            /// <param name="values">允许的字符串标准值</param>
            public StandardValuesAttribute(params string[] values)
            {
                standardValues.AddRange(values);
                StandardValues = standardValues.AsReadOnly();
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
        {
            var customizable = context?.PropertyDescriptor?.Attributes.OfType<StandardValuesAttribute>().FirstOrDefault()?.Customizable ?? false;
            return !customizable;
        }

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            // 查找属性上的 StandardValuesAttribute
            var attribute = context?.PropertyDescriptor?.Attributes.OfType<StandardValuesAttribute>().FirstOrDefault();
            StandardValuesCollection result;
            if (attribute != null)
                result = new StandardValuesCollection(attribute.StandardValues);
            else
                result = new StandardValuesCollection(Array.Empty<string>());
            return result;
        }
    }

    public class SpineVersionConverter : EnumConverter
    {
        public SpineVersionConverter() : base(typeof(SpineVersion)) { }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
        {
            if (destinationType == typeof(string) && value is SpineVersion version)
                return version.GetName();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class SpineSkinNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is SpineViewer.Spine.Spine obj)
            {
                return new StandardValuesCollection(obj.SkinNames);
            }
            else if (context?.Instance is SpineViewer.Spine.Spine[] spines)
            {
                if (spines.Length > 0)
                {
                    IEnumerable<string> common = spines[0].SkinNames;
                    foreach (var spine in spines.Skip(1))
                        common = common.Union(spine.SkinNames);
                    return new StandardValuesCollection(common.ToArray());
                }
            }
            return base.GetStandardValues(context);
        }
    }

    public class SpineAnimationNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is SpineViewer.Spine.Spine obj)
            {
                return new StandardValuesCollection(obj.AnimationNames);
            }
            else if (context?.Instance is SpineViewer.Spine.Spine[] spines)
            {
                if (spines.Length > 0)
                {
                    IEnumerable<string> common = spines[0].AnimationNames;
                    foreach (var spine in spines.Skip(1))
                        common = common.Union(spine.AnimationNames);
                    return new StandardValuesCollection(common.ToArray());
                }
            }
            return base.GetStandardValues(context);
        }
    }

    /// <summary>
    /// 皮肤位包装类转换器, 实现字符串和包装类的互相转换, 并且提供标准值列表对属性进行设置, 同时还提供在面板上显示包装类属性的能力
    /// </summary>
    public class SkinWrapperConverter : StringConverter
    {
        // NOTE: 可以不用实现 ConvertTo/ConvertFrom, 因为属性实现了与字符串之间的互转
        // ToString 实现了 ConvertTo
        // SetValue 实现了从字符串设置属性

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is SpineSkinWrapper manager)
            {
                return new StandardValuesCollection(manager.Spine.SkinNames);
            }
            else if (context?.Instance is object[] instances && instances.All(x => x is SpineSkinWrapper))
            {
                // XXX: 这里不知道为啥总是会得到 object[] 类型而不是具体的 SpineSkinWrapper[] 类型
                var managers = instances.Cast<SpineSkinWrapper>().ToArray();
                if (managers.Length > 0)
                {
                    IEnumerable<string> common = managers[0].Spine.SkinNames;
                    foreach (var t in managers.Skip(1))
                        common = common.Union(t.Spine.SkinNames);
                    return new StandardValuesCollection(common.ToArray());
                }
            }
            return base.GetStandardValues(context);
        }
    }

    /// <summary>
    /// 轨道索引包装类转换器, 实现字符串和包装类的互相转换, 并且提供标准值列表对属性进行设置, 同时还提供在面板上显示包装类属性的能力
    /// </summary>
    public class TrackWrapperConverter : ExpandableObjectConverter
    {
        // NOTE: 可以不用实现 ConvertTo/ConvertFrom, 因为属性实现了与字符串之间的互转
        // ToString 实现了 ConvertTo
        // SetValue 实现了从字符串设置属性

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is SpineAnimationWrapper tracks)
            {
                return new StandardValuesCollection(tracks.Spine.AnimationNames);
            }
            else if (context?.Instance is object[] instances && instances.All(x => x is SpineAnimationWrapper))
            {
                // XXX: 这里不知道为啥总是会得到 object[] 类型而不是具体的类型
                var animTracks = instances.Cast<SpineAnimationWrapper>().ToArray();
                if (animTracks.Length > 0)
                {
                    IEnumerable<string> common = animTracks[0].Spine.AnimationNames;
                    foreach (var t in animTracks.Skip(1))
                        common = common.Union(t.Spine.AnimationNames);
                    return new StandardValuesCollection(common.ToArray());
                }
            }
            return base.GetStandardValues(context);
        }
    }

    public class SFMLColorConverter : ExpandableObjectConverter
    {
        private class SFMLColorPropertyDescriptor : SimplePropertyDescriptor
        {
            public SFMLColorPropertyDescriptor(Type componentType, string name, Type propertyType) : base(componentType, name, propertyType) { }

            public override object? GetValue(object? component) => component?.GetType().GetField(Name)?.GetValue(component) ?? default;

            public override void SetValue(object? component, object? value) => component?.GetType().GetField(Name)?.SetValue(component, value);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
            {
                s = s.Trim();
                if (s.StartsWith("#") && s.Length == 9)
                {
                    try
                    {
                        // 解析 R, G, B, A 分量，注意16进制解析
                        byte r = byte.Parse(s.Substring(1, 2), NumberStyles.HexNumber);
                        byte g = byte.Parse(s.Substring(3, 2), NumberStyles.HexNumber);
                        byte b = byte.Parse(s.Substring(5, 2), NumberStyles.HexNumber);
                        byte a = byte.Parse(s.Substring(7, 2), NumberStyles.HexNumber);
                        return new SFML.Graphics.Color(r, g, b, a);
                    }
                    catch (Exception ex)
                    {
                        throw new FormatException("无法解析颜色，确保格式为 #RRGGBBAA", ex);
                    }
                }
                throw new FormatException("格式错误，正确格式为 #RRGGBBAA");
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is SFML.Graphics.Color color)
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
        {
            // 自定义属性集合
            var properties = new List<PropertyDescriptor>
            {
                // 定义 R, G, B, A 四个字段的描述器
                new SFMLColorPropertyDescriptor(typeof(SFML.Graphics.Color), "R", typeof(byte)),
                new SFMLColorPropertyDescriptor(typeof(SFML.Graphics.Color), "G", typeof(byte)),
                new SFMLColorPropertyDescriptor(typeof(SFML.Graphics.Color), "B", typeof(byte)),
                new SFMLColorPropertyDescriptor(typeof(SFML.Graphics.Color), "A", typeof(byte))
            };

            // 返回自定义属性集合
            return new PropertyDescriptorCollection(properties.ToArray());
        }
    }
}
