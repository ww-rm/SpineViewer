using System;
using System.Collections.Generic;
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

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
        {
            return TypeDescriptor.GetProperties(typeof(PointF), attributes);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;
    }
}
