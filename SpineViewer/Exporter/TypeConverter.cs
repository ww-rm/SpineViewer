using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter
{
    public class SFMLImageFileSuffixConverter : StringConverter
    {
        private readonly string[] supportedFileSuffix = [".png", ".jpg", ".tga", ".bmp"];

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            return new StandardValuesCollection(supportedFileSuffix);
        }
    }

    public class SFMLColorConverter : ExpandableObjectConverter
    {
        private class SFMLColorPropertyDescriptor : SimplePropertyDescriptor
        {
            public SFMLColorPropertyDescriptor(Type componentType, string name, Type propertyType) : base(componentType, name, propertyType) { }

            public override object? GetValue(object? component)
            {
                return component?.GetType().GetField(Name)?.GetValue(component) ?? default;
            }

            public override void SetValue(object? component, object? value)
            {
                component?.GetType().GetField(Name)?.SetValue(component, value);
            }
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
