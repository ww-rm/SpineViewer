using SpineViewer.Spine;
using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.SpineView
{
    /// <summary>
    /// 皮肤列表动态类型包装类, 用于提供对 Spine 皮肤列表的管理能力
    /// </summary>
    /// <param name="spine">关联的 Spine 对象</param>
    public class SpineSkinProperty(SpineObject spine) : ICustomTypeDescriptor
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>
        /// <see cref="SpineSkinProperty"/> 属性缓存
        /// </summary>
        private readonly Dictionary<int, SkinNameProperty> skinNameProperties = [];

        /// <summary>
        /// <c>this.Skin{i}</c>
        /// </summary>
        public SkinNameProperty GetSkinName(int i)
        {
            if (!skinNameProperties.ContainsKey(i))
                skinNameProperties[i] = new SkinNameProperty(Spine, i);
            return skinNameProperties[i];
        }

        /// <summary>
        /// <c>this.Skin{i} = <paramref name="value"/></c>
        /// </summary>
        public void SetSkinName(int i, string value)
        {
            Spine.ReplaceSkin(i, value);
            TypeDescriptor.Refresh(this);
        }

        /// <summary>  
        /// 在属性面板悬停可以显示已加载的皮肤列表  
        /// </summary>  
        public override string ToString() => $"[{string.Join(", ", Spine.GetLoadedSkins())}]";

        public override bool Equals(object? obj)
        {
            if (obj is SpineSkinProperty prop) return ToString() == prop.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => HashCode.Combine(typeof(SpineSkinProperty).FullName.GetHashCode(), ToString().GetHashCode());

        #region ICustomTypeDescriptor 接口实现

        // XXX: 必须实现 ICustomTypeDescriptor 接口, 不能继承 CustomTypeDescriptor, 似乎继承下来的东西会有问题, 导致某些调用不正确

        private static readonly Dictionary<int, SkinNamePropertyDescriptor> pdCache = [];

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(this, true);
        public string? GetClassName() => TypeDescriptor.GetClassName(this, true);
        public string? GetComponentName() => TypeDescriptor.GetComponentName(this, true);
        public TypeConverter? GetConverter() => TypeDescriptor.GetConverter(this, true);
        public EventDescriptor? GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);
        public PropertyDescriptor? GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);
        public object? GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(this, true);
        public EventDescriptorCollection GetEvents(Attribute[]? attributes) => TypeDescriptor.GetEvents(this, attributes, true);
        public object? GetPropertyOwner(PropertyDescriptor? pd) => this;
        public PropertyDescriptorCollection GetProperties() => GetProperties(null);
        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
        {
            var props = new PropertyDescriptorCollection(TypeDescriptor.GetProperties(this, attributes, true).Cast<PropertyDescriptor>().ToArray());
            for (var i = 0; i < Spine.GetLoadedSkins().Length; i++)
            {
                if (!pdCache.ContainsKey(i))
                    pdCache[i] = new SkinNamePropertyDescriptor(i, [new DisplayNameAttribute($"皮肤 {i}")]);
                props.Add(pdCache[i]);
            }
            return props;
        }

        /// <summary>
        /// 皮肤属性描述符, 实现对属性的读取和赋值
        /// </summary>
        private class SkinNamePropertyDescriptor(int i, Attribute[]? attributes) : PropertyDescriptor($"Skin{i}", attributes)
        {
            private readonly int idx = i;

            public override Type ComponentType => typeof(SpineSkinProperty);
            public override bool IsReadOnly => false;
            public override Type PropertyType => typeof(SkinNameProperty);
            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;

            /// <summary>
            /// 得到一个 <see cref="SpineSkinProperty"/>, 允许用户查看或者修改具体的属性值, 这个地方决定了在面板上看到的是一个对象及其属性
            /// </summary>
            public override object? GetValue(object? component)
            {
                if (component is SpineSkinProperty prop)
                    return prop.GetSkinName(idx);
                return null;
            }

            /// <summary>
            /// 允许通过字符串赋值修改该位置的皮肤
            /// </summary>
            public override void SetValue(object? component, object? value)
            {
                if (component is SpineSkinProperty prop)
                {
                    if (value is string s)
                        prop.SetSkinName(idx, s);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 对 <c><see cref="SpineSkinProperty"/>.Skin{i}</c> 属性的包装类
    /// </summary>
    [TypeConverter(typeof(SkinNamePropertyConverter))]
    public class SkinNameProperty(SpineObject spine, int i)
    {
        private readonly SpineObject spine = spine;

        [Browsable(false)]
        public int Index { get; } = i;

        public override string ToString()
        {
            var loadedSkins = spine.GetLoadedSkins();
            if (Index >= 0 && Index < loadedSkins.Length)
                return loadedSkins[Index];
            return "!NULL"; // XXX: 预期应该不会发生
        }

        public override bool Equals(object? obj)
        {
            if (obj is SkinNameProperty) return ToString() == obj.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => HashCode.Combine(typeof(SkinNameProperty).FullName.GetHashCode(), ToString().GetHashCode());
    }

    public class SkinNamePropertyConverter : StringConverter
    {
        // NOTE: 可以不用实现 ConvertTo/ConvertFrom, 因为属性实现了与字符串之间的互转
        // ToString 实现了 ConvertTo
        // SetValue 实现了从字符串设置属性

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is SpineSkinProperty manager)
            {
                return new StandardValuesCollection(manager.Spine.SkinNames);
            }
            else if (context?.Instance is object[] instances && instances.All(x => x is SpineSkinProperty))
            {
                // XXX: 这里不知道为啥总是会得到 object[] 类型而不是具体的 SpineSkinWrapper[] 类型
                var managers = instances.Cast<SpineSkinProperty>().ToArray();
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
