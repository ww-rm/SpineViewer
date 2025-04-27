using SpineViewer.Spine;
using SpineViewer.Utils;
using SpineViewer.Utils.Localize;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.SpineView
{
    /// <summary>
    /// 用于在 PropertyGrid 上显示 Spine 动画列表的包装类
    /// </summary>
    public class SpineAnimationProperty(SpineObject spine) : ICustomTypeDescriptor
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>
        /// 全轨道动画最大时长
        /// </summary>
        [LocalizedDisplayName(typeof(Properties.Resources), "maximumTrackLength")]
        public float AnimationTracksMaxDuration => Spine.GetTrackIndices().Select(i => Spine.GetAnimationDuration(Spine.GetAnimation(i))).Max();

        /// <summary>
        /// <see cref="TrackAnimationProperty"/> 属性对象缓存
        /// </summary>
        private readonly Dictionary<int, TrackAnimationProperty> trackAnimationProperties = [];

        /// <summary>
        /// <c>this.Track{i}</c>
        /// </summary>
        public TrackAnimationProperty GetTrackAnimation(int i)
        {
            if (!trackAnimationProperties.ContainsKey(i))
                trackAnimationProperties[i] = new TrackAnimationProperty(Spine, i);
            return trackAnimationProperties[i];
        }

        /// <summary>
        /// <c>this.Track{i} = <paramref name="value"/></c>
        /// </summary>
        public void SetTrackAnimation(int i, string value)
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
            if (obj is SpineAnimationProperty prop) return ToString() == prop.ToString();
            return base.Equals(obj);
        }

        public override int GetHashCode() => HashCode.Combine(typeof(SpineAnimationProperty).FullName.GetHashCode(), ToString().GetHashCode());

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
                if (!pdCache.TryGetValue(i, out var pd))
                    pdCache[i] = pd = new TrackWrapperPropertyDescriptor(i, [new DisplayNameAttribute($"轨道 {i}")]);
                props.Add(pd);
            }
            return props;
        }

        /// <summary>
        /// 轨道属性描述符, 实现对属性的读取和赋值
        /// </summary>
        /// <param name="i">轨道索引</param>
        private class TrackWrapperPropertyDescriptor(int i, Attribute[]? attributes) : PropertyDescriptor($"Track{i}", attributes)
        {
            private readonly int idx = i;

            public override Type ComponentType => typeof(SpineAnimationProperty);
            public override bool IsReadOnly => false;
            public override Type PropertyType => typeof(TrackAnimationProperty);
            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;

            /// <summary>
            /// 得到一个轨道包装类, 允许用户查看或者修改具体的属性值, 这个地方决定了在面板上看到的是一个对象及其属性
            /// </summary>
            public override object? GetValue(object? component)
            {
                if (component is SpineAnimationProperty tracks)
                    return tracks.GetTrackAnimation(idx);
                return null;
            }

            /// <summary>
            /// 允许通过字符串赋值修改该轨道的动画, 这里决定了当其他地方的调用 (比如 Converter) 通过 value 来设置属性值的时候应该怎么处理
            /// </summary>
            public override void SetValue(object? component, object? value)
            {
                if (component is SpineAnimationProperty tracks)
                {
                    if (value is string s)
                        tracks.SetTrackAnimation(idx, s);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 对 <c><see cref="SpineAnimationProperty"/>.Track{i}</c> 属性的包装类
    /// </summary>
    [TypeConverter(typeof(TrackAnimationPropertyConverter))]
    public class TrackAnimationProperty(SpineObject spine, int i)
    {
        private readonly SpineObject spine = spine;

        [Browsable(false)]
        public int Index { get; } = i;

		[LocalizedDisplayName(typeof(Properties.Resources), "duration")]
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
            if (obj is TrackAnimationProperty) return ToString() == obj.ToString();
            return base.Equals(obj);
        }

        /// <summary>
        /// 哈希码需要和 Equals 行为类似
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(typeof(TrackAnimationProperty).FullName.GetHashCode(), ToString().GetHashCode());
    }

    /// <summary>
    /// 轨道索引包装类转换器, 实现字符串和包装类的互相转换, 并且提供标准值列表对属性进行设置, 同时还提供在面板上显示包装类属性的能力
    /// </summary>
    public class TrackAnimationPropertyConverter : ExpandableObjectConverter
    {
        // NOTE: 可以不用实现 ConvertTo/ConvertFrom, 因为属性实现了与字符串之间的互转
        // ToString 实现了 ConvertTo
        // SetValue 实现了从字符串设置属性

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            if (context?.Instance is SpineAnimationProperty tracks)
            {
                return new StandardValuesCollection(tracks.Spine.AnimationNames);
            }
            else if (context?.Instance is object[] instances)
            {
                IEnumerable<string> common = [];
                foreach (SpineAnimationProperty prop in instances.Where(inst => inst is SpineAnimationProperty))
                    common = common.Union(prop.Spine.AnimationNames);
                return new StandardValuesCollection(common.ToArray());
            }
            return base.GetStandardValues(context);
        }
    }
}
