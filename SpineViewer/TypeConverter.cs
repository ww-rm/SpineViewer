using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer
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

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

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
}
