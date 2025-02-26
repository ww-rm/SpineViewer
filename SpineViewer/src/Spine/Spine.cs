using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Collections;
using System.Collections.ObjectModel;
using SFML.System;
using SFML.Window;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SpineViewer.Spine
{


    /// <summary>
    /// Spine 实现类标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SpineImplementationAttribute : Attribute
    {
        public Version Version { get; }

        public SpineImplementationAttribute(Version version)
        {
            Version = version;
        }
    }

    /// <summary>
    /// Spine 基类, 使用静态方法 New 来创建具体版本对象
    /// </summary>
    public abstract class Spine : SFML.Graphics.Drawable, IDisposable
    {
        /// <summary>
        /// 实现类缓存
        /// </summary>
        private static readonly Dictionary<Version, Type> ImplementationTypes = [];

        /// <summary>
        /// 用于解决 PMA 和渐变动画问题的片段着色器
        /// </summary>
        private const string FRAGMENT_SHADER = (
            "uniform sampler2D t;" +
            "void main() { vec4 p = texture2D(t, gl_TexCoord[0].xy);" +
            "if (p.a > 0) p.rgb /= max(max(max(p.r, p.g), p.b), p.a);" +
            "gl_FragColor = gl_Color * p; }"
        );

        /// <summary>
        /// 用于解决 PMA 和渐变动画问题的片段着色器
        /// </summary>
        protected static readonly SFML.Graphics.Shader? FragmentShader = null;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static Spine()
        {
            // 遍历并缓存标记了 SpineImplementationAttribute 的类型
            var impTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(Spine).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in impTypes)
            {
                var attr = type.GetCustomAttribute<SpineImplementationAttribute>();
                if (attr is not null)
                {
                    ImplementationTypes[attr.Version] = type;
                }
            }
            Program.Logger.Debug($"Find Spine implementations: [{string.Join(", ", ImplementationTypes.Keys)}]");

            // 加载 FragmentShader
            try
            {
                FragmentShader = SFML.Graphics.Shader.FromString(null, null, FRAGMENT_SHADER);
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex.ToString());
                Program.Logger.Error("Failed to load fragment shader");
                FragmentShader = null;
            }
        }

        /// <summary>
        /// 创建特定版本的 Spine
        /// </summary>
        public static Spine New(Version version, string skelPath, string? atlasPath = null)
        {
            if (!ImplementationTypes.TryGetValue(version, out var spineType))
            {
                throw new NotImplementedException($"Not implemented version: {version}");
            }
            return (Spine)Activator.CreateInstance(spineType, skelPath, atlasPath);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Spine(string skelPath, string? atlasPath = null)
        {
            // 获取子类类型
            var type = GetType();
            var attr = type.GetCustomAttribute<SpineImplementationAttribute>();
            if (attr is null)
            {
                throw new InvalidOperationException($"Class {type.Name} has no SpineImplementationAttribute.");
            }

            atlasPath ??= Path.ChangeExtension(skelPath, ".atlas");

            // 设置 Version
            Version = attr.Version;
            SkelPath = Path.GetFullPath(skelPath);
            AtlasPath = Path.GetFullPath(atlasPath);
            Name = Path.GetFileNameWithoutExtension(skelPath);
        }

        ~Spine() { Dispose(false); }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing) { }

        /// <summary>
        /// 缩放最小值
        /// </summary>
        [Browsable(false)]
        public const float SCALE_MIN = 0.001f;

        /// <summary>
        /// 获取所属版本
        /// </summary>
        [TypeConverter(typeof(VersionTypeConverter))]
        [Browsable(true), Category("基本信息"), DisplayName("版本")]
        public Version Version { get; }

        /// <summary>
        /// skel 文件完整路径
        /// </summary>
        [Browsable(true), Category("基本信息"), DisplayName("skel文件路径")]
        public string SkelPath { get; }

        /// <summary>
        /// atlas 文件完整路径
        /// </summary>
        [Browsable(true), Category("基本信息"), DisplayName("atlas文件路径")]
        public string AtlasPath { get; }

        [Browsable(true), Category("基本信息"), DisplayName("名称")]
        public string Name { get; }

        /// <summary>
        /// 缩放比例
        /// </summary>
        [Browsable(true), Category("空间变换"), DisplayName("缩放比例")]
        public abstract float Scale { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        [TypeConverter(typeof(PointFTypeConverter))]
        [Browsable(true), Category("空间变换"), DisplayName("位置")]
        public abstract PointF Position { get; set; }

        /// <summary>
        /// 水平翻转
        /// </summary>
        [Browsable(true), Category("空间变换"), DisplayName("水平翻转")]
        public abstract bool FlipX { get; set; }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        [Browsable(true), Category("空间变换"), DisplayName("垂直翻转")]
        public abstract bool FlipY { get; set; }

        /// <summary>
        /// 是否使用预乘Alpha
        /// </summary>
        [Browsable(true), Category("其他"), DisplayName("预乘Alpha通道")]
        public bool UsePremultipliedAlpha { get; set; }

        /// <summary>
        /// 包含的所有动画名称
        /// </summary>
        [Browsable(false)]
        public ReadOnlyCollection<string> AnimationNames { get => animationNames.AsReadOnly(); }
        protected List<string> animationNames = [];

        /// <summary>
        /// 默认动画名称
        /// </summary>
        [Browsable(false)]
        public string DefaultAnimationName { get => animationNames.Last(); }

        /// <summary>
        /// 当前动画名称
        /// </summary>
        [TypeConverter(typeof(AnimationTypeConverter))]
        [Browsable(true), Category("其他"), DisplayName("当前播放动画"), PropertyTab()]
        public abstract string CurrentAnimation { get; set; }

        /// <summary>
        /// 骨骼包围盒
        /// </summary>
        [Browsable(false)]
        public abstract RectangleF Bounds { get; }

        /// <summary>
        /// 获取动画时长, 如果动画不存在则返回 0
        /// </summary>
        public abstract float GetAnimationDuration(string name);

        /// <summary>
        /// 更新内部状态
        /// </summary>
        /// <param name="delta">时间间隔</param>
        public abstract void Update(float delta);

        /// <summary>
        /// 顶点坐标缓冲区
        /// </summary>
        protected float[] worldVerticesBuffer = new float[1024];

        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        protected SFML.Graphics.VertexArray vertexArray = new(SFML.Graphics.PrimitiveType.Triangles);

        /// <summary>
        /// SFML.Graphics.Drawable 接口实现
        /// </summary>
        public abstract void Draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states);
    }
}
