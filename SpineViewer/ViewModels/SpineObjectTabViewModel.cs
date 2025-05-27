using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spine;
using Spine.SpineWrappers;
using SpineViewer.Models;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace SpineViewer.ViewModels
{
    public class SpineObjectTabViewModel : ObservableObject
    {
        private SpineObjectModel[] _selectedObjects = [];
        private readonly ObservableCollection<SkinViewModel> _skins = [];
        private readonly ObservableCollection<SlotAttachmentViewModel> _slots = [];
        private readonly ObservableCollection<AnimationTrackViewModel> _animationTracks = [];

        public ImmutableArray<ISkeleton.Physics> PhysicsOptions { get; } = Enum.GetValues<ISkeleton.Physics>().ToImmutableArray();

        public SpineObjectModel[] SelectedObjects
        {
            get => _selectedObjects;
            set
            {
                if (ReferenceEquals(_selectedObjects, value)) return;

                // 清空之前的所有内容
                foreach (var obj in _selectedObjects)
                {
                    obj.PropertyChanged -= SingleModel_PropertyChanged;
                    obj.AnimationChanged -= SingleModel_AnimationChanged;
                }
                _skins.Clear();
                _slots.Clear();
                _animationTracks.Clear();

                // 生成新的内容
                _selectedObjects = value ?? [];
                if (_selectedObjects.Length > 0)
                {
                    foreach (var obj in _selectedObjects)
                    {
                        obj.PropertyChanged += SingleModel_PropertyChanged;
                        obj.AnimationChanged += SingleModel_AnimationChanged;
                    }

                    IEnumerable<string> commonSkinNames = _selectedObjects[0].Skins;
                    foreach (var obj in _selectedObjects.Skip(1)) commonSkinNames = commonSkinNames.Intersect(obj.Skins);
                    foreach (var name in commonSkinNames) _skins.Add(new(name, _selectedObjects));

                    IEnumerable<string> commonSlotNames = _selectedObjects[0].SlotAttachments.Keys;
                    foreach (var obj in _selectedObjects.Skip(1)) commonSlotNames = commonSlotNames.Intersect(obj.SlotAttachments.Keys);
                    foreach (var name in commonSlotNames) _slots.Add(new(name, _selectedObjects));

                    IEnumerable<int> commonTrackIndices = _selectedObjects[0].GetTrackIndices();
                    foreach (var obj in _selectedObjects.Skip(1)) commonTrackIndices = commonTrackIndices.Intersect(obj.GetTrackIndices());
                    foreach (var idx in commonTrackIndices) _animationTracks.Add(new(idx, _selectedObjects));
                }

                OnPropertyChanged();

                Cmd_AppendTrack.NotifyCanExecuteChanged();

                OnPropertyChanged(nameof(Version));
                OnPropertyChanged(nameof(AssetsDir));
                OnPropertyChanged(nameof(SkelPath));
                OnPropertyChanged(nameof(AtlasPath));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(FileVersion));

                OnPropertyChanged(nameof(IsShown));
                OnPropertyChanged(nameof(UsePma));
                OnPropertyChanged(nameof(Physics));

                OnPropertyChanged(nameof(Scale));
                OnPropertyChanged(nameof(FlipX));
                OnPropertyChanged(nameof(FlipY));
                OnPropertyChanged(nameof(X));
                OnPropertyChanged(nameof(Y));

                OnPropertyChanged(nameof(DebugTexture));
                OnPropertyChanged(nameof(DebugBounds));
                OnPropertyChanged(nameof(DebugBones));
                OnPropertyChanged(nameof(DebugRegions));
                OnPropertyChanged(nameof(DebugMeshHulls));
                OnPropertyChanged(nameof(DebugMeshes));
                OnPropertyChanged(nameof(DebugBoundingBoxes));
                OnPropertyChanged(nameof(DebugPaths));
                OnPropertyChanged(nameof(DebugPoints));
                OnPropertyChanged(nameof(DebugClippings));
            }
        }

        public SpineVersion? Version
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].Version;
                if (_selectedObjects.Skip(1).Any(it => it.Version != val)) return null;
                return val;
            }
        }

        public string? AssetsDir
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].AssetsDir;
                if (_selectedObjects.Skip(1).Any(it => it.AssetsDir != val)) return null;
                return val;
            }
        }

        public string? SkelPath
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].SkelPath;
                if (_selectedObjects.Skip(1).Any(it => it.SkelPath != val)) return null;
                return val;
            }
        }

        public string? AtlasPath
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].AtlasPath;
                if (_selectedObjects.Skip(1).Any(it => it.AtlasPath != val)) return null;
                return val;
            }
        }

        public string? Name
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].Name;
                if (_selectedObjects.Skip(1).Any(it => it.Name != val)) return null;
                return val;
            }
        }

        public string? FileVersion
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].FileVersion;
                if (_selectedObjects.Skip(1).Any(it => it.FileVersion != val)) return null;
                return val;
            }
        }

        public bool? IsShown
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].IsShown;
                if (_selectedObjects.Skip(1).Any(it => it.IsShown != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.IsShown = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? UsePma
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].UsePma;
                if (_selectedObjects.Skip(1).Any(it => it.UsePma != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.UsePma = (bool)value;
                OnPropertyChanged();
            }
        }

        public ISkeleton.Physics? Physics
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].Physics;
                if (_selectedObjects.Skip(1).Any(it => it.Physics != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.Physics = (ISkeleton.Physics)value;
                OnPropertyChanged();
            }
        }

        public float? Scale
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].Scale;
                if (_selectedObjects.Skip(1).Any(it => it.Scale != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.Scale = (float)value;
                OnPropertyChanged();
            }
        }

        public bool? FlipX
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].FlipX;
                if (_selectedObjects.Skip(1).Any(it => it.FlipX != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.FlipX = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? FlipY
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].FlipY;
                if (_selectedObjects.Skip(1).Any(it => it.FlipY != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.FlipY = (bool)value;
                OnPropertyChanged();
            }
        }

        public float? X
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].X;
                if (_selectedObjects.Skip(1).Any(it => it.X != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.X = (float)value;
                OnPropertyChanged();
            }
        }

        public float? Y
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].Y;
                if (_selectedObjects.Skip(1).Any(it => it.Y != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.Y = (float)value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SkinViewModel> Skins => _skins;

        public RelayCommand<IList?> Cmd_EnableSkins { get; } = new(
            args => { if (args is null) return; foreach (var s in args.OfType<SkinViewModel>()) s.Status = true; },
            args => { return args is not null && args.OfType<SkinViewModel>().Any(); }
        );

        public RelayCommand<IList?> Cmd_DisableSkins { get; } = new(
            args => { if (args is null) return; foreach (var s in args.OfType<SkinViewModel>()) s.Status = false; },
            args => { return args is not null && args.OfType<SkinViewModel>().Any(); }
        );

        public ObservableCollection<SlotAttachmentViewModel> Slots => _slots;

        public RelayCommand<IList?> Cmd_ClearSlotsAttachment { get; } = new(
            args => { if (args is null) return; foreach (var s in args.OfType<SlotAttachmentViewModel>()) s.AttachmentName = null; },
            args => { return args is not null && args.OfType<SlotAttachmentViewModel>().Any(); }
        );

        public ObservableCollection<AnimationTrackViewModel> AnimationTracks => _animationTracks;

        public RelayCommand Cmd_AppendTrack => _cmd_AppendTrack ??= new(
            () =>
            {
                if (_selectedObjects.Length != 1) return;
                var sp = _selectedObjects[0];
                if (sp.Animations.Length <= 0) return;
                sp.SetAnimation(sp.GetTrackIndices().LastOrDefault(-1) + 1, sp.Animations[0]);
            },
            () => { return _selectedObjects.Length == 1; }
        );
        private RelayCommand? _cmd_AppendTrack;

        public RelayCommand<IList?> Cmd_InsertTrack => _cmd_InsertTrack ??= new(
            args =>
            {
                if (_selectedObjects.Length != 1) return;
                var sp = _selectedObjects[0];

                if (sp.Animations.Length <= 0) return;
                if (args is null) return;
                if (args.Count != 1) return;
                if (args[0] is not AnimationTrackViewModel vm) return;
                var idx = vm.TrackIndex;

                if (idx <= 0) return;
                if (sp.GetTrackIndices().Contains(idx - 1)) return;
                sp.SetAnimation(idx - 1, sp.Animations[0]);
            },
            args =>
            {
                if (_selectedObjects.Length != 1) return false;
                var sp = _selectedObjects[0];

                if (sp.Animations.Length <= 0) return false;
                if (args is null) return false;
                if (args.Count != 1) return false;
                if (args[0] is not AnimationTrackViewModel vm) return false;
                var idx = vm.TrackIndex;

                if (idx <= 0) return false;
                if (sp.GetTrackIndices().Contains(idx - 1)) return false;
                return true;
            }
        );
        private RelayCommand<IList?>? _cmd_InsertTrack;

        public bool? DebugTexture
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugTexture;
                if (_selectedObjects.Skip(1).Any(it => it.DebugTexture != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugTexture = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugBounds
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugBounds;
                if (_selectedObjects.Skip(1).Any(it => it.DebugBounds != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugBounds = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugBones
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugBones;
                if (_selectedObjects.Skip(1).Any(it => it.DebugBones != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugBones = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugRegions
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugRegions;
                if (_selectedObjects.Skip(1).Any(it => it.DebugRegions != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugRegions = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugMeshHulls
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugMeshHulls;
                if (_selectedObjects.Skip(1).Any(it => it.DebugMeshHulls != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugMeshHulls = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugMeshes
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugMeshes;
                if (_selectedObjects.Skip(1).Any(it => it.DebugMeshes != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugMeshes = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugBoundingBoxes
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugBoundingBoxes;
                if (_selectedObjects.Skip(1).Any(it => it.DebugBoundingBoxes != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugBoundingBoxes = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugPaths
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugPaths;
                if (_selectedObjects.Skip(1).Any(it => it.DebugPaths != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugPaths = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugPoints
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugPoints;
                if (_selectedObjects.Skip(1).Any(it => it.DebugPoints != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugPoints = (bool)value;
                OnPropertyChanged();
            }
        }

        public bool? DebugClippings
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].DebugClippings;
                if (_selectedObjects.Skip(1).Any(it => it.DebugClippings != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.DebugClippings = (bool)value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 监听单个模型属性发生变化, 则更新聚合属性值
        /// </summary>
        private void SingleModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SpineObjectModel.IsShown)) OnPropertyChanged(nameof(IsShown));
            else if (e.PropertyName == nameof(SpineObjectModel.UsePma)) OnPropertyChanged(nameof(UsePma));
            else if (e.PropertyName == nameof(SpineObjectModel.Physics)) OnPropertyChanged(nameof(Physics));

            else if (e.PropertyName == nameof(SpineObjectModel.Scale)) OnPropertyChanged(nameof(Scale));
            else if (e.PropertyName == nameof(SpineObjectModel.FlipX)) OnPropertyChanged(nameof(FlipX));
            else if (e.PropertyName == nameof(SpineObjectModel.FlipY)) OnPropertyChanged(nameof(FlipY));
            else if (e.PropertyName == nameof(SpineObjectModel.X)) OnPropertyChanged(nameof(X));
            else if (e.PropertyName == nameof(SpineObjectModel.Y)) OnPropertyChanged(nameof(Y));

            // Skins 变化在 SkinViewModel 中监听
            // Slots 变化在 SlotAttachmentViewModel 中监听
            // AnimationTracks 变化在 AnimationTrackViewModel 中监听

            else if (e.PropertyName == nameof(SpineObjectModel.DebugTexture)) OnPropertyChanged(nameof(DebugTexture));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugBounds)) OnPropertyChanged(nameof(DebugBounds));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugBones)) OnPropertyChanged(nameof(DebugBones));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugRegions)) OnPropertyChanged(nameof(DebugRegions));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugMeshHulls)) OnPropertyChanged(nameof(DebugMeshHulls));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugMeshes)) OnPropertyChanged(nameof(DebugMeshes));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugBoundingBoxes)) OnPropertyChanged(nameof(DebugBoundingBoxes));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugPaths)) OnPropertyChanged(nameof(DebugPaths));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugPoints)) OnPropertyChanged(nameof(DebugPoints));
            else if (e.PropertyName == nameof(SpineObjectModel.DebugClippings)) OnPropertyChanged(nameof(DebugClippings));
        }

        /// <summary>
        /// 监听单个模型动画轨道发生变化, 则重建聚合后的动画列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleModel_AnimationChanged(object? sender, AnimationChangedEventArgs e)
        {
            // XXX: 这里应该有更好的实现, 当 e.AnimationName == null 的时候代表删除轨道需要重新构建列表
            // 但是目前无法识别是否增加了轨道, 因此总是重建列表

            // 由于某些原因, 直接使用 Clear 会和 UI 逻辑冲突产生报错, 因此需要放到 Dispatcher 里延迟执行
            App.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    _animationTracks.Clear();
                    IEnumerable<int> commonTrackIndices = _selectedObjects[0].GetTrackIndices();
                    foreach (var obj in _selectedObjects.Skip(1)) commonTrackIndices = commonTrackIndices.Intersect(obj.GetTrackIndices());
                    foreach (var idx in commonTrackIndices) _animationTracks.Add(new(idx, _selectedObjects));
                }
            );

        }

        public class SkinViewModel : ObservableObject
        {
            private readonly SpineObjectModel[] _spines;
            private readonly string _name;

            public SkinViewModel(string name, SpineObjectModel[] spines)
            {
                _spines = spines;
                _name = name;

                // 使用弱引用, 则此 ViewModel 被释放时无需显式退订事件
                foreach (var sp in _spines)
                {
                    WeakEventManager<SpineObjectModel, SkinStatusChangedEventArgs>.AddHandler(
                        sp,
                        nameof(sp.SkinStatusChanged),
                        SingleModel_SkinStatusChanged
                    );
                }
            }

            public string Name => _name;

            public bool? Status
            {
                get
                {
                    if (_spines.Length <= 0) return null;
                    var val = _spines[0].GetSkinStatus(_name);
                    if (_spines.Skip(1).Any(it => it.GetSkinStatus(_name) != val)) return null;
                    return val;
                }

                set
                {
                    if (_spines.Length <= 0) return;
                    if (value is null) return;
                    bool changed = false;
                    foreach (var sp in _spines) if (sp.SetSkinStatus(_name, (bool)value)) changed = true;
                    if (changed) OnPropertyChanged();
                }
            }

            private void SingleModel_SkinStatusChanged(object? sender, SkinStatusChangedEventArgs e)
            {
                if (e.Name == _name) OnPropertyChanged(nameof(Status));
            }
        }

        public class SlotAttachmentViewModel : ObservableObject
        {
            private readonly SpineObjectModel[] _spines;
            private readonly string[] _attachmentNames = [];
            private readonly string _slotName;

            public SlotAttachmentViewModel(string slotName, SpineObjectModel[] spines)
            {
                _spines = spines;
                _slotName = slotName;

                if (_spines.Length > 0)
                {
                    IEnumerable<string> attachmentNames = _spines[0].SlotAttachments[_slotName];
                    foreach (var sp in _spines.Skip(1))
                        attachmentNames = attachmentNames.Union(sp.SlotAttachments[_slotName]);
                    _attachmentNames = attachmentNames.ToArray();
                }

                // 使用弱引用, 则此 ViewModel 被释放时无需显式退订事件
                foreach (var sp in _spines)
                {
                    WeakEventManager<SpineObjectModel, SlotAttachmentChangedEventArgs>.AddHandler(
                        sp,
                        nameof(sp.SlotAttachmentChanged),
                        SingleModel_SlotAttachmentChanged
                    );
                    WeakEventManager<SpineObjectModel, SkinStatusChangedEventArgs>.AddHandler(
                        sp,
                        nameof(sp.SkinStatusChanged),
                        SingleModel_SkinStatusChanged
                    );
                }
            }

            public RelayCommand Cmd_ClearAttachment => _cmd_ClearAttachment ??= new(() => AttachmentName = null);
            private RelayCommand? _cmd_ClearAttachment;

            public ReadOnlyCollection<string> AttachmentNames => _attachmentNames.AsReadOnly();

            public string SlotName => _slotName;

            public string? AttachmentName
            {
                get
                {
                    if (_spines.Length <= 0) return null;
                    var val = _spines[0].GetAttachment(_slotName);
                    if (_spines.Skip(1).Any(it => it.GetAttachment(_slotName) != val)) return null;
                    return val;
                }

                set
                {
                    if (_spines.Length <= 0) return;
                    bool changed = false;
                    foreach (var sp in _spines) if (sp.SetAttachment(_slotName, value)) changed = true;
                    if (changed) OnPropertyChanged();
                }
            }

            private void SingleModel_SlotAttachmentChanged(object? sender, SlotAttachmentChangedEventArgs e)
            {
                if (e.SlotName == _slotName) OnPropertyChanged(nameof(AttachmentName));
            }

            private void SingleModel_SkinStatusChanged(object? sender, SkinStatusChangedEventArgs e)
            {
                // 如果皮肤发生改变, 则直接触发附件属性变化事件
                OnPropertyChanged(nameof(AttachmentName));
            }
        }

        public class AnimationTrackViewModel : ObservableObject
        {
            private readonly SpineObjectModel[] _spines;
            private readonly string[] _animationNames = [];
            private readonly int _trackIndex;

            public AnimationTrackViewModel(int trackIndex, SpineObjectModel[] spines)
            {
                _spines = spines;
                _trackIndex = trackIndex;

                if (_spines.Length > 0)
                {
                    IEnumerable<string> animationNames = _spines[0].Animations;
                    foreach (var sp in _spines.Skip(1))
                        animationNames = animationNames.Union(sp.Animations);
                    _animationNames = animationNames.ToArray();
                }

                // 使用弱引用, 则此 ViewModel 被释放时无需显式退订事件
                foreach (var sp in _spines)
                {
                    WeakEventManager<SpineObjectModel, AnimationChangedEventArgs>.AddHandler(
                        sp,
                        nameof(sp.AnimationChanged),
                        SingleModel_AnimationChanged
                    );
                }
            }

            public RelayCommand Cmd_ClearTrack => _cmd_ClearTrack ??= new(() => { foreach (var sp in _spines) sp.ClearTrack(_trackIndex); });
            private RelayCommand? _cmd_ClearTrack;

            public ReadOnlyCollection<string> AnimationNames => _animationNames.AsReadOnly();

            public int TrackIndex => _trackIndex;

            public string? AnimationName
            {
                get
                {
                    /// XXX: 空轨道和多选不相同都会返回 null
                    if (_spines.Length <= 0) return null;
                    var val = _spines[0].GetAnimation(_trackIndex);
                    if (_spines.Skip(1).Any(it => it.GetAnimation(_trackIndex) != val)) return null;
                    return val;
                }

                set
                {
                    if (_spines.Length <= 0) return;
                    if (value is null) return;
                    foreach (var sp in _spines) sp.SetAnimation(_trackIndex, value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AnimationDuration));
                }
            }

            public float? AnimationDuration
            {
                get
                {
                    if (_spines.Length <= 0) return null;
                    var ani = _spines[0].GetAnimation(_trackIndex);
                    if (ani is null) return null;
                    var val = _spines[0].GetAnimationDuration(ani);
                    foreach (var sp in _spines.Skip(1))
                    {
                        var a = sp.GetAnimation(_trackIndex);
                        if (a is null) return null;
                        if (sp.GetAnimationDuration(a) != val) return null;
                    }
                    return val;
                }
            }

            private void SingleModel_AnimationChanged(object? sender, AnimationChangedEventArgs e)
            {
                if (e.TrackIndex == _trackIndex) OnPropertyChanged(nameof(AnimationName));
            }
        }
    }
}