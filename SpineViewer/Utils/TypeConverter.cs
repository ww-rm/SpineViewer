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

    public class SFMLColorConverter : ExpandableObjectConverter
    {
        private class SFMLColorPropertyDescriptor : SimplePropertyDescriptor
        {
            public SFMLColorPropertyDescriptor(Type componentType, string name, Type propertyType) : base(componentType, name, propertyType) { }

            public override object? GetValue(object? component) => component?.GetType().GetField(Name)?.GetValue(component) ?? default;

            public override void SetValue(object? component, object? value) => component?.GetType().GetField(Name)?.SetValue(component, value);
        }

        private static PropertyDescriptorCollection pdCollection = null;

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
