using SpineViewer.Spine;
using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Spine
{
    /// <summary>
    /// 对轨道索引属性的包装类, 能够在面板上显示例如时长的属性, 但是处理该属性时按字符串去处理, 例如 ToString 和判断对象相等都是用动画名称实现逻辑
    /// </summary>
    /// <param name="spine"></param>
    /// <param name="i"></param>
    [TypeConverter(typeof(TrackWrapperConverter))]
    public class TrackWrapper(SpineObject spine, int i)
    {
        private readonly SpineObject spine = spine;

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
        public override int GetHashCode() => HashCode.Combine(typeof(TrackWrapper).FullName.GetHashCode(), ToString().GetHashCode());
    }

    /// <summary>
    /// 用于在 PropertyGrid 上显示 Spine 动画列表的包装类
    /// </summary>
    public class SpineAnimationWrapper(SpineObject spine) : ICustomTypeDescriptor
    {
        /// <summary>
        /// 轨道属性描述符, 实现对属性的读取和赋值
        /// </summary>
        /// <param name="i">轨道索引</param>
        private class TrackWrapperPropertyDescriptor(int i, Attribute[]? attributes) : PropertyDescriptor($"Track{i}", attributes)
        {
            private readonly int idx = i;

            public override Type ComponentType => typeof(SpineAnimationWrapper);
            public override bool IsReadOnly => false;
            public override Type PropertyType => typeof(TrackWrapper);
            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;

            /// <summary>
            /// 得到一个轨道包装类, 允许用户查看或者修改具体的属性值, 这个地方决定了在面板上看到的是一个对象及其属性
            /// </summary>
            public override object? GetValue(object? component)
            {
                if (component is SpineAnimationWrapper tracks)
                    return tracks.GetTrackWrapper(idx);
                return null;
            }

            /// <summary>
            /// 允许通过字符串赋值修改该轨道的动画, 这里决定了当其他地方的调用 (比如 Converter) 通过 value 来设置属性值的时候应该怎么处理
            /// </summary>
            public override void SetValue(object? component, object? value)
            {
                if (component is SpineAnimationWrapper tracks)
                {
                    if (value is string s)
                        tracks.SetTrackWrapper(idx, s);
                }
            }
        }

        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>
        /// 全轨道动画最大时长
        /// </summary>
        [DisplayName("全轨道最大时长")]
        public float AnimationTracksMaxDuration => Spine.GetTrackIndices().Select(i => Spine.GetAnimationDuration(Spine.GetAnimation(i))).Max();

        /// <summary>
        /// TrackWrapper 属性对象缓存
        /// </summary>
        private readonly Dictionary<int, TrackWrapper> trackWrapperProperties = [];

        /// <summary>
        /// 访问 TrackWrapper 属性 <c>AnimationTracks.Track{i}</c>
        /// </summary>
        public TrackWrapper GetTrackWrapper(int i)
        {
            if (!trackWrapperProperties.ContainsKey(i))
                trackWrapperProperties[i] = new TrackWrapper(Spine, i);
            return trackWrapperProperties[i];
        }

        /// <summary>
        /// 设置 TrackWrapper 属性 <c>AnimationTracks.Track{i} = <paramref name="value"/></c>
        /// </summary>
        public void SetTrackWrapper(int i, string value)
        {
            Spine.SetAnimation(i, value);
            TypeDescriptor.Refresh(this);
        }

        /// <summary>
        /// 在属性面板悬停可以按轨道顺序显示动画名称
        /// </summary>
        public override string ToString() => $"[{string.Join(", ", Spine.GetTrackIndices().Select(Spine.GetAnimation))}]";

        public override bool Equals(object? obj)
        {
            if (obj is SpineAnimationWrapper wrapper) return ToString() == wrapper.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => HashCode.Combine(typeof(SpineAnimationWrapper).FullName.GetHashCode(), ToString().GetHashCode());

        #region ICustomTypeDescriptor 接口实现

        // XXX: 必须实现 ICustomTypeDescriptor 接口, 不能继承 CustomTypeDescriptor, 似乎继承下来的东西会有问题, 导致某些调用不正确

        /// <summary>
        /// 属性描述符缓存
        /// </summary>
        private static readonly Dictionary<int, TrackWrapperPropertyDescriptor> pdCache = [];

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
            foreach (var i in Spine.GetTrackIndices())
            {
                if (!pdCache.ContainsKey(i))
                    pdCache[i] = new TrackWrapperPropertyDescriptor(i, [new DisplayNameAttribute($"轨道 {i}")]);
                props.Add(pdCache[i]);
            }
            return props;
        }

        #endregion
    }
}
