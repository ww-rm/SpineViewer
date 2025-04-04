using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
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
                        common = common.Union(spine.SkinNames);
                    return new StandardValuesCollection(common.ToArray());
                }
            }
            return base.GetStandardValues(context);
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
                        common = common.Union(spine.AnimationNames);
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
            if (context.Instance is AnimationTracksType animTrack)
            {
                return new StandardValuesCollection(animTrack.Spine.AnimationNames);
            }
            else if (context.Instance is object[] instances && instances.All(x => x is AnimationTracksType))
            {
                // XXX: 这里不知道为啥总是会得到 object[] 类型而不是具体的 AnimationTracksType[] 类型
                var animTracks = instances.Cast<AnimationTracksType>().ToArray();
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

    public class SkinWrapperConverter : StringConverter
    {
        // NOTE: 可以不用实现 ConvertTo/ConvertFrom, 因为属性实现了与字符串之间的互转
        // ToString 实现了 ConvertTo
        // SetValue 实现了从字符串设置属性

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context.Instance is SkinManager manager)
            {
                return new StandardValuesCollection(manager.Spine.SkinNames);
            }
            else if (context.Instance is object[] instances && instances.All(x => x is SkinManager))
            {
                // XXX: 这里不知道为啥总是会得到 object[] 类型而不是具体的 SkinManager[] 类型
                var managers = instances.Cast<SkinManager>().ToArray();
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
}
