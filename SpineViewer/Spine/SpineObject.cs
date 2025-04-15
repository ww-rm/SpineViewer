using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Drawing.Design;
using NLog;
using System.Xml.Linq;
using SpineViewer.Extensions;
using SpineViewer.Utils;

namespace SpineViewer.Spine
{
    /// <summary>
    /// Spine 基类, 使用静态方法 New 来创建具体版本对象, 该类是线程安全的
    /// </summary>
    public abstract class SpineObject : ImplementationResolver<SpineObject, SpineImplementationAttribute, SpineVersion>, SFML.Graphics.Drawable, IDisposable
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
        /// 创建特定版本的 Spine
        /// </summary>
        public static SpineObject New(SpineVersion version, string skelPath, string? atlasPath = null)
        {
            atlasPath ??= Path.ChangeExtension(skelPath, ".atlas");
            skelPath = Path.GetFullPath(skelPath);
            atlasPath = Path.GetFullPath(atlasPath);
            
            if (version == SpineVersion.Auto) version = SpineUtils.GetVersion(skelPath);
            if (!File.Exists(atlasPath)) throw new FileNotFoundException($"atlas file {atlasPath} not found");
            return New(version, [skelPath, atlasPath]).PostInit();
        }

        /// <summary>
        /// 数据锁
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// 日志器
        /// </summary>
        protected readonly Logger logger = LogManager.GetCurrentClassLogger();
        private bool skinLoggerWarned = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SpineObject(string skelPath, string atlasPath)
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
        private SpineObject PostInit()
        {
            SkinNames = skinNames.AsReadOnly();
            AnimationNames = animationNames.AsReadOnly();

            // 必须 Update 一次否则包围盒还没有值
            update(0);

            // XXX: tex 没办法在这里主动 Dispose
            // 批量添加在获取预览图的时候极大概率会和预览线程死锁
            // 虽然两边不会同时调用 Draw, 但是死锁似乎和 Draw 函数有关
            // 除此之外, 似乎还和 tex 的 Dispose 有关
            // 如果不对 tex 进行 Dispose, 那么不管是否 Draw 都正常不会死锁
            var tex = new SFML.Graphics.RenderTexture(PREVIEW_WIDTH, PREVIEW_HEIGHT);
            using var view = getBounds().GetView(PREVIEW_WIDTH, PREVIEW_HEIGHT);
            tex.SetView(view);
            tex.Clear(SFML.Graphics.Color.Transparent);
            tex.Draw(this);
            tex.Display();
            Preview = tex.Texture.CopyToBitmap();

            // 默认初始化10个空位
            for (int i = 0; i < 10; i++)
            {
                setAnimation(i, AnimationNames.First());
                loadedSkins.Add(SkinNames.First());
            }
            reloadSkins();

            return this;
        }

        ~SpineObject() { Dispose(false); }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing)
        { 
            Preview?.Dispose();
            triangleVertices.Dispose();
            lineVertices.Dispose();
            rectLineVertices.Dispose();
        }

        /// <summary>
        /// 运行时唯一 ID
        /// </summary>
        public string ID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 骨骼预览图, 并没有去除预乘, 画面可能偏暗
        /// </summary>
        public Image Preview { get; private set; }

        /// <summary>
        /// 获取所属版本
        /// </summary>
        public SpineVersion Version { get; }

        /// <summary>
        /// 资源所在完整目录
        /// </summary>
        public string AssetsDir { get; }

        /// <summary>
        /// skel 文件完整路径
        /// </summary>
        public string SkelPath { get; }

        /// <summary>
        /// atlas 文件完整路径
        /// </summary>
        public string AtlasPath { get; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 获取所属文件版本
        /// </summary>
        public abstract string FileVersion { get; }

        /// <summary>
        /// 是否被隐藏, 被隐藏的模型将仅仅在列表显示, 不参与其他行为
        /// </summary>
        public bool IsHidden { get { lock (_lock) return isHidden; } set { lock (_lock) isHidden = value; } }
        protected bool isHidden = false;

        /// <summary>
        /// 是否使用预乘 Alpha
        /// </summary>
        public bool UsePma { get { lock (_lock) return usePma; } set { lock (_lock) usePma = value; } }
        protected bool usePma = false;

        /// <summary>
        /// 缩放比例
        /// </summary>
        public float Scale
        {
            get { lock (_lock) return scale; }
            set { lock (_lock) { scale = Math.Max(value, 0.001f); update(0); } }
        }
        protected abstract float scale { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public PointF Position
        {
            get { lock (_lock) return position; }
            set { lock (_lock) { position = value; update(0); } }
        }
        protected abstract PointF position { get; set; }

        /// <summary>
        /// 水平翻转
        /// </summary>
        public bool FlipX
        {
            get { lock (_lock) return flipX; }
            set { lock (_lock) { flipX = value; update(0); } }
        }
        protected abstract bool flipX { get; set; }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        public bool FlipY
        {
            get { lock (_lock) return flipY; }
            set { lock (_lock) { flipY = value; update(0); } }
        }
        protected abstract bool flipY { get; set; }

        /// <summary>
        /// 包含的所有皮肤名称
        /// </summary>
        public ReadOnlyCollection<string> SkinNames { get; private set; }
        protected readonly List<string> skinNames = [];

        /// <summary>
        /// 包含的所有动画名称
        /// </summary>
        public ReadOnlyCollection<string> AnimationNames { get; private set; }
        protected readonly List<string> animationNames = [EMPTY_ANIMATION];

        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool IsSelected
        {
            get { lock (_lock) return isSelected; }
            set { lock (_lock) { isSelected = value; update(0); } }
        }
        protected bool isSelected = false;

        /// <summary>
        /// 启用渲染调试
        /// </summary>
        public bool EnableDebug
        {
            get { lock (_lock) return enableDebug; }
            set { lock (_lock) { enableDebug = value; update(0); } }
        }
        private bool enableDebug = false;

        /// <summary>
        /// 显示纹理
        /// </summary>
        public bool DebugTexture
        {
            get { lock (_lock) return debugTexture; }
            set { lock (_lock) { debugTexture = value; update(0); } }
        }
        private bool debugTexture = true;

        /// <summary>
        /// 显示包围盒
        /// </summary>
        public bool DebugBounds
        {
            get { lock (_lock) return debugBounds; }
            set { lock (_lock) { debugBounds = value; update(0); } }
        }
        protected bool debugBounds = true;

        /// <summary>
        /// 显示骨骼
        /// </summary>
        public bool DebugBones
        {
            get { lock (_lock) return debugBones; }
            set { lock (_lock) { debugBones = value; update(0); } }
        }
        protected bool debugBones = false;

        /// <summary>
        /// 显示区域附件边框
        /// </summary>
        public bool DebugRegions
        {
            get { lock (_lock) return debugRegions; }
            set { lock (_lock) { debugRegions = value; update(0); } }
        }
        protected bool debugRegions = false;

        /// <summary>
        /// 显示网格附件边框线
        /// </summary>
        public bool DebugMeshHulls
        {
            get { lock (_lock) return debugMeshHulls; }
            set { lock (_lock) { debugMeshHulls = value; update(0); } }
        }
        protected bool debugMeshHulls = false;

        /// <summary>
        /// 显示网格附件网格线
        /// </summary>
        public bool DebugMeshes
        {
            get { lock (_lock) return debugMeshes; }
            set { lock (_lock) { debugMeshes = value; update(0); } }
        }
        protected bool debugMeshes = false;

        /// <summary>
        /// 显示碰撞盒附件边框线
        /// </summary>
        public bool DebugBoundingBoxes
        {
            get { lock (_lock) return debugBoundingBoxes; }
            set { lock (_lock) { debugBoundingBoxes = value; update(0); } }
        }
        protected bool debugBoundingBoxes = false;

        /// <summary>
        /// 显示路径附件网格线
        /// </summary>
        public bool DebugPaths
        {
            get { lock (_lock) return debugPaths; }
            set { lock (_lock) { debugPaths = value; update(0); } }
        }
        protected bool debugPaths = false;

        /// <summary>
        /// 显示点附件
        /// </summary>
        public bool DebugPoints
        {
            get { lock (_lock) return debugPoints; }
            set { lock (_lock) { debugPoints = value; update(0); } }
        }
        protected bool debugPoints = false;

        /// <summary>
        /// 显示剪裁附件网格线
        /// </summary>
        public bool DebugClippings
        {
            get { lock (_lock) return debugClippings; }
            set { lock (_lock) { debugClippings = value; update(0); } }
        }
        protected bool debugClippings = false;

        /// <summary>
        /// 获取已加载的皮肤列表快照, 允许出现重复值
        /// </summary>
        public string[] GetLoadedSkins() { lock (_lock) return loadedSkins.ToArray(); }
        protected readonly List<string> loadedSkins = [];

        /// <summary>
        /// 加载指定皮肤, 添加至列表末尾, 如果不存在则忽略, 允许加载重复的值
        /// </summary>
        public void LoadSkin(string name)
        {
            if (!skinNames.Contains(name)) return;
            lock (_lock)
            {
                loadedSkins.Add(name);
                reloadSkins();

                if (!skinLoggerWarned && Version < SpineVersion.V38 && loadedSkins.Count > 1)
                {
                    logger.Warn($"Multiplt skins not supported in SpineVersion {Version.GetName()}");
                    skinLoggerWarned = true;
                }
            }
        }

        /// <summary>
        /// 卸载列表指定位置皮肤, 如果超出范围则忽略
        /// </summary>
        public void UnloadSkin(int idx)
        {
            lock (_lock)
            {
                if (idx < 0 || idx >= loadedSkins.Count) return;
                loadedSkins.RemoveAt(idx);
                reloadSkins();
            }
        }

        /// <summary>
        /// 替换皮肤列表指定位置皮肤, 超出范围或者皮肤不存在则忽略
        /// </summary>
        public void ReplaceSkin(int idx, string name)
        {
            lock (_lock)
            {
                if (idx < 0 || idx >= loadedSkins.Count || !skinNames.Contains(name)) return;
                loadedSkins[idx] = name;
                reloadSkins();
            }
        }

        /// <summary>
        /// 重新加载现有皮肤列表, 用于刷新等操作
        /// </summary>
        public void ReloadSkins() { lock (_lock) reloadSkins(); }
        private void reloadSkins()
        {
            clearSkin();
            foreach (var s in loadedSkins.Distinct()) addSkin(s);
            update(0);
        }

        /// <summary>
        /// 加载皮肤, 如果不存在则忽略
        /// </summary>
        protected abstract void addSkin(string name);

        /// <summary>
        /// 清空加载的所有皮肤
        /// </summary>
        protected abstract void clearSkin();

        /// <summary>
        /// 获取所有非 null 的轨道索引快照
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
        public void SetAnimation(int track, string name) { lock (_lock) { setAnimation(track, name); update(0); } }
        protected abstract void setAnimation(int track, string name);

        /// <summary>
        /// 清除某个轨道, 与设置空动画不同, 是彻底删除轨道内的东西
        /// </summary>
        public void ClearTrack(int i) { lock (_lock) { clearTrack(i); update(0); } }
        protected abstract void clearTrack(int i); // XXX: 清除轨道之后被加载的附件还是会保留, 不会自动卸下, 除非使用 SetSlotsToSetupPose

        /// <summary>
        /// 获取动画时长, 如果动画不存在则返回 0
        /// </summary>
        public abstract float GetAnimationDuration(string name);

        /// <summary>
        /// 重置所有轨道上的动画时间
        /// </summary>
        public void ResetAnimationsTime() { lock (_lock) { foreach (var i in getTrackIndices()) setAnimation(i, getAnimation(i)); update(0); } }

        /// <summary>
        /// 获取当前状态包围盒
        /// </summary>
        public RectangleF GetBounds() { lock (_lock) return getBounds(); }
        protected abstract RectangleF getBounds();

        /// <summary>
        /// 更新内部状态
        /// </summary>
        public void Update(float delta) { lock (_lock) update(delta); }
        protected abstract void update(float delta);

        #region SFML.Graphics.Drawable 接口实现

        /// <summary>
        /// 包围盒颜色
        /// </summary>
        protected static readonly SFML.Graphics.Color BoundsColor = new(120, 200, 0);

        /// <summary>
        /// 骨骼点颜色
        /// </summary>
        protected static readonly SFML.Graphics.Color BonePointColor = new(0, 255, 0);

        /// <summary>
        /// 骨骼线颜色
        /// </summary>
        protected static readonly SFML.Graphics.Color BoneLineColor = new(255, 0, 0);

        /// <summary>
        /// 网格线颜色
        /// </summary>
        protected static readonly SFML.Graphics.Color MeshLineColor = new(255, 163, 0, 128);

        /// <summary>
        /// 附件边框线颜色
        /// </summary>
        protected static readonly SFML.Graphics.Color AttachmentLineColor = new(0, 0, 255, 128);

        /// <summary>
        /// 剪裁附件边框线颜色
        /// </summary>
        protected static readonly SFML.Graphics.Color ClippingLineColor = new(204, 0, 0);

        /// <summary>
        /// spine 顶点坐标缓冲区
        /// </summary>
        protected float[] worldVerticesBuffer = new float[1024];

        /// <summary>
        /// 三角形顶点缓冲区
        /// </summary>
        protected readonly SFML.Graphics.VertexArray triangleVertices = new(SFML.Graphics.PrimitiveType.Triangles);

        /// <summary>
        /// 无面积线条缓冲区
        /// </summary>
        protected readonly SFML.Graphics.VertexArray lineVertices = new(SFML.Graphics.PrimitiveType.Lines);

        /// <summary>
        /// 有半径圆点临时缓存对象
        /// </summary>
        private readonly SFML.Graphics.CircleShape circlePointShape = new();

        /// <summary>
        /// 有宽度线条缓冲区, 需要通过 <see cref="AddRectLine"/> 添加顶点
        /// </summary>
        protected readonly SFML.Graphics.VertexArray rectLineVertices = new(SFML.Graphics.PrimitiveType.Quads);

        /// <summary>
        /// 绘制有半径的实心圆点, 随模型一起缩放大小
        /// </summary>
        protected void DrawCirclePoint(SFML.Graphics.RenderTarget target, SFML.System.Vector2f p, SFML.Graphics.Color color, float radius = 1)
        {
            circlePointShape.Origin = new(radius, radius);
            circlePointShape.Position = p;
            circlePointShape.FillColor = color;
            circlePointShape.Radius = radius;
            target.Draw(circlePointShape);
        }

        /// <summary>
        /// 绘制有宽度的实心线, 会随模型一起缩放粗细, 顶点被存储在 <see cref="rectLineVertices"/> 数组内
        /// </summary>
        protected void AddRectLine(SFML.System.Vector2f p1, SFML.System.Vector2f p2, SFML.Graphics.Color color, float width = 1)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            var dt = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dt == 0) return;

            var cosTheta = -dy / dt;
            var sinTheta = dx / dt;
            var halfWidth = width / 2;
            var t = new SFML.System.Vector2f(halfWidth * cosTheta, halfWidth * sinTheta);
            var v = new SFML.Graphics.Vertex() { Color = color };

            v.Position = p1 + t; rectLineVertices.Append(v);
            v.Position = p2 + t; rectLineVertices.Append(v);
            v.Position = p2 - t; rectLineVertices.Append(v);
            v.Position = p1 - t; rectLineVertices.Append(v);
        }

        /// <summary>
        /// SFML.Graphics.Drawable 接口实现
        /// <para>这个渲染实现绘制出来的像素将是预乘的, 当渲染的背景透明度是 1 时, 则等价于非预乘的结果, 即正常画面, 否则画面偏暗</para>
        /// <para>可以用于 <see cref="SFML.Graphics.RenderWindow"/> 的渲染, 因为直接在窗口上绘制时窗口始终是不透明的</para>
        /// </summary>
        public void Draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states)
        {
            lock (_lock)
            {
                if (!enableDebug)
                {
                    draw(target, states);
                }
                else
                {
                    if (debugTexture) draw(target, states);
                    if (isSelected) debugDraw(target);
                }
            }
        }

        /// <summary>
        /// 这个渲染实现绘制出来的像素将是预乘的, 当渲染的背景透明度是 1 时, 则等价于非预乘的结果, 即正常画面, 否则画面偏暗
        /// <para>可以用于 <see cref="SFML.Graphics.RenderWindow"/> 的渲染, 因为直接在窗口上绘制时窗口始终是不透明的</para>
        /// </summary>
        protected abstract void draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states);

        /// <summary>
        /// 渲染调试内容
        /// </summary>
        protected abstract void debugDraw(SFML.Graphics.RenderTarget target);

        #endregion
    }
}