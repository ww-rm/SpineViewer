using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    /// <summary>
    /// 支持的 Spine 版本
    /// </summary>
    public enum SpineVersion
    {
        [Description("<Auto>")] Auto = 0x0000,
        [Description("2.1.x")] V21 = 0x0201,
        [Description("3.6.x")] V36 = 0x0306,
        [Description("3.7.x")] V37 = 0x0307,
        [Description("3.8.x")] V38 = 0x0308,
        [Description("4.0.x")] V40 = 0x0400,
        [Description("4.1.x")] V41 = 0x0401,
        [Description("4.2.x")] V42 = 0x0402,
        [Description("4.3.x")] V43 = 0x0403,
    }

    /// <summary>
    /// Spine 实现类标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SpineImplementationAttribute(SpineVersion version) : Attribute, IImplementationKey<SpineVersion>
    {
        public SpineVersion ImplementationKey { get; private set; } = version;
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
}
