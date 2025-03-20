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
using System.Text.Json.Nodes;

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
        /// 空动画标记
        /// </summary>
        public const string EMPTY_ANIMATION = "<Empty>";

        /// <summary>
        /// 预览图大小
        /// </summary>
        public static readonly Size PREVIEW_SIZE = new(256, 256);

        /// <summary>
        /// 缩放最小值
        /// </summary>
        public const float SCALE_MIN = 0.001f;

        /// <summary>
        /// 实现类缓存
        /// </summary>
        private static readonly Dictionary<Version, Type> ImplementationTypes = [];
        public static readonly Dictionary<Version, Type>.KeyCollection ImplementedVersions;

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
                    if (ImplementationTypes.ContainsKey(attr.Version))
                        throw new InvalidOperationException($"Multiple implementations found: {attr.Version}");
                    ImplementationTypes[attr.Version] = type;
                }
            }
            Program.Logger.Debug("Find Spine implementations: [{}]", string.Join(", ", ImplementationTypes.Keys));
            ImplementedVersions = ImplementationTypes.Keys;

            // 加载 FragmentShader
            try
            {
                FragmentShader = SFML.Graphics.Shader.FromString(null, null, FRAGMENT_SHADER);
            }
            catch (Exception ex)
            {
                FragmentShader = null;
                Program.Logger.Error(ex.ToString());
                Program.Logger.Error("Failed to load fragment shader");
                MessageBox.Show("Fragment shader 加载失败，预乘Alpha通道属性失效", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 尝试检测骨骼文件版本
        /// </summary>
        public static Version? GetVersion(string skelPath)
        {
            string versionString = null;
            Version? version = null;
            using var input = File.OpenRead(skelPath);
            var reader = new SkeletonConverter.BinaryReader(input);

            // try json format
            try
            {
                if (JsonNode.Parse(input) is JsonObject root && root.TryGetPropertyValue("spine", out var node))
                    versionString = (string)node;
            }
            catch { }

            // try v4 binary format
            if (versionString is null)
            {
                try
                {
                    input.Position = 0;
                    var hash = reader.ReadLong();
                    var versionPosition = input.Position;
                    var versionByteCount = reader.ReadVarInt();
                    input.Position = versionPosition;
                    if (versionByteCount <= 13)
                        versionString = reader.ReadString();
                }
                catch { }
            }

            // try v3 binary format
            if (versionString is null)
            {
                try
                {
                    input.Position = 0;
                    var hash = reader.ReadString();
                    versionString = reader.ReadString();
                }
                catch { }
            }

            if (versionString is not null)
            {
                if (versionString.StartsWith("2.1.")) version = Version.V21;
                else if (versionString.StartsWith("3.6.")) version = Version.V36;
                else if (versionString.StartsWith("3.7.")) version = Version.V37;
                else if (versionString.StartsWith("3.8.")) version = Version.V38;
                else if (versionString.StartsWith("4.0.")) version = Version.V40;
                else if (versionString.StartsWith("4.1.")) version = Version.V41;
                else if (versionString.StartsWith("4.2.")) version = Version.V42;
                else if (versionString.StartsWith("4.3.")) version = Version.V43;
            }

            return version;
        }

        /// <summary>
        /// 创建特定版本的 Spine
        /// </summary>
        public static Spine New(Version version, string skelPath, string? atlasPath = null)
        {
            if (version == Version.Auto)
            {
                if (GetVersion(skelPath) is Version detectedVersion)
                    version = detectedVersion;
                else
                    throw new InvalidDataException($"Auto version detection failed for {skelPath}, try to use a specific version");
            }
            if (!ImplementationTypes.TryGetValue(version, out var spineType))
            {
                throw new NotImplementedException($"Not implemented version: {version}");
            }
            return (Spine)Activator.CreateInstance(spineType, skelPath, atlasPath);
        }

        /// <summary>
        /// 标识符
        /// </summary>
        public readonly string ID = Guid.NewGuid().ToString();

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
                throw new InvalidOperationException($"Class {type.Name} has no SpineImplementationAttribute");
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
        protected virtual void Dispose(bool disposing) { preview?.Dispose(); }

        /// <summary>
        /// 获取所属版本
        /// </summary>
        [TypeConverter(typeof(VersionConverter))]
        [Category("基本信息"), DisplayName("运行时版本")]
        public Version Version { get; }

        /// <summary>
        /// skel 文件完整路径
        /// </summary>
        [Category("基本信息"), DisplayName("skel文件路径")]
        public string SkelPath { get; }

        /// <summary>
        /// atlas 文件完整路径
        /// </summary>
        [Category("基本信息"), DisplayName("atlas文件路径")]
        public string AtlasPath { get; }

        [Category("基本信息"), DisplayName("名称")]
        public string Name { get; }

        /// <summary>
        /// 获取所属文件版本
        /// </summary>
        [Category("基本信息"), DisplayName("文件版本")]
        public abstract string FileVersion { get; }

        /// <summary>
        /// 缩放比例
        /// </summary>
        [Category("变换"), DisplayName("缩放比例")]
        public abstract float Scale { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        [TypeConverter(typeof(PointFConverter))]
        [Category("变换"), DisplayName("位置")]
        public abstract PointF Position { get; set; }

        /// <summary>
        /// 水平翻转
        /// </summary>
        [Category("变换"), DisplayName("水平翻转")]
        public abstract bool FlipX { get; set; }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        [Category("变换"), DisplayName("垂直翻转")]
        public abstract bool FlipY { get; set; }

        /// <summary>
        /// 是否使用预乘Alpha
        /// </summary>
        [Category("画面"), DisplayName("预乘Alpha通道")]
        public bool UsePremultipliedAlpha { get; set; } = true;

        /// <summary>
        /// 包含的所有动画名称
        /// </summary>
        [Browsable(false)]
        public ReadOnlyCollection<string> AnimationNames { get => animationNames.AsReadOnly(); }
        protected List<string> animationNames = [EMPTY_ANIMATION];

        /// <summary>
        /// 默认动画名称
        /// </summary>
        [Browsable(false)]
        public string DefaultAnimationName { get => animationNames.Last(); }

        /// <summary>
        /// 当前动画名称
        /// </summary>
        [TypeConverter(typeof(AnimationConverter))]
        [Category("动画"), DisplayName("当前动画")]
        public abstract string CurrentAnimation { get; set; }

        /// <summary>
        /// 当前动画时长
        /// </summary>
        [Category("动画"), DisplayName("当前动画时长")]
        public float CurrentAnimationDuration { get => GetAnimationDuration(CurrentAnimation); }

        /// <summary>
        /// 骨骼包围盒
        /// </summary>
        [Browsable(false)]
        public abstract RectangleF Bounds { get; }

        /// <summary>
        /// 骨骼预览图
        /// </summary>
        [Browsable(false)]
        public Image Preview
        {
            get
            {
                if (preview is null)
                {
                    using var img = GetPreview((uint)PREVIEW_SIZE.Width, (uint)PREVIEW_SIZE.Height);
                    img.SaveToMemory(out var imgBuffer, "bmp");
                    using var stream = new MemoryStream(imgBuffer);
                    preview = new Bitmap(stream);
                }
                return preview;
            }
        }
        private Image preview = null;

        /// <summary>
        /// 获取指定尺寸的预览图
        /// </summary>
        public SFML.Graphics.Image GetPreview(uint width, uint height)
        {
            var curAnimation = CurrentAnimation;
            CurrentAnimation = EMPTY_ANIMATION;
            var bounds = Bounds;

            float viewX = width;
            float viewY = height;
            float sizeX = bounds.Width;
            float sizeY = bounds.Height;

            var scale = 1f;
            if ((sizeY / sizeX) < (viewY / viewX))
                scale = sizeX / viewX;// 相同的 X, 视窗 Y 更大
            else
                scale = sizeY / viewY;// 相同的 Y, 视窗 X 更大

            viewX *= scale;
            viewY *= scale;

            using var tex = new SFML.Graphics.RenderTexture(width, height);
            var view = tex.GetView();
            view.Center = new(bounds.X + viewX / 2, bounds.Y + viewY / 2);
            view.Size = new(viewX, -viewY);
            tex.SetView(view);
            tex.Clear(SFML.Graphics.Color.Transparent);
            tex.Draw(this);
            tex.Display();
            CurrentAnimation = curAnimation;
            return tex.Texture.CopyToImage();
        }

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

        /// <summary>
        /// 是否被选中
        /// </summary>
        [Browsable(false)]
        public bool IsSelected { get; set; } = false;
    }
}
