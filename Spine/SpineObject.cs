using NLog;
using Spine.Implementations;
using Spine.Interfaces;
using Spine.Interfaces.Attachments;
using Spine.Utils;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Spine
{
    public class SpineObject : SFML.Graphics.Drawable, IDisposable
    {
        /// <summary>
        /// 可能的 skel 和 atlas 文件后缀, key 是 skel 后缀, value 是对应的可能的 atlas 后缀
        /// </summary>
        public static readonly FrozenDictionary<string, string> PossibleSuffixMapping = new Dictionary<string, string>()
        {
            [".skel"] = ".atlas",
            [".skel.bytes"] = ".atlas.txt",
            [".json"] = ".atlas",
        }.ToFrozenDictionary();

        /// <summary>
        /// 日志器
        /// </summary>
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        protected readonly SpineObjectData _data;
        protected readonly ISkeleton _skeleton;
        protected readonly IAnimationState _animationState;
        protected readonly ISkeletonClipping _clipping;

        /// <summary>
        /// 皮肤加载情况, 不含 default 皮肤
        /// </summary>
        protected readonly Dictionary<string, bool> _skinLoadStatus;

        /// <summary>
        /// 构造 Spine 对象实例, 构造失败会抛出异常
        /// </summary>
        /// <param name="skelPath">skel 文件路径</param>
        /// <param name="atlasPath">atlas 文件路径, 为空时会根据 <paramref name="skelPath"/> 进行自动检测</param>
        /// <param name="version">要使用的运行时版本, 为空时会自动检测</param>
        public SpineObject(string skelPath, string? atlasPath = null, SpineVersion? version = null, TextureLoader? textureLoader = null)
        {
            if (string.IsNullOrWhiteSpace(skelPath)) throw new ArgumentException(skelPath, nameof(skelPath));
            if (!File.Exists(skelPath)) throw new FileNotFoundException($"{nameof(skelPath)} not found", skelPath);
            textureLoader ??= TextureLoader.DefaultLoader;

            SkelPath = Path.GetFullPath(skelPath);
            AssetsDir = Directory.GetParent(skelPath).FullName;
            Name = Path.GetFileNameWithoutExtension(skelPath);

            if (string.IsNullOrWhiteSpace(atlasPath))
            {
                try
                {
                    var (skelSuffix, atlasSuffix) = PossibleSuffixMapping.First(kv => skelPath.EndsWith(kv.Key, StringComparison.OrdinalIgnoreCase));
                    var basePath = skelPath.Substring(0, skelPath.Length - skelSuffix.Length);
                    atlasPath = basePath + atlasSuffix;
                    if (!File.Exists(atlasPath)) throw new FileNotFoundException("Matching atlas file not found", atlasPath);
                }
                catch (InvalidOperationException)
                {
                    throw new KeyNotFoundException($"Unrecognized skel file suffix");
                }
            }
            else if (!File.Exists(atlasPath)) throw new FileNotFoundException($"{nameof(atlasPath)} not found", atlasPath);
            AtlasPath = Path.GetFullPath(atlasPath);

            // 自动检测版本, 可能会抛出异常
            if (version is null)
            {
                try
                {
                    version = SpineVersion.GetVersion(skelPath);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Warn("Failed to detect version for skel {0}, try all available versions", skelPath);
                }
            }

            if (version is null)
            {
                // 从高版本向低版本逐一尝试
                foreach (var v in SpineVersion.RegisteredVersions.OrderDescending())
                {
                    try
                    {
                        _data = SpineObjectData.New(v, skelPath, atlasPath, textureLoader);
                        Version = v;
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                }

                // 依然加载不成功就只能报错
                if (_data is null || Version is null)
                    throw new InvalidDataException($"Failed to load spine by existed versions");
            }
            else
            {
                // 根据版本实例化对象
                Version = version;
                try
                {
                    _data = SpineObjectData.New(Version, skelPath, atlasPath, textureLoader);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    throw new InvalidDataException($"Failed to load spine with version '{version}'");
                }
            }

            // 创建状态实例
            _skeleton = _data.CreateSkeleton();
            _animationState = _data.CreateAnimationState();
            _clipping = _data.CreateSkeletonClipping();

            // 挂载一个空皮肤
            _skeleton.Skin = _data.CreateSkin(Guid.NewGuid().ToString());

            // 初始化皮肤加载情况, 不需要记录 default 的值
            _skinLoadStatus = _data.Skins.Select(it => it.Name).Where(it => it != "default").ToDictionary(it => it, it => false);

            // 必须更新一次, 否则部分内部参数还未生成
            Update(0);
        }

        /// <summary>
        /// 拷贝构造函数, 共用同一份资源数据, 可选是否保留除动画以外的实例属性状态, 与原对象拥有不同的 <c><see cref="ID"/></c>
        /// </summary>
        public SpineObject(SpineObject other, bool keepStates = false)
        {
            // 拷贝基本信息
            SkelPath = other.SkelPath;
            AssetsDir = other.AssetsDir;
            Name = other.Name;
            AtlasPath = other.AtlasPath;
            Version = other.Version;

            // 拷贝数据并且增加引用计数
            _data = other._data;
            _data.IncRef();

            // 新的实例
            _skeleton = _data.CreateSkeleton();
            _animationState = _data.CreateAnimationState();
            _clipping = _data.CreateSkeletonClipping();

            // 挂载一个空皮肤
            _skeleton.Skin = _data.CreateSkin(Guid.NewGuid().ToString());

            // 初始化皮肤加载情况, 不需要记录 default 的值
            _skinLoadStatus = _data.Skins.Select(it => it.Name).Where(it => it != "default").ToDictionary(it => it, it => false);

            if (keepStates)
            {
                // 拷贝骨骼状态
                _skeleton.X = other._skeleton.X;
                _skeleton.Y = other._skeleton.Y;
                _skeleton.ScaleX = other._skeleton.ScaleX;
                _skeleton.ScaleY = other._skeleton.ScaleY;

                // 拷贝渲染设置
                UsePma = other.UsePma;
                Physics = other.Physics;
                _animationState.TimeScale = other._animationState.TimeScale;

                // 拷贝皮肤加载情况
                _skinLoadStatus = other._skinLoadStatus.ToDictionary();
                ReloadSkins();

                // XXX(#105): 部分 3.4 模型此处可能导致预期外的插槽附件残留
                // 拷贝插槽属性值
                for (int i = 0; i < other._skeleton.Slots.Length; i++)
                {
                    _skeleton.Slots[i].Attachment = other._skeleton.Slots[i].Attachment;
                    _skeleton.Slots[i].Disabled = other._skeleton.Slots[i].Disabled;
                }

                // 拷贝调试属性
                EnableDebug = other.EnableDebug;
                DebugTexture = other.DebugTexture;
                DebugBounds = other.DebugBounds;
                DebugBones = other.DebugBones;
                DebugRegions = other.DebugRegions;
                DebugMeshHulls = other.DebugMeshHulls;
                DebugMeshes = other.DebugMeshes;
                DebugBoundingBoxes = other.DebugBoundingBoxes;
                DebugPaths = other.DebugPaths;
                DebugPoints = other.DebugPoints;
                DebugClippings = other.DebugClippings;
            }

            // 必须更新一次, 否则部分内部参数还未生成
            Update(0);
        }

        /// <summary>
        /// 数据对象
        /// </summary>
        public ISpineObjectData Data => _data;

        /// <summary>
        /// Skeleton 对象
        /// </summary>
        public ISkeleton Skeleton => _skeleton;

        /// <summary>
        /// AnimationState 对象
        /// </summary>
        public IAnimationState AnimationState => _animationState;

        /// <summary>
        /// 获取加载时用的版本
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
        /// 是否使用预乘 Alpha
        /// </summary>
        public bool UsePma { get; set; }

        /// <summary>
        /// 物理约束更新方式
        /// </summary>
        public ISkeleton.Physics Physics { get; set; } = ISkeleton.Physics.Update;

        /// <summary>
        /// 启用渲染调试, 将会使所有 <c>DebugXXX</c> 属性生效
        /// </summary>
        public bool EnableDebug { get; set; }

        /// <summary>
        /// 显示纹理
        /// </summary>
        public bool DebugTexture { get; set; } = true;

        /// <summary>
        /// 显示包围盒
        /// </summary>
        public bool DebugBounds { get; set; }

        /// <summary>
        /// 显示骨骼
        /// </summary>
        public bool DebugBones { get; set; }

        /// <summary>
        /// 显示区域附件边框
        /// </summary>
        public bool DebugRegions { get; set; }

        /// <summary>
        /// 显示网格附件边框线
        /// </summary>
        public bool DebugMeshHulls { get; set; }

        /// <summary>
        /// 显示网格附件网格线
        /// </summary>
        public bool DebugMeshes { get; set; }

        /// <summary>
        /// 显示碰撞盒附件边框线
        /// </summary>
        public bool DebugBoundingBoxes { get; set; }

        /// <summary>
        /// 显示路径附件网格线
        /// </summary>
        public bool DebugPaths { get; set; }

        /// <summary>
        /// 显示点附件
        /// </summary>
        public bool DebugPoints { get; set; }

        /// <summary>
        /// 显示剪裁附件网格线
        /// </summary>
        public bool DebugClippings { get; set; }

        /// <summary>
        /// 获取插槽可见性, 如果不存在则默认返回 false
        /// </summary>
        public bool GetSlotVisible(string slotName)
        {
            if (_skeleton.SlotsByName.TryGetValue(slotName, out var slot))
                return !slot.Disabled;
            return false;
        }

        /// <summary>
        /// 设置插槽可见性, 插槽不可见后将不会在任何渲染中出现, 插槽不存在则忽略操作
        /// </summary>
        /// <returns>操作是否成功, 插槽不存在则返回 false</returns>
        public bool SetSlotVisible(string slotName, bool visible)
        {
            if (_skeleton.SlotsByName.TryGetValue(slotName, out var slot))
            {
                slot.Disabled = !visible;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取某个插槽上的附件名, 插槽不存在或者无附件均返回 null
        /// </summary>
        public string? GetAttachment(string slotName)
        {
            if (_skeleton.SlotsByName.TryGetValue(slotName, out var slot))
                return slot.Attachment?.Name;
            return null;
        }

        /// <summary>
        /// 设置某个插槽的附件, 如果不存在则忽略, 可以使用 null 来尝试清除附件
        /// </summary>
        /// <returns>是否操作成功</returns>
        public bool SetAttachment(string slotName, string? attachmentName)
        {
            if (_skeleton.SlotsByName.TryGetValue(slotName, out var slot) && 
                _data.SlotAttachments.TryGetValue(slotName, out var slotAttachments))
            {
                if (attachmentName is null)
                    slot.Attachment = null;
                else if (slotAttachments.TryGetValue(attachmentName, out var attachment))
                    slot.Attachment = attachment;
                _skeleton.UpdateCache();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 查询皮肤加载状态, 皮肤不存在时返回 false
        /// </summary>
        public bool GetSkinStatus(string name) => name == "default" || _skinLoadStatus.TryGetValue(name, out var status) && status;

        /// <summary>
        /// 设置皮肤加载状态, 忽略不存在的皮肤
        /// </summary>
        /// <returns>是否操作成功</returns>
        public bool SetSkinStatus(string name, bool status)
        {
            if (!_skinLoadStatus.ContainsKey(name)) return false;
            _skinLoadStatus[name] = status;
            ReloadSkins();
            return true;
        }

        /// <summary>
        /// 刷新已加载皮肤, 会丢失自定义插槽附件设置
        /// </summary>
        public void ReloadSkins()
        {
            var skin = _skeleton.Skin ??= _data.CreateSkin(Guid.NewGuid().ToString());
            skin.Clear();
            foreach (var (name, _) in _skinLoadStatus.Where(e => e.Value)) 
                skin.AddSkin(_data.SkinsByName[name]);
            _skeleton.SetSlotsToSetupPose();
            _skeleton.UpdateCache();
        }

        /// <summary>
        /// 更新状态, 按顺序调用动画状态更新并应用到骨骼上更新
        /// </summary>
        public void Update(float delta)
        {
            _animationState.Update(delta);
            _animationState.Apply(_skeleton);
            _skeleton.Update(delta);
            _skeleton.UpdateWorldTransform(Physics);
        }

        #region SFML.Graphics.Drawable 接口实现

        /// <summary>
        /// 包围盒颜色
        /// </summary>
        public static SFML.Graphics.Color BoundsColor { get; set; } = new(120, 200, 0);

        /// <summary>
        /// 骨骼点颜色
        /// </summary>
        public static SFML.Graphics.Color BonePointColor { get; set; } = new(0, 255, 0);

        /// <summary>
        /// 骨骼线颜色
        /// </summary>
        public static SFML.Graphics.Color BoneLineColor { get; set; } = new(255, 0, 0);

        /// <summary>
        /// 网格线颜色
        /// </summary>
        public static SFML.Graphics.Color MeshLineColor { get; set; } = new(255, 163, 0, 128);

        /// <summary>
        /// 附件边框线颜色
        /// </summary>
        public static SFML.Graphics.Color AttachmentLineColor { get; set; } = new(0, 0, 255, 128);

        /// <summary>
        /// 剪裁附件边框线颜色
        /// </summary>
        public static SFML.Graphics.Color ClippingLineColor { get; set; } = new(204, 0, 0);

        /// <summary>
        /// spine 顶点坐标缓冲区
        /// </summary>
        protected float[] _worldVertices = new float[1024];

        /// <summary>
        /// 三角形顶点缓冲区
        /// </summary>
        protected readonly SFML.Graphics.VertexArray _triangleVertices = new(SFML.Graphics.PrimitiveType.Triangles);

        /// <summary>
        /// 无面积线条缓冲区
        /// </summary>
        protected readonly SFML.Graphics.VertexArray _lineVertices = new(SFML.Graphics.PrimitiveType.Lines);

        /// <summary>
        /// 有宽度线条缓冲区, 需要通过 <see cref="AddRectLine"/> 添加顶点
        /// </summary>
        protected readonly SFML.Graphics.VertexArray _rectLineVertices = new(SFML.Graphics.PrimitiveType.Quads);

        /// <summary>
        /// 有半径圆点临时缓存对象
        /// </summary>
        private readonly SFML.Graphics.CircleShape _circlePointShape = new();

        /// <summary>
        /// 绘制有半径的实心圆点, 随模型一起缩放大小
        /// </summary>
        protected void DrawCirclePoint(SFML.Graphics.RenderTarget target, SFML.System.Vector2f p, SFML.Graphics.Color color, float radius = 1)
        {
            _circlePointShape.Origin = new(radius, radius);
            _circlePointShape.Position = p;
            _circlePointShape.FillColor = color;
            _circlePointShape.Radius = radius;
            target.Draw(_circlePointShape);
        }

        /// <summary>
        /// 绘制有宽度的实心线, 会随模型一起缩放粗细, 顶点被存储在 <see cref="_rectLineVertices"/> 数组内
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

            v.Position = p1 + t; _rectLineVertices.Append(v);
            v.Position = p2 + t; _rectLineVertices.Append(v);
            v.Position = p2 - t; _rectLineVertices.Append(v);
            v.Position = p1 - t; _rectLineVertices.Append(v);
        }

        /// <summary>
        /// 渲染纹理 (正常渲染)
        /// </summary>
        protected void DrawTexture(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states)
        {
            _triangleVertices.Clear();
            states.Texture = null;
            states.Shader = UsePma ? SFMLShader.VertexAlphaPma : SFMLShader.VertexAlpha;

            foreach (var slot in _skeleton.IterDrawOrder())
            {
                if (slot.A <= 0 || !slot.Bone.Active || slot.Disabled)
                {
                    _clipping.ClipEnd(slot);
                    continue;
                }

                var attachment = slot.Attachment;

                float[] worldVertices;                          // 顶点世界坐标数组, 连续的 [x0, y0, x1, y1, ...] 坐标值
                int worldVerticesLength;                        // 顶点数组的长度
                int[] triangles;                                // 三角形索引, 从顶点坐标数组取的时候要乘以 2, 最大值是 worldVerticesCount - 1
                int trianglesLength;                            // 三角形索引数组长度
                float[] uvs;                                    // 纹理坐标数组, 连续的 [u0, v0, u1, v1, ...] 坐标值, 长度和顶点数组相同

                float tintR = _skeleton.R * slot.R;
                float tintG = _skeleton.G * slot.G;
                float tintB = _skeleton.B * slot.B;
                float tintA = _skeleton.A * slot.A;

                SFML.Graphics.Texture texture;

                switch (attachment)
                {
                    case IRegionAttachment regionAttachment:
                        worldVerticesLength = regionAttachment.ComputeWorldVertices(slot, ref _worldVertices);
                        worldVertices = _worldVertices;
                        triangles = regionAttachment.Triangles;
                        trianglesLength = triangles.Length;
                        uvs = regionAttachment.UVs;
                        tintR *= regionAttachment.R;
                        tintG *= regionAttachment.G;
                        tintB *= regionAttachment.B;
                        tintA *= regionAttachment.A;

                        // NOTE: RenderObject 的获取要在 ComputeWorldVertices 发生之后, 否则可能存在某些 Region 尚未被赋值产生 null 引用报错
                        texture = regionAttachment.RendererObject;
                        break;
                    case IMeshAttachment meshAttachment:
                        worldVerticesLength = meshAttachment.ComputeWorldVertices(slot, ref _worldVertices);
                        worldVertices = _worldVertices;
                        triangles = meshAttachment.Triangles;
                        trianglesLength = triangles.Length;
                        uvs = meshAttachment.UVs;
                        tintR *= meshAttachment.R;
                        tintG *= meshAttachment.G;
                        tintB *= meshAttachment.B;
                        tintA *= meshAttachment.A;
                        texture = meshAttachment.RendererObject;
                        break;
                    case IClippingAttachment clippingAttachment:
                        _clipping.ClipStart(slot, clippingAttachment);
                        continue;
                    default:
                        _clipping.ClipEnd(slot);
                        continue;
                }

                // 纹理或者混合模式发生变化则立即渲染
                var blendMode = slot.Blend;
                states.Texture ??= texture;
                if (states.BlendMode != blendMode || states.Texture != texture)
                {
                    if (_triangleVertices.VertexCount > 0)
                    {
                        target.Draw(_triangleVertices, states);
                        _triangleVertices.Clear();
                    }
                    states.BlendMode = blendMode;
                    states.Texture = texture;
                }

                if (_clipping.IsClipping)
                {
                    _clipping.ClipTriangles(worldVertices, worldVerticesLength, triangles, trianglesLength, uvs);
                    worldVertices = _clipping.ClippedVertices;
                    worldVerticesLength = _clipping.ClippedVerticesLength;
                    triangles = _clipping.ClippedTriangles;
                    trianglesLength = _clipping.ClippedTrianglesLength;
                    uvs = _clipping.ClippedUVs;
                }

                var texW = texture.Size.X;
                var texH = texture.Size.Y;

                SFML.Graphics.Vertex vt = new();
                vt.Color.R = (byte)(tintR * 255);
                vt.Color.G = (byte)(tintG * 255);
                vt.Color.B = (byte)(tintB * 255);
                vt.Color.A = (byte)(tintA * 255);

                for (int i = 0; i < trianglesLength; i++)
                {
                    var index = triangles[i] << 1;
                    vt.Position.X = worldVertices[index];
                    vt.Position.Y = worldVertices[index + 1];
                    vt.TexCoords.X = uvs[index] * texW;
                    vt.TexCoords.Y = uvs[index + 1] * texH;
                    _triangleVertices.Append(vt);
                }

                _clipping.ClipEnd(slot);
            }
            _clipping.ClipEnd();

            target.Draw(_triangleVertices, states);
        }

        /// <summary>
        /// 渲染调试内容
        /// </summary>
        protected void DrawNonTexture(SFML.Graphics.RenderTarget target)
        {
            _lineVertices.Clear();
            _rectLineVertices.Clear();
            SFML.Graphics.Vertex vt = new();

            if (DebugRegions)
            {
                vt.Color = AttachmentLineColor;
                foreach (var slot in _skeleton.IterDrawOrder().Where(s => s.A > 0 && s.Bone.Active && !s.Disabled))
                {
                    if (slot.Attachment is IRegionAttachment regionAttachment)
                    {
                        regionAttachment.ComputeWorldVertices(slot, ref _worldVertices);

                        vt.Position.X = _worldVertices[0];
                        vt.Position.Y = _worldVertices[1];
                        _lineVertices.Append(vt);

                        vt.Position.X = _worldVertices[2];
                        vt.Position.Y = _worldVertices[3];
                        _lineVertices.Append(vt); _lineVertices.Append(vt);

                        vt.Position.X = _worldVertices[4];
                        vt.Position.Y = _worldVertices[5];
                        _lineVertices.Append(vt); _lineVertices.Append(vt);

                        vt.Position.X = _worldVertices[6];
                        vt.Position.Y = _worldVertices[7];
                        _lineVertices.Append(vt); _lineVertices.Append(vt);

                        vt.Position.X = _worldVertices[0];
                        vt.Position.Y = _worldVertices[1];
                        _lineVertices.Append(vt);
                    }
                }
            }

            if (DebugMeshes)
            {
                vt.Color = MeshLineColor;
                foreach (var slot in _skeleton.IterDrawOrder().Where(s => s.A > 0 && s.Bone.Active && !s.Disabled))
                {
                    if (slot.Attachment is IMeshAttachment meshAttachment)
                    {
                        meshAttachment.ComputeWorldVertices(slot, ref _worldVertices);

                        var triangles = meshAttachment.Triangles;
                        for (int i = 0; i < triangles.Length - 2; i += 3)
                        {
                            var idx0 = triangles[i] << 1;
                            var idx1 = triangles[i + 1] << 1;
                            var idx2 = triangles[i + 2] << 1;

                            vt.Position.X = _worldVertices[idx0];
                            vt.Position.Y = _worldVertices[idx0 + 1];
                            _lineVertices.Append(vt);

                            vt.Position.X = _worldVertices[idx1];
                            vt.Position.Y = _worldVertices[idx1 + 1];
                            _lineVertices.Append(vt); _lineVertices.Append(vt);

                            vt.Position.X = _worldVertices[idx2];
                            vt.Position.Y = _worldVertices[idx2 + 1];
                            _lineVertices.Append(vt); _lineVertices.Append(vt);

                            vt.Position.X = _worldVertices[idx0];
                            vt.Position.Y = _worldVertices[idx0 + 1];
                            _lineVertices.Append(vt);
                        }
                    }
                }
            }

            if (DebugMeshHulls)
            {
                vt.Color = AttachmentLineColor;
                foreach (var slot in _skeleton.IterDrawOrder().Where(s => s.A > 0 && s.Bone.Active && !s.Disabled))
                {
                    if (slot.Attachment is IMeshAttachment meshAttachment)
                    {
                        meshAttachment.ComputeWorldVertices(slot, ref _worldVertices);

                        var hullLength = meshAttachment.HullLength;
                        if (hullLength < 4) continue;

                        vt.Position.X = _worldVertices[0];
                        vt.Position.Y = _worldVertices[1];
                        _lineVertices.Append(vt);

                        for (int i = 2; i < hullLength - 1; i += 2)
                        {
                            vt.Position.X = _worldVertices[i];
                            vt.Position.Y = _worldVertices[i + 1];
                            _lineVertices.Append(vt);
                            _lineVertices.Append(vt);
                        }

                        vt.Position.X = _worldVertices[0];
                        vt.Position.Y = _worldVertices[1];
                        _lineVertices.Append(vt);
                    }
                }
            }

            if (DebugBoundingBoxes)
            {
                //throw new NotImplementedException();
            }

            if (DebugPaths)
            {
                //throw new NotImplementedException();
            }

            if (DebugPoints)
            {
                //throw new NotImplementedException();
            }

            if (DebugClippings)
            {
                vt.Color = ClippingLineColor;
                foreach (var slot in _skeleton.IterDrawOrder().Where(s => s.A > 0 && s.Bone.Active && !s.Disabled))
                {
                    if (slot.Attachment is IClippingAttachment clippingAttachment)
                    {
                        int length = clippingAttachment.ComputeWorldVertices(slot, ref _worldVertices);

                        vt.Position.X = _worldVertices[0];
                        vt.Position.Y = _worldVertices[1];
                        _lineVertices.Append(vt);

                        for (int i = 2; i < length - 1; i += 2)
                        {
                            vt.Position.X = _worldVertices[i];
                            vt.Position.Y = _worldVertices[i + 1];
                            _lineVertices.Append(vt);
                            _lineVertices.Append(vt);
                        }

                        vt.Position.X = _worldVertices[0];
                        vt.Position.Y = _worldVertices[1];
                        _lineVertices.Append(vt);
                    }
                }
            }

            if (DebugBounds)
            {
                vt.Color = BoundsColor;
                _skeleton.GetBounds(out var x1, out var y1, out var w, out var h);
                var x2 = x1 + w;
                var y2 = y1 + h;

                vt.Position.X = x1;
                vt.Position.Y = y1;
                _lineVertices.Append(vt);

                vt.Position.X = x2;
                vt.Position.Y = y1;
                _lineVertices.Append(vt); _lineVertices.Append(vt);

                vt.Position.X = x2;
                vt.Position.Y = y2;
                _lineVertices.Append(vt); _lineVertices.Append(vt);

                vt.Position.X = x1;
                vt.Position.Y = y2;
                _lineVertices.Append(vt); _lineVertices.Append(vt);

                vt.Position.X = x1;
                vt.Position.Y = y1;
                _lineVertices.Append(vt);
            }

            // 骨骼线放最后画
            if (DebugBones)
            {
                var width = Math.Max(Math.Abs(_skeleton.ScaleX), Math.Abs(_skeleton.ScaleY));
                foreach (var bone in _skeleton.IterDrawOrder().Where(s => s.A > 0 && s.Bone.Active && !s.Disabled).Select(st => st.Bone))
                {
                    var boneLength = bone.Length;
                    var p1 = new SFML.System.Vector2f(bone.WorldX, bone.WorldY);
                    var p2 = new SFML.System.Vector2f(bone.WorldX + boneLength * bone.A, bone.WorldY + boneLength * bone.C);
                    AddRectLine(p1, p2, BoneLineColor, width);
                }
            }

            target.Draw(_lineVertices);
            target.Draw(_rectLineVertices);

            // 骨骼的点最后画, 层级处于骨骼线上面
            if (DebugBones)
            {
                var radius = Math.Max(Math.Abs(_skeleton.ScaleX), Math.Abs(_skeleton.ScaleY));
                foreach (var bone in _skeleton.IterDrawOrder().Where(s => s.A > 0 && s.Bone.Active && !s.Disabled).Select(st => st.Bone))
                {
                    DrawCirclePoint(target, new(bone.WorldX, bone.WorldY), BonePointColor, radius);
                }
            }
        }

        /// <summary>
        /// SFML.Graphics.Drawable 接口实现
        /// <para>这个渲染实现绘制出来的像素将是预乘的, 当渲染的背景透明度是 1 时, 则等价于非预乘的结果, 即正常画面, 否则画面偏暗</para>
        /// <para>可以用于 <see cref="SFML.Graphics.RenderWindow"/> 的渲染, 因为直接在窗口上绘制时窗口始终是不透明的</para>
        /// </summary>
        public virtual void Draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states)
        {
            if (_disposed) return;

            if (!EnableDebug)
            {
                DrawTexture(target, states);
            }
            else
            {
                if (DebugTexture) DrawTexture(target, states);
                DrawNonTexture(target);
            }
        }

        #endregion

        #region IDisposable 接口实现

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _data.Dispose();
                _triangleVertices.Dispose();
                _lineVertices.Dispose();
                _rectLineVertices.Dispose();
                _circlePointShape.Dispose();
            }
            _disposed = true;
        }

        ~SpineObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            if (_disposed)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
