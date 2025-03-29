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
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Collections.Immutable;
using SpineViewer.Exporter;

namespace SpineViewer.Spine
{
    /// <summary>
    /// Spine 基类, 使用静态方法 New 来创建具体版本对象
    /// </summary>
    public abstract class Spine : ImplementationResolver<Spine, SpineImplementationAttribute, SpineVersion>, SFML.Graphics.Drawable, IDisposable
    {
        /// <summary>
        /// 空动画标记
        /// </summary>
        protected const string EMPTY_ANIMATION = "<Empty>";

        /// <summary>
        /// 预览图宽
        /// </summary>
        protected const uint PREVIEW_WIDTH = 256;

        /// <summary>
        /// 预览图高
        /// </summary>
        protected const uint PREVIEW_HEIGHT = 256;

        /// <summary>
        /// 缩放最小值
        /// </summary>
        protected const float SCALE_MIN = 0.001f;

        /// <summary>
        /// 创建特定版本的 Spine
        /// </summary>
        public static Spine New(SpineVersion version, string skelPath, string? atlasPath = null)
        {
            if (version == SpineVersion.Auto) version = SpineHelper.GetVersion(skelPath);
            atlasPath ??= Path.ChangeExtension(skelPath, ".atlas");
            return New(version, [skelPath, atlasPath]).PostInit();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Spine(string skelPath, string atlasPath)
        {
            Version = GetType().GetCustomAttribute<SpineImplementationAttribute>().ImplementationKey;
            AssetsDir = Directory.GetParent(skelPath).FullName;
            SkelPath = Path.GetFullPath(skelPath);
            AtlasPath = Path.GetFullPath(atlasPath);
            Name = Path.GetFileNameWithoutExtension(skelPath);
        }

        /// <summary>
        /// 构造函数之后的初始化工作
        /// </summary>
        private Spine PostInit()
        {
            SkinNames = skinNames.AsReadOnly();
            AnimationNames = animationNames.AsReadOnly();

            InitBounds = Bounds;

            // XXX: tex 没办法在这里主动 Dispose
            // 批量添加在获取预览图的时候极大概率会和预览线程死锁
            // 虽然两边不会同时调用 Draw, 但是死锁似乎和 Draw 函数有关
            // 除此之外, 似乎还和 tex 的 Dispose 有关
            // 如果不对 tex 进行 Dispose, 那么不管是否 Draw 都正常不会死锁
            var tex = new SFML.Graphics.RenderTexture(PREVIEW_WIDTH, PREVIEW_HEIGHT);
            tex.SetView(InitBounds.GetView(PREVIEW_WIDTH, PREVIEW_HEIGHT));
            tex.Clear(SFML.Graphics.Color.Transparent);
            tex.Draw(this);
            tex.Display();

            using (var img = tex.Texture.CopyToImage())
            {
                if (img.SaveToMemory(out var imgBuffer, "bmp"))
                {
                    // 必须重复构造一个副本才能摆脱对流的依赖, 否则之后使用会报错
                    using var stream = new MemoryStream(imgBuffer);
                    using var bitmap = new Bitmap(stream);
                    Preview = new Bitmap(bitmap);
                }
            }

            // 取最后一个作为初始, 尽可能去显示非默认的内容
            Skin = SkinNames.Last();
            Track0Animation = AnimationNames.Last();

            return this;
        }

        ~Spine() { Dispose(false); }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing) { Preview?.Dispose(); }

        #region 属性 | [0] 基本信息

        /// <summary>
        /// 获取所属版本
        /// </summary>
        [TypeConverter(typeof(SpineVersionConverter))]
        [Category("[0] 基本信息"), DisplayName("运行时版本")]
        public SpineVersion Version { get; }

        /// <summary>
        /// 资源所在完整目录
        /// </summary>
        [Category("[0] 基本信息"), DisplayName("资源目录")]
        public string AssetsDir { get; }

        /// <summary>
        /// skel 文件完整路径
        /// </summary>
        [Category("[0] 基本信息"), DisplayName("skel文件路径")]
        public string SkelPath { get; }

        /// <summary>
        /// atlas 文件完整路径
        /// </summary>
        [Category("[0] 基本信息"), DisplayName("atlas文件路径")]
        public string AtlasPath { get; }

        /// <summary>
        /// 名称
        /// </summary>
        [Category("[0] 基本信息"), DisplayName("名称")]
        public string Name { get; }

        /// <summary>
        /// 获取所属文件版本
        /// </summary>
        [Category("[0] 基本信息"), DisplayName("文件版本")]
        public abstract string FileVersion { get; }

        #endregion

        #region 属性 | [1] 设置

        /// <summary>
        /// 是否被隐藏, 被隐藏的模型将仅仅在列表显示, 不参与其他行为
        /// </summary>
        [Category("[1] 设置"), DisplayName("是否隐藏")]
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否使用预乘Alpha
        /// </summary>
        [Category("[1] 设置"), DisplayName("预乘Alpha通道")]
        public bool UsePremultipliedAlpha { get; set; } = true;

        #endregion

        #region 属性 | [2] 变换

        /// <summary>
        /// 缩放比例
        /// </summary>
        [Category("[2] 变换"), DisplayName("缩放比例")]
        public abstract float Scale { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        [TypeConverter(typeof(PointFConverter))]
        [Category("[2] 变换"), DisplayName("位置")]
        public abstract PointF Position { get; set; }

        /// <summary>
        /// 水平翻转
        /// </summary>
        [Category("[2] 变换"), DisplayName("水平翻转")]
        public abstract bool FlipX { get; set; }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        [Category("[2] 变换"), DisplayName("垂直翻转")]
        public abstract bool FlipY { get; set; }

        #endregion

        #region 属性 | [3] 动画

        /// <summary>
        /// 包含的所有皮肤名称
        /// </summary>
        public ReadOnlyCollection<string> SkinNames { get; private set; }
        protected List<string> skinNames = [];

        /// <summary>
        /// 使用的皮肤名称, 如果设置的皮肤不存在则忽略
        /// </summary>
        [TypeConverter(typeof(SkinConverter))]
        [Category("[3] 动画"), DisplayName("皮肤")]
        public abstract string Skin { get; set; }

        /// <summary>
        /// 包含的所有动画名称
        /// </summary>
        public ReadOnlyCollection<string> AnimationNames { get; private set; }
        protected List<string> animationNames = [EMPTY_ANIMATION];

        /// <summary>
        /// 默认轨道动画名称, 如果设置的动画不存在则忽略
        /// </summary>
        [TypeConverter(typeof(AnimationConverter))]
        [Category("[3] 动画"), DisplayName("默认轨道动画")]
        public abstract string Track0Animation { get; set; }

        /// <summary>
        /// 默认轨道动画时长
        /// </summary>
        [Category("[3] 动画"), DisplayName("默认轨道动画时长")]
        public float Track0AnimationDuration { get => GetAnimationDuration(Track0Animation); } // TODO: 动画时长变成伪属性在面板显示

        #endregion

        #region 属性 | [4] 调试

        /// <summary>
        /// 显示调试
        /// </summary>
        [Browsable(false)]
        public bool IsDebug { get; set; } = false;

        /// <summary>
        /// 显示纹理
        /// </summary>
        [Category("[4] 调试"), DisplayName("显示纹理")]
        public bool DebugTexture { get; set; } = true;

        /// <summary>
        /// 显示包围盒
        /// </summary>
        [Category("[4] 调试"), DisplayName("显示包围盒")]
        public bool DebugBounds { get; set; } = true;

        /// <summary>
        /// 显示骨骼
        /// </summary>
        [Category("[4] 调试"), DisplayName("显示骨架")]
        public bool DebugBones { get; set; } = false;

        #endregion

        /// <summary>
        /// 标识符
        /// </summary>
        public readonly string ID = Guid.NewGuid().ToString();

        /// <summary>
        /// 是否被选中
        /// </summary>
        [Browsable(false)]
        public bool IsSelected { get; set; } = false;

        /// <summary>
        /// 骨骼包围盒
        /// </summary>
        [Browsable(false)]
        public abstract RectangleF Bounds { get; }

        /// <summary>
        /// 初始状态下的骨骼包围盒
        /// </summary>
        [Browsable(false)]
        public RectangleF InitBounds { get; private set; }

        /// <summary>
        /// 骨骼预览图
        /// </summary>
        [Browsable(false)]
        public Image Preview { get; private set; }

        /// <summary>
        /// 获取动画时长, 如果动画不存在则返回 0
        /// </summary>
        public abstract float GetAnimationDuration(string name);

        /// <summary>
        /// 更新内部状态
        /// </summary>
        public abstract void Update(float delta);

        #region SFML.Graphics.Drawable 接口实现

        /// <summary>
        /// 顶点坐标缓冲区
        /// </summary>
        protected float[] worldVerticesBuffer = new float[1024];

        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        protected readonly SFML.Graphics.VertexArray vertexArray = new(SFML.Graphics.PrimitiveType.Triangles);

        /// <summary>
        /// 包围盒颜色
        /// </summary>
        protected static readonly SFML.Graphics.Color BoundsColor = new(120, 200, 0);

        /// <summary>
        /// 包围盒顶点数组
        /// </summary>
        protected readonly SFML.Graphics.VertexArray boundsVertices = new(SFML.Graphics.PrimitiveType.LineStrip, 5);

        /// <summary>
        /// SFML.Graphics.Drawable 接口实现
        /// </summary>
        public abstract void Draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states);

        #endregion
    }
}
