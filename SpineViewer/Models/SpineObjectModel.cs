using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using Spine;
using Spine.SpineWrappers;
using SpineViewer.Extensions;

namespace SpineViewer.Models
{
    /// <summary>
    /// 线程安全的模型对象类
    /// </summary>
    public class SpineObjectModel : ObservableObject, SFML.Graphics.Drawable, IDisposable
    {
        /// <summary>
        /// 日志器
        /// </summary>
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly object _lock = new();
        private readonly SpineObject _spineObject;

        private readonly ImmutableArray<string> _skins;
        private readonly FrozenDictionary<string, ImmutableArray<string>> _slotAttachments;
        private readonly ImmutableArray<string> _animations;

        /// <summary>
        /// 构造函数, 可能会抛出异常
        /// </summary>
        public SpineObjectModel(string skelPath, string? atlasPath = null, SpineVersion? version = null)
        {
            _spineObject = new(skelPath, atlasPath, version) { DebugNonTexture = false };
            _skins = _spineObject.Data.Skins.Select(v => v.Name).ToImmutableArray();
            _slotAttachments = _spineObject.Data.SlotAttachments.ToFrozenDictionary(it => it.Key, it => it.Value.Keys);
            _animations = _spineObject.Data.Animations.Select(v => v.Name).ToImmutableArray();

            // 默认自带一个动画
            if (_spineObject.Data.Animations.Length > 0)
                _spineObject.AnimationState.SetAnimation(0, _spineObject.Data.Animations[0], true);
        }

        public event EventHandler<SkinStatusChangedEventArgs>? SkinStatusChanged;

        public event EventHandler<SlotAttachmentChangedEventArgs>? SlotAttachmentChanged;

        public event EventHandler<AnimationChangedEventArgs>? AnimationChanged;

        public SpineVersion Version => _spineObject.Version;

        public string AssetsDir => _spineObject.AssetsDir;

        public string SkelPath => _spineObject.SkelPath;

        public string AtlasPath => _spineObject.AtlasPath;

        public string Name => _spineObject.Name;

        public string FileVersion => _spineObject.Data.SkeletonVersion;

        public bool IsSelected
        {
            get { lock (_lock) return _isSelected; }
            set { lock (_lock) if (SetProperty(ref _isSelected, value)) _spineObject.DebugNonTexture = _isSelected; }
        }
        private bool _isSelected = false;

        public bool IsShown
        {
            get { lock (_lock) return _isShown; }
            set { lock (_lock) SetProperty(ref _isShown, value); }
        }
        private bool _isShown = true;

        public bool UsePma
        {
            get { lock (_lock) return _spineObject.UsePma; }
            set { lock (_lock) SetProperty(_spineObject.UsePma, value, _spineObject, (m, v) => m.UsePma = v); }
        }

        public ISkeleton.Physics Physics
        {
            get { lock (_lock) return _spineObject.Physics; }
            set { lock (_lock) SetProperty(_spineObject.Physics, value, _spineObject, (m, v) => m.Physics = v); }
        }

        /// <summary>
        /// 缩放倍数, 绝对值大小, 两个方向大小不一致时返回 -1, 设置时不会影响正负号
        /// </summary>
        public float Scale
        {
            get
            {
                lock (_lock)
                {
                    var x = Math.Abs(_spineObject.Skeleton.ScaleX);
                    var y = Math.Abs(_spineObject.Skeleton.ScaleY);
                    return Math.Abs(x - y) < 1e-6 ? x : -1;
                }
            }
            set
            {
                lock (_lock)
                {
                    value = Math.Clamp(value, 0.001f, 1000f);
                    _spineObject.Skeleton.ScaleX = value * Math.Sign(_spineObject.Skeleton.ScaleX);
                    _spineObject.Skeleton.ScaleY = value * Math.Sign(_spineObject.Skeleton.ScaleY);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 水平翻转
        /// </summary>
        public bool FlipX
        {
            get { lock (_lock) return _spineObject.Skeleton.ScaleX < 0; }
            set { lock (_lock) SetProperty(_spineObject.Skeleton.ScaleX < 0, value, _spineObject, (m, v) => m.Skeleton.ScaleX *= -1); }
        }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        public bool FlipY
        {
            get { lock (_lock) return _spineObject.Skeleton.ScaleY < 0; }
            set { lock (_lock) SetProperty(_spineObject.Skeleton.ScaleY < 0, value, _spineObject, (m, v) => m.Skeleton.ScaleY *= -1); }
        }

        public float X
        {
            get { lock (_lock) return _spineObject.Skeleton.X; }
            set { lock (_lock) SetProperty(_spineObject.Skeleton.X, value, _spineObject, (m, v) => m.Skeleton.X = v); }
        }

        public float Y
        {
            get { lock (_lock) return _spineObject.Skeleton.Y; }
            set { lock (_lock) SetProperty(_spineObject.Skeleton.Y, value, _spineObject, (m, v) => m.Skeleton.Y = v); }
        }

        public ImmutableArray<string> Skins => _skins;

        public bool GetSkinStatus(string skinName)
        {
            lock (_lock) return _spineObject.GetSkinStatus(skinName);
        }

        public bool SetSkinStatus(string skinName, bool status)
        {
            bool changed = false;
            lock (_lock) changed = _spineObject.SetSkinStatus(skinName, status);
            if (changed) SkinStatusChanged?.Invoke(this, new(skinName, status));
            return changed;
        }

        public FrozenDictionary<string, ImmutableArray<string>> SlotAttachments => _slotAttachments;

        public string? GetAttachment(string slotName)
        {
            lock (_lock) return _spineObject.GetAttachment(slotName);
        }

        public bool SetAttachment(string slotName, string? attachmentName)
        {
            bool changed = false;
            lock (_lock) changed = _spineObject.SetAttachment(slotName, attachmentName);
            if (changed) SlotAttachmentChanged?.Invoke(this, new(slotName, attachmentName));
            return changed;
        }

        public ImmutableArray<string> Animations => _animations;

        public float GetAnimationDuration(string name)
        {
            if (_spineObject.Data.AnimationsByName.TryGetValue(name, out var ani))
                return ani.Duration;
            return 0f;
        }

        public string? GetAnimation(int index)
        {
            lock (_lock) return _spineObject.AnimationState.GetCurrent(index)?.Animation.Name;
        }

        /// <summary>
        /// 在指定轨道上设置循环动画, 会忽略不存在的动画
        /// </summary>
        public void SetAnimation(int index, string name)
        {
            bool changed = false;
            lock (_lock)
            {
                if (_spineObject.Data.AnimationsByName.ContainsKey(name))
                {
                    _spineObject.AnimationState.SetAnimation(index, name, true);
                    changed = true;
                }
            }
            if (changed) AnimationChanged?.Invoke(this, new(index, name));
        }

        public int[] GetTrackIndices()
        {
            lock (_lock)
            {
                List<int> indices = [];
                int idx = 0;
                foreach (var e in _spineObject.AnimationState.IterTracks())
                {
                    if (e is not null) indices.Add(idx);
                    idx++;
                }
                return indices.ToArray();
            }
        }

        public void ClearTrack(int index)
        {
            lock (_lock) _spineObject.AnimationState.ClearTrack(index);
            AnimationChanged?.Invoke(this, new(index, null));
        }

        public void ResetAnimationsTime()
        {
            lock (_lock) _spineObject.ResetAnimationsTime();
        }

        public bool EnableDebug
        {
            get { lock (_lock) return _spineObject.EnableDebug; }
            set { lock (_lock) SetProperty(_spineObject.EnableDebug, value, _spineObject, (m, v) => m.EnableDebug = v); }
        }

        public bool DebugTexture
        {
            get { lock (_lock) return _spineObject.DebugTexture; }
            set { lock (_lock) SetProperty(_spineObject.DebugTexture, value, _spineObject, (m, v) => m.DebugTexture = v); }
        }

        public bool DebugBounds
        {
            get { lock (_lock) return _spineObject.DebugBounds; }
            set { lock (_lock) SetProperty(_spineObject.DebugBounds, value, _spineObject, (m, v) => m.DebugBounds = v); }
        }

        public bool DebugBones
        {
            get { lock (_lock) return _spineObject.DebugBones; }
            set { lock (_lock) SetProperty(_spineObject.DebugBones, value, _spineObject, (m, v) => m.DebugBones = v); }
        }

        public bool DebugRegions
        {
            get { lock (_lock) return _spineObject.DebugRegions; }
            set { lock (_lock) SetProperty(_spineObject.DebugRegions, value, _spineObject, (m, v) => m.DebugRegions = v); }
        }

        public bool DebugMeshHulls
        {
            get { lock (_lock) return _spineObject.DebugMeshHulls; }
            set { lock (_lock) SetProperty(_spineObject.DebugMeshHulls, value, _spineObject, (m, v) => m.DebugMeshHulls = v); }
        }

        public bool DebugMeshes
        {
            get { lock (_lock) return _spineObject.DebugMeshes; }
            set { lock (_lock) SetProperty(_spineObject.DebugMeshes, value, _spineObject, (m, v) => m.DebugMeshes = v); }
        }

        public bool DebugBoundingBoxes
        {
            get { lock (_lock) return _spineObject.DebugBoundingBoxes; }
            set { lock (_lock) SetProperty(_spineObject.DebugBoundingBoxes, value, _spineObject, (m, v) => m.DebugBoundingBoxes = v); }
        }

        public bool DebugPaths
        {
            get { lock (_lock) return _spineObject.DebugPaths; }
            set { lock (_lock) SetProperty(_spineObject.DebugPaths, value, _spineObject, (m, v) => m.DebugPaths = v); }
        }

        public bool DebugPoints
        {
            get { lock (_lock) return _spineObject.DebugPoints; }
            set { lock (_lock) SetProperty(_spineObject.DebugPoints, value, _spineObject, (m, v) => m.DebugPoints = v); }
        }

        public bool DebugClippings
        {
            get { lock (_lock) return _spineObject.DebugClippings; }
            set { lock (_lock) SetProperty(_spineObject.DebugClippings, value, _spineObject, (m, v) => m.DebugClippings = v); }
        }

        public void Update(float delta)
        {
            lock (_lock) _spineObject.Update(delta);
        }

        /// <summary>
        /// 获取一个独立的内部对象, 继承所有状态
        /// </summary>
        public SpineObject GetSpineObject(bool keepTrackTime = false)
        {
            lock (_lock) return _spineObject.Copy(keepTrackTime);
        }

        /// <summary>
        /// 获取当前状态包围盒
        /// </summary>
        public Rect GetCurrentBounds()
        {
            lock (_lock) return _spineObject.GetCurrentBounds();
        }

        /// <summary>
        /// 导出参数对象
        /// </summary>
        public SpineObjectConfigModel Dump()
        {
            lock (_lock)
            {
                SpineObjectConfigModel config = new()
                {
                    Scale = Math.Abs(_spineObject.Skeleton.ScaleX),
                    FlipX = _spineObject.Skeleton.ScaleX < 0,
                    FlipY = _spineObject.Skeleton.ScaleY < 0,
                    X = _spineObject.Skeleton.X,
                    Y = _spineObject.Skeleton.Y,
                    UsePma = _spineObject.UsePma,
                    Physics = _spineObject.Physics.ToString(),

                    DebugTexture = _spineObject.DebugTexture,
                    DebugBounds = _spineObject.DebugBounds,
                    DebugBones = _spineObject.DebugBones,
                    DebugRegions = _spineObject.DebugRegions,
                    DebugMeshHulls = _spineObject.DebugMeshHulls,
                    DebugMeshes = _spineObject.DebugMeshes,
                    DebugBoundingBoxes = _spineObject.DebugBoundingBoxes,
                    DebugPaths = _spineObject.DebugPaths,
                    DebugPoints = _spineObject.DebugPoints,
                    DebugClippings = _spineObject.DebugClippings
                };

                config.LoadedSkins.AddRange(_spineObject.Data.Skins.Select(it => it.Name).Where(_spineObject.GetSkinStatus));

                foreach (var slot in _spineObject.Skeleton.Slots) config.SlotAttachment[slot.Name] = slot.Attachment?.Name;

                // XXX: 处理空动画
                config.Animations.AddRange(_spineObject.AnimationState.IterTracks().Select(tr => tr?.Animation.Name));

                return config;
            }
        }

        /// <summary>
        /// 从参数对象加载参数值
        /// </summary>
        public void Load(SpineObjectConfigModel config)
        {
            lock (_lock)
            {
                _spineObject.Skeleton.ScaleX = config.Scale;
                _spineObject.Skeleton.ScaleY = config.Scale;
                OnPropertyChanged(nameof(Scale));
                SetProperty(_spineObject.Skeleton.ScaleX < 0, config.FlipX, _spineObject, (m, v) => m.Skeleton.ScaleX *= -1, nameof(FlipX));
                SetProperty(_spineObject.Skeleton.ScaleY < 0, config.FlipY, _spineObject, (m, v) => m.Skeleton.ScaleY *= -1, nameof(FlipY));
                SetProperty(_spineObject.Skeleton.X, config.X, _spineObject, (m, v) => m.Skeleton.X = v, nameof(X));
                SetProperty(_spineObject.Skeleton.Y, config.Y, _spineObject, (m, v) => m.Skeleton.Y = v, nameof(Y));
                SetProperty(_spineObject.UsePma, config.UsePma, _spineObject, (m, v) => m.UsePma = v, nameof(UsePma));
                SetProperty(_spineObject.Physics, Enum.Parse<ISkeleton.Physics>(config.Physics ?? "Update", true), _spineObject, (m, v) => m.Physics = v, nameof(Physics));

                foreach (var name in _spineObject.Data.Skins.Select(v => v.Name).Except(config.LoadedSkins))
                    if (_spineObject.SetSkinStatus(name, false))
                        SkinStatusChanged?.Invoke(this, new(name, false));
                foreach (var name in config.LoadedSkins)
                    if (_spineObject.SetSkinStatus(name, true))
                        SkinStatusChanged?.Invoke(this, new(name, true));

                foreach (var (slotName, attachmentName) in config.SlotAttachment)
                    if (_spineObject.SetAttachment(slotName, attachmentName))
                        SlotAttachmentChanged?.Invoke(this, new(slotName, attachmentName));

                // XXX: 处理空动画
                _spineObject.AnimationState.ClearTracks();
                int trackIndex = 0;
                foreach (var name in config.Animations)
                {
                    if (!string.IsNullOrEmpty(name))
                        _spineObject.AnimationState.SetAnimation(trackIndex, name, true);
                    AnimationChanged?.Invoke(this, new(trackIndex, name));
                    trackIndex++;
                }

                SetProperty(_spineObject.DebugTexture, config.DebugTexture, _spineObject, (m, v) => m.DebugTexture = v, nameof(DebugTexture));
                SetProperty(_spineObject.DebugBounds, config.DebugBounds, _spineObject, (m, v) => m.DebugBounds = v, nameof(DebugBounds));
                SetProperty(_spineObject.DebugBones, config.DebugBones, _spineObject, (m, v) => m.DebugBones = v, nameof(DebugBones));
                SetProperty(_spineObject.DebugRegions, config.DebugRegions, _spineObject, (m, v) => m.DebugRegions = v, nameof(DebugRegions));
                SetProperty(_spineObject.DebugMeshHulls, config.DebugMeshHulls, _spineObject, (m, v) => m.DebugMeshHulls = v, nameof(DebugMeshHulls));
                SetProperty(_spineObject.DebugMeshes, config.DebugMeshes, _spineObject, (m, v) => m.DebugMeshes = v, nameof(DebugMeshes));
                SetProperty(_spineObject.DebugBoundingBoxes, config.DebugBoundingBoxes, _spineObject, (m, v) => m.DebugBoundingBoxes = v, nameof(DebugBoundingBoxes));
                SetProperty(_spineObject.DebugPaths, config.DebugPaths, _spineObject, (m, v) => m.DebugPaths = v, nameof(DebugPaths));
                SetProperty(_spineObject.DebugPoints, config.DebugPoints, _spineObject, (m, v) => m.DebugPoints = v, nameof(DebugPoints));
                SetProperty(_spineObject.DebugClippings, config.DebugClippings, _spineObject, (m, v) => m.DebugClippings = v, nameof(DebugClippings));
            }
        }

        #region SFML.Graphics.Drawable 接口实现

        public void Draw(SFML.Graphics.RenderTarget target, SFML.Graphics.RenderStates states)
        {
            lock (_lock) _spineObject.Draw(target, states);
        }

        #endregion

        #region IDisposable 接口实现

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _spineObject.Dispose();
            }
            _disposed = true;
        }

        ~SpineObjectModel()
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

    public class SkinStatusChangedEventArgs(string name, bool status) : EventArgs
    {
        public string Name { get; } = name;
        public bool Status { get; } = status;
    }

    public class SlotAttachmentChangedEventArgs(string slotName, string? attachmentName) : EventArgs
    {
        public string SlotName { get; } = slotName;
        public string? AttachmentName { get; } = attachmentName;
    }

    public class AnimationChangedEventArgs(int trackIndex, string? animationName) : EventArgs
    {
        public int TrackIndex { get; } = trackIndex;
        public string? AnimationName { get; } = animationName;
    }
}
