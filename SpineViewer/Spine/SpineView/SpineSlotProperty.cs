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
    /// 用于在 PropertyGrid 上显示槽位附件加载情况包装类
    /// </summary>
    public class SpineSlotProperty(SpineObject spine) : ICustomTypeDescriptor
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>
        /// 显示所有槽位集合
        /// </summary>
        public override string ToString() => $"[{string.Join(", ", Spine.SlotAttachmentNames.Keys)}]";

        public override bool Equals(object? obj)
        {
            if (obj is SpineAnimationProperty prop) return ToString() == prop.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => HashCode.Combine(typeof(SpineAnimationProperty).FullName.GetHashCode(), ToString().GetHashCode());

        #region ICustomTypeDescriptor 接口实现

        // XXX: 必须实现 ICustomTypeDescriptor 接口, 不能继承 CustomTypeDescriptor, 似乎继承下来的东西会有问题, 导致某些调用不正确

        /// <summary>
        /// 属性描述符缓存
        /// </summary>
        private static readonly Dictionary<string, SlotPropertyDescriptor> pdCache = [];

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
            foreach (var slotName in Spine.SlotAttachmentNames.Keys)
            {
                if (!pdCache.TryGetValue(slotName, out var pd))
                    pdCache[slotName] = pd =new SlotPropertyDescriptor(slotName, [new DisplayNameAttribute($"{slotName}")]);
                props.Add(pd);
            }
            return props;
        }

        /// <summary>
        /// 槽位属性描述符, 实现对属性的读取和赋值
        /// </summary>
        internal class SlotPropertyDescriptor(string name, Attribute[]? attributes) : PropertyDescriptor($"Slot_{name}", attributes)
        {
            public string SlotName { get; } = name;

            public override Type ComponentType => typeof(SpineSlotProperty);
            public override bool IsReadOnly => false;
            public override Type PropertyType => typeof(SlotProperty);
            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;

            /// <summary>
            /// 得到一个轨道包装类, 允许用户查看或者修改具体的属性值, 这个地方决定了在面板上看到的是一个对象及其属性
            /// </summary>
            public override object? GetValue(object? component)
            {
                if (component is SpineSlotProperty slots)
                    return slots.Spine.GetSlotAttachment(SlotName);
                return null;
            }

            /// <summary>
            /// 允许通过字符串赋值修改该轨道的动画, 这里决定了当其他地方的调用 (比如 Converter) 通过 value 来设置属性值的时候应该怎么处理
            /// </summary>
            public override void SetValue(object? component, object? value)
            {
                if (component is SpineSlotProperty slots)
                {
                    if (value is string s)
                        slots.Spine.SetSlotAttachment(SlotName, s);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 对 <c><see cref="SpineSlotProperty"/>.Slot_{name}</c> 属性的包装类
    /// </summary>
    [TypeConverter(typeof(SlotPropertyConverter))]
    public class SlotProperty(SpineObject spine, string name)
    {
        private readonly SpineObject spine = spine;

        [Browsable(false)]
        public string Name { get; } = name;

        /// <summary>
        /// 实现了默认的转为字符串的方式
        /// </summary>
        public override string ToString() => spine.GetSlotAttachment(Name);

        /// <summary>
        /// 影响了属性面板的判断, 当动画名称相同的时候认为两个对象是相同的, 这样属性面板可以在多选的时候正确显示相同取值的内容
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is SlotProperty) return ToString() == obj.ToString();
            return base.Equals(obj);
        }

        /// <summary>
        /// 哈希码需要和 Equals 行为类似
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(typeof(SlotProperty).FullName.GetHashCode(), ToString().GetHashCode());
    }

    /// <summary>
    /// 轨道索引包装类转换器, 实现字符串和包装类的互相转换, 并且提供标准值列表对属性进行设置, 同时还提供在面板上显示包装类属性的能力
    /// </summary>
    public class SlotPropertyConverter : StringConverter
    {
        // NOTE: 可以不用实现 ConvertTo/ConvertFrom, 因为属性实现了与字符串之间的互转
        // ToString 实现了 ConvertTo
        // SetValue 实现了从字符串设置属性

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.PropertyDescriptor is SpineSlotProperty.SlotPropertyDescriptor pd)
            {
                if (context?.Instance is SpineSlotProperty slots)
                {
                    if (slots.Spine.SlotAttachmentNames.TryGetValue(pd.SlotName, out var names))
                        return new StandardValuesCollection(names);
                }
                else if (context?.Instance is object[] instances && instances.All(x => x is SpineSlotProperty))
                {
                    // XXX: 这里不知道为啥总是会得到 object[] 类型而不是具体的类型
                    var spinesSlots = instances.Cast<SpineAnimationProperty>().ToArray();
                    if (spinesSlots.Length > 0)
                    {
                        IEnumerable<string> common = [];
                        foreach (var t in spinesSlots)
                        {
                            if (t.Spine.SlotAttachmentNames.TryGetValue(pd.SlotName, out var names))
                                common = common.Union(names);
                        }
                        return new StandardValuesCollection(common.ToArray());
                    }
                }
            }
            return base.GetStandardValues(context);
        }
    }
}
