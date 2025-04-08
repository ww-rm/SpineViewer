using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Spine
{
    /// <summary>
    /// 对皮肤属性的包装类
    /// </summary>
    [TypeConverter(typeof(SkinWrapperConverter))]
    public class SkinWrapper(SpineViewer.Spine.Spine spine, int i)
    {
        private readonly SpineViewer.Spine.Spine spine = spine;

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
            if (obj is SkinWrapper) return ToString() == obj.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => HashCode.Combine(typeof(SkinWrapper).FullName.GetHashCode(), ToString().GetHashCode());
    }

    /// <summary>
    /// 皮肤列表动态类型包装类, 用于提供对 Spine 皮肤列表的管理能力
    /// </summary>
    /// <param name="spine">关联的 Spine 对象</param>
    public class SpineSkinWrapper(SpineViewer.Spine.Spine spine) : ICustomTypeDescriptor
    {
        /// <summary>
        /// 皮肤属性描述符, 实现对属性的读取和赋值
        /// </summary>
        private class SkinWrapperPropertyDescriptor(int i, Attribute[]? attributes) : PropertyDescriptor($"Skin{i}", attributes)
        {
            private readonly int idx = i;

            public override Type ComponentType => typeof(SpineSkinWrapper);
            public override bool IsReadOnly => false;
            public override Type PropertyType => typeof(SkinWrapper);
            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;

            /// <summary>
            /// 得到一个 SkinWrapper, 允许用户查看或者修改具体的属性值, 这个地方决定了在面板上看到的是一个对象及其属性
            /// </summary>
            public override object? GetValue(object? component)
            {
                if (component is SpineSkinWrapper manager)
                    return manager.GetSkinWrapper(idx);
                return null;
            }

            /// <summary>
            /// 允许通过字符串赋值修改该位置的皮肤
            /// </summary>
            public override void SetValue(object? component, object? value)
            {
                if (component is SpineSkinWrapper manager)
                {
                    if (value is string s)
                        manager.SetSkinWrapper(idx, s);
                }
            }
        }

        [Browsable(false)]
        public SpineViewer.Spine.Spine Spine { get; } = spine;

        /// <summary>
        /// SkinWrapper 属性缓存
        /// </summary>
        private readonly Dictionary<int, SkinWrapper> skinWrapperProperties = [];

        /// <summary>
        /// 访问 SkinWrapper 属性 <c>SkinManager.Skin{i}</c>
        /// </summary>
        public SkinWrapper GetSkinWrapper(int i)
        {
            if (!skinWrapperProperties.ContainsKey(i))
                skinWrapperProperties[i] = new SkinWrapper(Spine, i);
            return skinWrapperProperties[i];
        }

        /// <summary>
        /// 设置 SkinWrapper 属性 <c>SkinManager.Skin{i} = <paramref name="value"/></c>
        /// </summary>
        public void SetSkinWrapper(int i, string value) => Spine.ReplaceSkin(i, value);

        /// <summary>  
        /// 在属性面板悬停可以显示已加载的皮肤列表  
        /// </summary>  
        public override string ToString() => $"[{string.Join(", ", Spine.GetLoadedSkins())}]";

        public override bool Equals(object? obj)
        {
            if (obj is SpineSkinWrapper wrapper) return ToString() == wrapper.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => HashCode.Combine(typeof(SpineSkinWrapper).FullName.GetHashCode(), ToString().GetHashCode());

        #region ICustomTypeDescriptor 接口实现

        // XXX: 必须实现 ICustomTypeDescriptor 接口, 不能继承 CustomTypeDescriptor, 似乎继承下来的东西会有问题, 导致某些调用不正确

        private static readonly Dictionary<int, SkinWrapperPropertyDescriptor> pdCache = [];

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
                    pdCache[i] = new SkinWrapperPropertyDescriptor(i, [new DisplayNameAttribute($"皮肤 {i}")]);
                props.Add(pdCache[i]);
            }
            return props;
        }

        #endregion
    }
}
