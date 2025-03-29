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
        public VersionConverter() : base(typeof(SpineVersion)) { }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
        {
            if (destinationType == typeof(string) && value is SpineVersion version)
                return version.GetName();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class AnimationConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is Spine obj)
            {
                return new StandardValuesCollection(obj.AnimationNames);
            }
            else if (context?.Instance is Spine[] spines)
            {
                if (spines.Length > 0)
                {
                    IEnumerable<string> common = spines[0].AnimationNames;
                    foreach (var spine in spines.Skip(1))
                        common = common.Intersect(spine.AnimationNames);
                    return new StandardValuesCollection(common.ToArray());
                }
            }
            return base.GetStandardValues(context);
        }
    }

    public class SkinConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is Spine obj)
            {
                return new StandardValuesCollection(obj.SkinNames);
            }
            else if (context?.Instance is Spine[] spines)
            {
                if (spines.Length > 0)
                {
                    IEnumerable<string> common = spines[0].SkinNames;
                    foreach (var spine in spines.Skip(1))
                        common = common.Intersect(spine.SkinNames);
                    return new StandardValuesCollection(common.ToArray());
                }
            }
            return base.GetStandardValues(context);
        }
    }
}
