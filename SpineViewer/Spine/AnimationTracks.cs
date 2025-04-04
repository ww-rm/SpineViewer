using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine
{
    /// <summary>
    /// 对轨道索引的包装类, 能够在面板上显示例如时长的属性, 但是处理该属性时按字符串去处理, 例如 ToString 和判断对象相等都是用动画名称实现逻辑
    /// </summary>
    /// <param name="spine"></param>
    /// <param name="i"></param>
    [TypeConverter(typeof(TrackWrapperConverter))]
    public class TrackWrapper(Spine spine, int i)
    {
        private readonly Spine spine = spine;

        [Browsable(false)]
        public int Index { get; } = i;

        [DisplayName("时长")]
        public float Duration => spine.GetAnimationDuration(spine.GetAnimation(Index));

        /// <summary>
        /// 实现了默认的转为字符串的方式
        /// </summary>
        public override string ToString() => spine.GetAnimation(Index);

        /// <summary>
        /// 影响了属性面板的判断, 当动画名称相同的时候认为两个对象是相同的, 这样属性面板可以在多选的时候正确显示相同取值的内容
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is TrackWrapper) return ToString() == obj.ToString();
            return base.Equals(obj);
        }

        /// <summary>
        /// 哈希码需要和 Equals 行为类似
        /// </summary>
        public override int GetHashCode() => (typeof(TrackWrapper).FullName + ToString()).GetHashCode();
    }

    /// <summary>
    /// 轨道属性描述符, 实现对属性的读取和赋值
    /// </summary>
    /// <param name="spine">关联的 Spine 对象</param>
    /// <param name="i">轨道索引</param>
    public class TrackWrapperPropertyDescriptor(Spine spine, int i) : PropertyDescriptor($"Track{i}", [new DisplayNameAttribute($"轨道 {i}")])
    {
        private readonly Spine spine = spine;
        private readonly int idx = i;

        public override Type ComponentType => typeof(AnimationTracksType);
        public override bool IsReadOnly => false;
        public override Type PropertyType => typeof(TrackWrapper);
        public override bool CanResetValue(object component) => false;
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) => false;

        /// <summary>
        /// 得到一个轨道包装类, 允许用户查看或者修改具体的属性值, 这个地方决定了在面板上看到的是一个对象及其属性
        /// </summary>
        public override object? GetValue(object? component) => new TrackWrapper(spine, idx);

        /// <summary>
        /// 允许通过字符串赋值修改该轨道的动画, 这里决定了当其他地方的调用 (比如 Converter) 通过 value 来设置属性值的时候应该怎么处理
        /// </summary>
        public override void SetValue(object? component, object? value) 
        {
            if (value is string s) spine.SetAnimation(idx, s);
        }
    }

    /// <summary>
    /// AnimationTracks 动态类型包装类, 用于提供对 Spine 对象多轨道动画的访问能力, 不同轨道将动态生成属性
    /// </summary>
    /// <param name="spine">关联的 Spine 对象</param>
    public class AnimationTracksType(Spine spine) : ICustomTypeDescriptor
    {
        private readonly Dictionary<int, TrackWrapperPropertyDescriptor> pdCache = [];
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
            var props = new List<TrackWrapperPropertyDescriptor>();
            foreach (var i in Spine.GetTrackIndices())
            {
                if (!pdCache.ContainsKey(i))
                    pdCache[i] = new TrackWrapperPropertyDescriptor(Spine, i);
                props.Add(pdCache[i]);
            }
            return new PropertyDescriptorCollection(props.ToArray());
        }

        /// <summary>
        /// 在属性面板悬停可以按轨道顺序显示动画名称
        /// </summary>
        public override string ToString() => $"[{string.Join(", ", Spine.GetTrackIndices().Select(Spine.GetAnimation))}]";

        public override bool Equals(object? obj)
        {
            if (obj is AnimationTracksType tracks) return ToString() == tracks.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => (typeof(AnimationTracksType).FullName + ToString()).GetHashCode();
    }
}
