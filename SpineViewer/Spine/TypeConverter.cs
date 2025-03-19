using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    public class VersionConverter : EnumConverter
    {
        public VersionConverter() : base(typeof(Version)) { }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
        {
            if (destinationType == typeof(string) && value is Version version)
            {
                // 调用自定义的 String() 方法
                return version.String();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class AnimationConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
        {
            // 支持标准值列表
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
        {
            // 排他模式，只有下拉列表中的值可选
            return true;
        }

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is Spine obj)
                return new StandardValuesCollection(obj.AnimationNames);
            return base.GetStandardValues(context);
        }
    }
}
