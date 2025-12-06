using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spine;
using Spine.Interfaces;
using SpineViewer.Models;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace SpineViewer.ViewModels.MainWindow
{
    public class SpineObjectTabViewModel : ObservableObject
    {
        private SpineObjectModel[] _selectedObjects = [];
        private readonly ObservableCollection<SkinViewModel> _skins = [];
        private readonly ObservableCollection<SlotViewModel> _slots = [];
        private readonly ObservableCollection<AnimationTrackViewModel> _animationTracks = [];

        public static ImmutableArray<ISkeleton.Physics> PhysicsOptions { get; } = Enum.GetValues<ISkeleton.Physics>().ToImmutableArray();

        public SpineObjectModel[] SelectedObjects
        {
            get => _selectedObjects;
            set
            {
                if (ReferenceEquals(_selectedObjects, value)) return;

                // 清空之前的所有内容
                _skins.Clear();
                _slots.Clear();
                _animationTracks.Clear();

                // 生成新的内容
                _selectedObjects = value ?? [];
                if (_selectedObjects.Length > 0)
                {
                    IEnumerable<string> commonSkinNames = _selectedObjects[0].Skins;
                    foreach (var obj in _selectedObjects.Skip(1)) commonSkinNames = commonSkinNames.Intersect(obj.Skins);
                    foreach (var name in commonSkinNames.Order()) _skins.Add(new(name, _selectedObjects));

                    IEnumerable<string> commonSlotNames = _selectedObjects[0].SlotAttachments.Keys;
                    foreach (var obj in _selectedObjects.Skip(1)) commonSlotNames = commonSlotNames.Intersect(obj.SlotAttachments.Keys);
                    foreach (var name in commonSlotNames.Order()) _slots.Add(new(name, _selectedObjects));

                    IEnumerable<int> commonTrackIndices = _selectedObjects[0].GetTrackIndices();
                    foreach (var obj in _selectedObjects.Skip(1)) commonTrackIndices = commonTrackIndices.Intersect(obj.GetTrackIndices());
                    foreach (var idx in commonTrackIndices.Order()) _animationTracks.Add(new(idx, _selectedObjects));
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
                OnPropertyChanged(nameof(TimeScale));

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

        public float? TimeScale
        {
            get
            {
                if (_selectedObjects.Length <= 0) return null;
                var val = _selectedObjects[0].TimeScale;
                if (_selectedObjects.Skip(1).Any(it => it.TimeScale != val)) return null;
                return val;
            }

            set
            {
                if (_selectedObjects.Length <= 0) return;
                if (value is null) return;
                foreach (var sp in _selectedObjects) sp.TimeScale = (float)value;
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

        public RelayCommand<IList?> Cmd_EnableSkins => _cmd_EnableSkins ??= new (
            args => { if (args is null) return; foreach (var s in args.OfType<SkinViewModel>()) s.Status = true; },
            args => { return args is not null && args.OfType<SkinViewModel>().Any(); }
        );
        private RelayCommand<IList?> _cmd_EnableSkins;

        public RelayCommand<IList?> Cmd_DisableSkins => _cmd_DisableSkins ??= new (
            args => { if (args is null) return; foreach (var s in args.OfType<SkinViewModel>()) s.Status = false; },
            args => { return args is not null && args.OfType<SkinViewModel>().Any(); }
        );
        private RelayCommand<IList?> _cmd_DisableSkins;

        public RelayCommand Cmd_EnableAllSkins => _cmd_EnableAllSkins ??= new(
            () => { if (_skins.Count <= 0) return; foreach (var s in _skins) s.Status = true; },
            () => { return _skins.Count > 0; }
        );
        private RelayCommand _cmd_EnableAllSkins;

        public RelayCommand Cmd_DisableAllSkins => _cmd_DisableAllSkins ??= new(
            () => { if (_skins.Count <= 0) return; foreach (var s in _skins) s.Status = false; },
            () => { return _skins.Count > 0; }
        );
        private RelayCommand _cmd_DisableAllSkins;

        public ObservableCollection<SlotViewModel> Slots => _slots;

        public RelayCommand<IList?> Cmd_EnableSlots => _cmd_EnableSlots ??= new (
            args => { if (args is null) return; foreach (var s in args.OfType<SlotViewModel>()) s.Visible = true; },
            args => { return args is not null && args.OfType<SlotViewModel>().Any(); }
        );
        private RelayCommand<IList?> _cmd_EnableSlots;

        public RelayCommand<IList?> Cmd_DisableSlots => _cmd_DisableSlots ??= new (
            args => { if (args is null) return; foreach (var s in args.OfType<SlotViewModel>()) s.Visible = false; },
            args => { return args is not null && args.OfType<SlotViewModel>().Any(); }
        );
        private RelayCommand<IList?> _cmd_DisableSlots;

        public RelayCommand Cmd_EnableAllSlots => _cmd_EnableAllSlots ??= new(
            () => { if (_slots.Count <= 0) return; foreach (var s in _slots) s.Visible = true; },
            () => { return _slots.Count > 0; }
        );
        private RelayCommand _cmd_EnableAllSlots;

        public RelayCommand Cmd_DisableAllSlots => _cmd_DisableAllSlots ??= new(
            () => { if (_slots.Count <= 0) return; foreach (var s in _slots) s.Visible = false; },
            () => { return _slots.Count > 0; }
        );
        private RelayCommand _cmd_DisableAllSlots;

        public ObservableCollection<AnimationTrackViewModel> AnimationTracks => _animationTracks;

        public RelayCommand Cmd_AppendTrack => _cmd_AppendTrack ??= new(
            () =>
            {
                if (_selectedObjects.Length != 1) return;
                var sp = _selectedObjects[0];
                if (sp.Animations.Length <= 0) return;
                sp.SetAnimation(sp.GetTrackIndices().LastOrDefault(-1) + 1, sp.Animations[0]);

                RebuildAnimationTracks();
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
                var trIdx = vm.TrackIndex;

                if (trIdx <= 0) return;
                if (sp.GetTrackIndices().Contains(trIdx - 1)) return;
                sp.SetAnimation(trIdx - 1, sp.Animations[0]);

                RebuildAnimationTracks();
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

        public RelayCommand<IList?>? Cmd_ClearTrack => _cmd_ClearTrack ??= new(
            args =>
            {
                if (_selectedObjects.Length <= 0) return;
                if (args is null) return;
                if (args.Count <= 0) return;

                foreach (var vm in args.OfType<AnimationTrackViewModel>())
                    foreach (var sp in _selectedObjects)
                        sp.ClearTrack(vm.TrackIndex);

                RebuildAnimationTracks();
            },
            args =>
            {
                if (_selectedObjects.Length <= 0) return false;
                if (args is null) return false;
                if (args.Count <= 0) return false;
                return true;
            }
        );
        private RelayCommand<IList?>? _cmd_ClearTrack;

        private void RebuildAnimationTracks()
        {
            _animationTracks.Clear();
            IEnumerable<int> commonTrackIndices = _selectedObjects[0].GetTrackIndices();
            foreach (var obj in _selectedObjects.Skip(1)) commonTrackIndices = commonTrackIndices.Intersect(obj.GetTrackIndices());
            foreach (var idx in commonTrackIndices) _animationTracks.Add(new(idx, _selectedObjects));
        }

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

        public class SkinViewModel : ObservableObject
        {
            private readonly SpineObjectModel[] _spines;
            private readonly string _name;

            public SkinViewModel(string name, SpineObjectModel[] spines)
            {
                _spines = spines;
                _name = name;
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
        }

        public class SlotViewModel : ObservableObject
        {
            private readonly SpineObjectModel[] _spines;
            private readonly string[] _attachmentNames = [];
            private readonly string _slotName;

            public SlotViewModel(string slotName, SpineObjectModel[] spines)
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
            }

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

            public bool? Visible
            {
                get
                {
                    if (_spines.Length <= 0) return null;
                    var val = _spines[0].GetSlotVisible(_slotName);
                    if (_spines.Skip(1).Any(it => it.GetSlotVisible(_slotName) != val)) return null;
                    return val;
                }

                set
                {
                    if (_spines.Length <= 0) return;
                    if (value is null) return;
                    bool changed = false;
                    foreach (var sp in _spines) if (sp.SetSlotVisible(_slotName, (bool)value)) changed = true;
                    if (changed) OnPropertyChanged();
                }
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
            }

            public ReadOnlyCollection<string> AnimationNames => _animationNames.AsReadOnly();

            public int TrackIndex => _trackIndex;

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

            public string? AnimationName
            {
                get
                {
                    // XXX: 空轨道和多选不相同都会返回 null
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

            public float? TrackTimeScale
            {
                get
                {
                    // XXX: 空轨道和多选不相同都会返回 null
                    if (_spines.Length <= 0) return null;
                    var val = _spines[0].GetTrackTimeScale(_trackIndex);
                    if (_spines.Skip(1).Any(it => it.GetTrackTimeScale(_trackIndex) != val)) return null;
                    return val;
                }

                set
                {
                    if (_spines.Length <= 0) return;
                    if (value is null) return;
                    foreach (var sp in _spines) sp.SetTrackTimeScale(_trackIndex, (float)value);
                    OnPropertyChanged();
                }
            }

            public float? TrackAlpha
            {
                get
                {
                    // XXX: 空轨道和多选不相同都会返回 null
                    if (_spines.Length <= 0) return null;
                    var val = _spines[0].GetTrackAlpha(_trackIndex);
                    if (_spines.Skip(1).Any(it => it.GetTrackAlpha(_trackIndex) != val)) return null;
                    return val;
                }

                set
                {
                    if (_spines.Length <= 0) return;
                    if (value is null) return;
                    foreach (var sp in _spines) sp.SetTrackAlpha(_trackIndex, (float)value);
                    OnPropertyChanged();
                }
            }
        }
    }
}