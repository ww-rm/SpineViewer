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
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace SpineViewer.Spine
{
    /// <summary>
    /// Spine 基类, 使用静态方法 New 来创建具体版本对象, 该类是线程安全的
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
        /// 数据锁
        /// </summary>
        private readonly object _lock = new();

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
            AnimationTracks = new(this);

            // 必须 Update 一次否则包围盒还没有值
            update(0);

            // XXX: tex 没办法在这里主动 Dispose
            // 批量添加在获取预览图的时候极大概率会和预览线程死锁
            // 虽然两边不会同时调用 Draw, 但是死锁似乎和 Draw 函数有关
            // 除此之外, 似乎还和 tex 的 Dispose 有关
            // 如果不对 tex 进行 Dispose, 那么不管是否 Draw 都正常不会死锁
            var tex = new SFML.Graphics.RenderTexture(PREVIEW_WIDTH, PREVIEW_HEIGHT);
            tex.SetView(bounds.GetView(PREVIEW_WIDTH, PREVIEW_HEIGHT));
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
            skin = SkinNames.Last();
            setAnimation(0, AnimationNames.Last());

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
        public bool IsHidden
        {
            get { lock (_lock) return isHidden; }
            set { lock (_lock) isHidden = value; }
        }
        protected bool isHidden = false;

        /// <summary>
        /// 是否使用预乘Alpha
        /// </summary>
        [Category("[1] 设置"), DisplayName("预乘Alpha通道")]
        public bool UsePremultipliedAlpha
        {
            get { lock (_lock) return usePremultipliedAlpha; }
            set { lock (_lock) usePremultipliedAlpha = value; }
        }
        protected bool usePremultipliedAlpha = false;

        #endregion

        #region 属性 | [2] 变换

        /// <summary>
        /// 缩放比例
        /// </summary>
        [Category("[2] 变换"), DisplayName("缩放比例")]
        public float Scale
        {
            get { lock (_lock) return scale; }
            set { lock (_lock) { scale = value; update(0); } }
        }
        protected abstract float scale { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        [TypeConverter(typeof(PointFConverter))]
        [Category("[2] 变换"), DisplayName("位置")]
        public PointF Position
        {
            get { lock (_lock) return position; }
            set { lock (_lock) { position = value; update(0); } }
        }
        protected abstract PointF position { get; set; }

        /// <summary>
        /// 水平翻转
        /// </summary>
        [Category("[2] 变换"), DisplayName("水平翻转")]
        public bool FlipX
        {
            get { lock (_lock) return flipX; }
            set { lock (_lock) { flipX = value; update(0); } }
        }
        protected abstract bool flipX { get; set; }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        [Category("[2] 变换"), DisplayName("垂直翻转")]
        public bool FlipY
        {
            get { lock (_lock) return flipY; }
            set { lock (_lock) { flipY = value; update(0); } }
        }
        protected abstract bool flipY { get; set; }

        #endregion

        #region 属性 | [3] 动画

        /// <summary>
        /// 使用的皮肤名称, 如果设置的皮肤不存在则忽略
        /// </summary>
        [TypeConverter(typeof(SkinConverter))]
        [Category("[3] 动画"), DisplayName("皮肤")]
        public string Skin
        {
            get { lock (_lock) return skin; }
            set { lock (_lock) { skin = value; update(0); } }
        }
        protected abstract string skin { get; set; }

        /// <summary>
        /// 默认轨道动画名称, 如果设置的动画不存在则忽略
        /// </summary>
        [TypeConverter(typeof(AnimationConverter))]
        [Category("[3] 动画"), DisplayName("轨道 0 动画")]
        public string Track0Animation
        {
            get { lock (_lock) return getAnimation(0); }
            set { lock (_lock) { setAnimation(0, value); update(0); } }
        }

        /// <summary>
        /// 默认轨道动画时长
        /// </summary>
        [Category("[3] 动画"), DisplayName("轨道 0 动画时长")]
        public float Track0AnimationDuration => GetAnimationDuration(Track0Animation);

        /// <summary>
        /// 默认轨道动画时长
        /// </summary>
        [Editor(typeof(AnimationTracksEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("[3] 动画"), DisplayName("多轨道动画管理")]
        public AnimationTracksType AnimationTracks { get; private set; }

        /// <summary>
        /// 包含的所有皮肤名称
        /// </summary>
        [Browsable(false)]
        public ReadOnlyCollection<string> SkinNames { get; private set; }
        protected List<string> skinNames = [];

        /// <summary>
        /// 包含的所有动画名称
        /// </summary>
        [Browsable(false)]
        public ReadOnlyCollection<string> AnimationNames { get; private set; }
        protected List<string> animationNames = [EMPTY_ANIMATION];

        /// <summary>
        /// 获取所有非 null 的轨道索引
        /// </summary>
        public int[] GetTrackIndices() { lock (_lock) return getTrackIndices(); }
        protected abstract int[] getTrackIndices();

        /// <summary>
        /// 获取指定轨道的当前动画, 如果没有, 应当返回空动画名称
        /// </summary>
        public string GetAnimation(int track) { lock (_lock) return getAnimation(track); }
        protected abstract string getAnimation(int track);

        /// <summary>
        /// 设置某个轨道动画
        /// </summary>
        public void SetAnimation(int track, string name) { lock (_lock) setAnimation(track, name); }
        protected abstract void setAnimation(int track, string name);

        /// <summary>
        /// 清除某个轨道, 与设置空动画不同, 是彻底删除轨道内的东西
        /// </summary>
        public void ClearTrack(int i) { lock (_lock) clearTrack(i); }
        protected abstract void clearTrack(int i); // XXX: 清除轨道之后被加载的附件还是会保留, 不会自动卸下, 除非使用 SetSlotsToSetupPose

        /// <summary>
        /// 获取动画时长, 如果动画不存在则返回 0
        /// </summary>
        public abstract float GetAnimationDuration(string name);

        #endregion

        #region 属性 | [4] 调试

        /// <summary>
        /// 显示调试
        /// </summary>
        [Browsable(false)]
        public bool IsDebug
        {
            get { lock (_lock) return isDebug; }
            set { lock (_lock) isDebug = value; }
        }
        protected bool isDebug = false;

        /// <summary>
        /// 显示纹理
        /// </summary>
        [Category("[4] 调试"), DisplayName("显示纹理")]
        public bool DebugTexture
        {
            get { lock (_lock) return debugTexture; }
            set { lock (_lock) debugTexture = value; }
        }
        protected bool debugTexture = true;

        /// <summary>
        /// 显示包围盒
        /// </summary>
        [Category("[4] 调试"), DisplayName("显示包围盒")]
        public bool DebugBounds
        {
            get { lock (_lock) return debugBounds; }
            set { lock (_lock) debugBounds = value; }
        }
        protected bool debugBounds = true;

        /// <summary>
        /// 显示骨骼
        /// </summary>
        [Category("[4] 调试"), DisplayName("显示骨架")]
        public bool DebugBones
        {
            get { lock (_lock) return debugBones; }
            set { lock (_lock) debugBones = value; }
        }
        protected bool debugBones = false;

        #endregion

        /// <summary>
        /// 标识符
        /// </summary>
        public readonly string ID = Guid.NewGuid().ToString();

        /// <summary>
        /// 是否被选中
        /// </summary>
        [Browsable(false)]
        public bool IsSelected
        {
            get { lock (_lock) return isSelected; }
            set { lock (_lock) isSelected = value; }
        }
        protected bool isSelected = false;

        /// <summary>
        /// 骨骼包围盒
        /// </summary>
        [Browsable(false)]
        public RectangleF Bounds { get { lock (_lock) return bounds; } }
        protected abstract RectangleF bounds { get; }

        /// <summary>
        /// 骨骼预览图
        /// </summary>
        [Browsable(false)]
        public Image Preview { get; private set; }

        /// <summary>
        /// 更新内部状态
        /// </summary>
        public void Update(float delta) { lock (_lock) update(delta); }
        protected abstract void update(float delta);

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
        public void Draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states) { lock (_lock) draw(target, states); }
        protected abstract void draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states);

        #endregion

    }
}