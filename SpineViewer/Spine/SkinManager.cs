using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    /// <summary>
    /// 对皮肤的包装类
    /// </summary>
    [TypeConverter(typeof(SkinWrapperConverter))]
    public class SkinWrapper(Spine spine, int i)
    {
        private readonly Spine spine = spine;

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

        public override int GetHashCode() => (typeof(SkinWrapper).FullName + ToString()).GetHashCode();
    }

    /// <summary>
    /// 皮肤属性描述符, 实现对属性的读取和赋值
    /// </summary>
    /// <param name="spine">关联的 Spine 对象</param>
    public class SkinWrapperPropertyDescriptor(Spine spine, int i) : PropertyDescriptor($"Skin{i}", [new DisplayNameAttribute($"皮肤 {i}")])
    {
        private readonly Spine spine = spine;
        private readonly int idx = i;

        public override Type ComponentType => typeof(SkinManager);
        public override bool IsReadOnly => false;
        public override Type PropertyType => typeof(SkinWrapper);
        public override bool CanResetValue(object component) => false;
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) => false;

        /// <summary>
        /// 得到一个 SkinWrapper, 允许用户查看或者修改具体的属性值, 这个地方决定了在面板上看到的是一个对象及其属性
        /// </summary>
        public override object? GetValue(object? component) => new SkinWrapper(spine, idx);

        /// <summary>
        /// 允许通过字符串赋值修改该位置的皮肤
        /// </summary>
        public override void SetValue(object? component, object? value)
        {
            if (value is string s) spine.ReplaceSkin(idx, s);
        }
    }

    /// <summary>
    /// SkinManager 动态类型包装类, 用于提供对 Spine 皮肤列表的管理能力
    /// </summary>
    /// <param name="spine">关联的 Spine 对象</param>
    public class SkinManager(Spine spine) : ICustomTypeDescriptor
    {
        private readonly Dictionary<int, SkinWrapperPropertyDescriptor> pdCache = [];
        public Spine Spine { get; } = spine;

        // XXX: 必须实现 ICustomTypeDescriptor 接口, 不能继承 CustomTypeDescriptor, 似乎继承下来的东西会有问题, 导致某些调用不正确

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
            var props = new List<SkinWrapperPropertyDescriptor>();
            for (var i = 0; i < Spine.GetLoadedSkins().Length; i++)
            {
                if (!pdCache.ContainsKey(i))
                    pdCache[i] = new SkinWrapperPropertyDescriptor(Spine, i);
                props.Add(pdCache[i]);
            }
            return new PropertyDescriptorCollection(props.ToArray());
        }

        /// <summary>  
        /// 在属性面板悬停可以显示已加载的皮肤列表  
        /// </summary>  
        public override string ToString() => $"[{string.Join(", ", Spine.GetLoadedSkins())}]";

        public override bool Equals(object? obj)
        {
            if (obj is SkinManager manager) return ToString() == manager.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => (typeof(SkinManager).FullName + ToString()).GetHashCode();
    }
}
