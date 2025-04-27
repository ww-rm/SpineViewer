using SpineViewer.Spine;
using SpineViewer.Spine.SpineView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Utils
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

        private StandardValuesCollection standardValues;

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
        {
            var customizable = context?.PropertyDescriptor?.Attributes.OfType<StandardValuesAttribute>().FirstOrDefault()?.Customizable ?? false;
            return !customizable;
        }

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (standardValues is null)
            {
                // 查找属性上的 StandardValuesAttribute
                var attribute = context?.PropertyDescriptor?.Attributes.OfType<StandardValuesAttribute>().FirstOrDefault();
                if (attribute != null)
                    standardValues = new StandardValuesCollection(attribute.StandardValues);
                else
                    standardValues = new StandardValuesCollection(Array.Empty<string>());
            }
            return standardValues;
        }
    }

    public class ResolutionConverter : SizeConverter
    {
        private static readonly StandardValuesCollection standardValues = new(new Size[] {
            new(4096, 4096),
            new(2048, 2048),
            new(1024, 1024),
            new(512, 512),
            new(3840, 2160),
            new(2560, 1440),
            new(1920, 1080),
            new(1280, 720),
            new(2160, 3840),
            new(1440, 2560),
            new(1080, 1920),
            new(720, 1280),
        });

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;
        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context) => standardValues;
    }

    public class SFMLColorConverter : ExpandableObjectConverter
    {
        private static SFML.Graphics.Color ParseHexColor(string hex, bool includeAlpha)
        {
            byte r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
            byte a = includeAlpha ? byte.Parse(hex.Substring(7, 2), NumberStyles.HexNumber) : (byte)255;
            return new SFML.Graphics.Color(r, g, b, a);
        }

        private static SFML.Graphics.Color ParseShortHexColor(string hex, bool includeAlpha)
        {
            byte r = Convert.ToByte($"{hex[1]}{hex[1]}", 16);
            byte g = Convert.ToByte($"{hex[2]}{hex[2]}", 16);
            byte b = Convert.ToByte($"{hex[3]}{hex[3]}", 16);
            byte a = includeAlpha ? Convert.ToByte($"{hex[4]}{hex[4]}", 16) : (byte)255;
            return new SFML.Graphics.Color(r, g, b, a);
        }

        private static readonly StandardValuesCollection standardValues;

        static SFMLColorConverter()
        {
            // 初始化所有 KnownColor
            var knownColors = Enum.GetValues(typeof(KnownColor))
                .Cast<KnownColor>()
                .Select(knownColor =>
                {
                    var color = Color.FromKnownColor(knownColor);
                    return new SFML.Graphics.Color(color.R, color.G, color.B, color.A);
                })
                .ToArray();

            standardValues = new StandardValuesCollection(knownColors);
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

                try
                {
                    // 处理 #RRGGBBAA 和 #RRGGBB 格式
                    if (s.StartsWith("#"))
                    {
                        if (s.Length == 9) // #RRGGBBAA
                            return ParseHexColor(s, includeAlpha: true);
                        if (s.Length == 7) // #RRGGBB
                            return ParseHexColor(s, includeAlpha: false);
                        if (s.Length == 5) // #RGBA
                            return ParseShortHexColor(s, includeAlpha: true);
                        if (s.Length == 4) // #RGB
                            return ParseShortHexColor(s, includeAlpha: false);

                        throw new FormatException(Properties.Resources.formatExceptionParseColor);
                    }

                    // 处理 R,G,B,A 和 R,G,B 格式
                    var parts = s.Split(',');
                    if (parts.Length == 3 || parts.Length == 4)
                    {
                        byte r = byte.Parse(parts[0].Trim());
                        byte g = byte.Parse(parts[1].Trim());
                        byte b = byte.Parse(parts[2].Trim());
                        byte a = parts.Length == 4 ? byte.Parse(parts[3].Trim()) : (byte)255;
                        return new SFML.Graphics.Color(r, g, b, a);
                    }

                    // 尝试解析为 KnownColor
                    var color = Color.FromName(s);
                    if (color.IsKnownColor || color.IsNamedColor)
                        return new SFML.Graphics.Color(color.R, color.G, color.B, color.A);

                    throw new FormatException(Properties.Resources.formatExceptionUnknownColor);
                }
                catch (Exception ex)
                {
                    throw new FormatException(Properties.Resources.formatExceptionParseColorError, ex);
                }
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

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;
        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context) => standardValues;

        private class SFMLColorPropertyDescriptor : SimplePropertyDescriptor
        {
            public SFMLColorPropertyDescriptor(Type componentType, string name, Type propertyType) : base(componentType, name, propertyType) { }

            public override object? GetValue(object? component) => component?.GetType().GetField(Name)?.GetValue(component) ?? default;

            public override void SetValue(object? component, object? value) => component?.GetType().GetField(Name)?.SetValue(component, value);
        }

        private static PropertyDescriptorCollection pdCollection = null;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
        {
            pdCollection ??= new(
                [
                    new SFMLColorPropertyDescriptor(typeof(SFML.Graphics.Color), "R", typeof(byte)),
                    new SFMLColorPropertyDescriptor(typeof(SFML.Graphics.Color), "G", typeof(byte)),
                    new SFMLColorPropertyDescriptor(typeof(SFML.Graphics.Color), "B", typeof(byte)),
                    new SFMLColorPropertyDescriptor(typeof(SFML.Graphics.Color), "A", typeof(byte))
                ],
                true
            );
            return pdCollection;
        }
    }
}
