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
    /// 皮肤动态类型包装类, 用于提供对 Spine 皮肤的管理能力
    /// </summary>
    /// <param name="spine">关联的 Spine 对象</param>
    public class SpineSkinProperty(SpineObject spine) : ICustomTypeDescriptor
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>  
        /// 在属性面板悬停可以显示已加载的皮肤列表  
        /// </summary>  
        public override string ToString() => $"[{string.Join(", ", Spine.SkinNames.Where(Spine.GetSkinStatus))}]";

        public override bool Equals(object? obj)
        {
            if (obj is SpineSkinProperty prop) return ToString() == prop.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => HashCode.Combine(typeof(SpineSkinProperty).FullName.GetHashCode(), ToString().GetHashCode());

        #region ICustomTypeDescriptor 接口实现

        // XXX: 必须实现 ICustomTypeDescriptor 接口, 不能继承 CustomTypeDescriptor, 似乎继承下来的东西会有问题, 导致某些调用不正确

        private static readonly Dictionary<string, SkinPropertyDescriptor> pdCache = [];

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
            foreach (var name in Spine.SkinNames)
            {
                if (!pdCache.ContainsKey(name))
                    pdCache[name] = new SkinPropertyDescriptor(name, [new DisplayNameAttribute(name)]);
                props.Add(pdCache[name]);
            }
            return props;
        }

        /// <summary>
        /// 皮肤属性描述符, 实现对皮肤的加载和卸载, <c><see cref="SpineSkinProperty"/>.Skin_{name}</c>
        /// </summary>
        private class SkinPropertyDescriptor(string name, Attribute[]? attributes) : PropertyDescriptor($"Skin_{name}", attributes)
        {
            private readonly string name = name;

            public override Type ComponentType => typeof(SpineSkinProperty);
            public override bool IsReadOnly => false;
            public override Type PropertyType => typeof(bool);
            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;

            public override object? GetValue(object? component)
            {
                if (component is SpineSkinProperty prop)
                    return prop.Spine.GetSkinStatus(name);
                return null;
            }

            public override void SetValue(object? component, object? value)
            {
                if (component is SpineSkinProperty prop)
                {
                    if (value is bool s)
                        prop.Spine.SetSkinStatus(name, s);
                }
            }
        }

        #endregion
    }
}
