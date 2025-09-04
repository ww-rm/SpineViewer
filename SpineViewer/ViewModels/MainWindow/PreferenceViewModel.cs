using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine.SpineWrappers;
using SpineViewer.Models;
using SpineViewer.Services;
using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpineViewer.ViewModels.MainWindow
{
    public class PreferenceViewModel : ObservableObject
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        public static readonly string PreferenceFilePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "preference.json");

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly MainWindowViewModel _vmMain;

        public PreferenceViewModel(MainWindowViewModel vmMain)
        {
            _vmMain = vmMain;
        }

        /// <summary>
        /// 显示首选项对话框
        /// </summary>
        public RelayCommand Cmd_ShowPreferenceDialog => _cmd_ShowPreferenceDialog ??= new(ShowPreferenceDialog_Execute);
        private RelayCommand? _cmd_ShowPreferenceDialog;

        private void ShowPreferenceDialog_Execute()
        {
            var m = Preference;
            if (!DialogService.ShowPreferenceDialog(m))
                return;

            Preference = m;
            SavePreference(m);
        }

        private static void SavePreference(PreferenceModel m)
        {
            JsonHelper.Serialize(m, PreferenceFilePath);
        }

        /// <summary>
        /// 保存首选项, 保存失败会有日志提示
        /// </summary>
        public void SavePreference() => SavePreference(Preference);

        /// <summary>
        /// 加载首选项, 加载失败会有日志提示
        /// </summary>
        public void LoadPreference()
        {
            if (JsonHelper.Deserialize<PreferenceModel>(PreferenceFilePath, out var obj, true)) 
                Preference = obj;
        }

        /// <summary>
        /// 获取参数副本或者进行设置
        /// </summary>
        private PreferenceModel Preference
        {
            get
            {
                return new()
                {
                    ForcePremul = ForcePremul,
                    ForceNearest = ForceNearest,
                    ForceMipmap = ForceMipmap,

                    IsShown = IsShown,
                    UsePma = UsePma,
                    DebugTexture = DebugTexture,
                    DebugBounds = DebugBounds,
                    DebugBones = DebugBones,
                    DebugRegions = DebugRegions,
                    DebugMeshHulls = DebugMeshHulls,
                    DebugMeshes = DebugMeshes,
                    DebugBoundingBoxes = DebugBoundingBoxes,
                    DebugPaths = DebugPaths,
                    DebugPoints = DebugPoints,
                    DebugClippings = DebugClippings,

                    MaxFps = MaxFps,
                    Speed = Speed,
                    ShowAxis = ShowAxis,
                    BackgroundColor = BackgroundColor,

                    RenderSelectedOnly = RenderSelectedOnly,
                    AppLanguage = AppLanguage,
                };
            }
            set
            {
                ForcePremul = value.ForcePremul;
                ForceNearest = value.ForceNearest;
                ForceMipmap = value.ForceMipmap;

                IsShown = value.IsShown;
                UsePma = value.UsePma;

                DebugTexture = value.DebugTexture;
                DebugBounds = value.DebugBounds;
                DebugBones = value.DebugBones;
                DebugRegions = value.DebugRegions;
                DebugMeshHulls = value.DebugMeshHulls;
                DebugMeshes = value.DebugMeshes;
                DebugBoundingBoxes = value.DebugBoundingBoxes;
                DebugPaths = value.DebugPaths;
                DebugPoints = value.DebugPoints;
                DebugClippings = value.DebugClippings;

                MaxFps = value.MaxFps;
                Speed = value.Speed;
                ShowAxis = value.ShowAxis;
                BackgroundColor = value.BackgroundColor;

                RenderSelectedOnly = value.RenderSelectedOnly;
                AppLanguage = value.AppLanguage;
            }
        }

        #region 纹理加载首选项

        public bool ForcePremul
        {
            get => TextureLoader.DefaultLoader.ForcePremul;
            set => SetProperty(TextureLoader.DefaultLoader.ForcePremul, value, v => TextureLoader.DefaultLoader.ForcePremul = v);
        }

        public bool ForceNearest 
        {
            get => TextureLoader.DefaultLoader.ForceNearest;
            set => SetProperty(TextureLoader.DefaultLoader.ForceNearest, value, v => TextureLoader.DefaultLoader.ForceNearest = v);
        }

        public bool ForceMipmap 
        {
            get => TextureLoader.DefaultLoader.ForceMipmap;
            set => SetProperty(TextureLoader.DefaultLoader.ForceMipmap, value, v => TextureLoader.DefaultLoader.ForceMipmap = v);
        }

        #endregion
        
        #region 模型加载首选项

        public bool IsShown
        {
            get => SpineObjectModel.LoadOptions.IsShown;
            set => SetProperty(SpineObjectModel.LoadOptions.IsShown, value, v => SpineObjectModel.LoadOptions.IsShown = v);
        }

        public bool UsePma 
        { 
            get => SpineObjectModel.LoadOptions.UsePma; 
            set => SetProperty(SpineObjectModel.LoadOptions.UsePma, value, v => SpineObjectModel.LoadOptions.UsePma = v); 
        }

        public bool DebugTexture
        {
            get => SpineObjectModel.LoadOptions.DebugTexture;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugTexture, value, v => SpineObjectModel.LoadOptions.DebugTexture = v);
        }

        public bool DebugBounds
        {
            get => SpineObjectModel.LoadOptions.DebugBounds;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugBounds, value, v => SpineObjectModel.LoadOptions.DebugBounds = v);
        }

        public bool DebugBones
        {
            get => SpineObjectModel.LoadOptions.DebugBones;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugBones, value, v => SpineObjectModel.LoadOptions.DebugBones = v);
        }

        public bool DebugRegions
        {
            get => SpineObjectModel.LoadOptions.DebugRegions;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugRegions, value, v => SpineObjectModel.LoadOptions.DebugRegions = v);
        }

        public bool DebugMeshHulls
        {
            get => SpineObjectModel.LoadOptions.DebugMeshHulls;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugMeshHulls, value, v => SpineObjectModel.LoadOptions.DebugMeshHulls = v);
        }

        public bool DebugMeshes
        {
            get => SpineObjectModel.LoadOptions.DebugMeshes;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugMeshes, value, v => SpineObjectModel.LoadOptions.DebugMeshes = v);
        }

        public bool DebugBoundingBoxes
        {
            get => SpineObjectModel.LoadOptions.DebugBoundingBoxes;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugBoundingBoxes, value, v => SpineObjectModel.LoadOptions.DebugBoundingBoxes = v);
        }

        public bool DebugPaths
        {
            get => SpineObjectModel.LoadOptions.DebugPaths;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugPaths, value, v => SpineObjectModel.LoadOptions.DebugPaths = v);
        }

        public bool DebugPoints
        {
            get => SpineObjectModel.LoadOptions.DebugPoints;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugPoints, value, v => SpineObjectModel.LoadOptions.DebugPoints = v);
        }

        public bool DebugClippings
        {
            get => SpineObjectModel.LoadOptions.DebugClippings;
            set => SetProperty(SpineObjectModel.LoadOptions.DebugClippings, value, v => SpineObjectModel.LoadOptions.DebugClippings = v);
        }

        #endregion

        #region 预览画面首选项

        public uint MaxFps
        {
            get => _vmMain.SFMLRendererViewModel.MaxFps;
            set => SetProperty(_vmMain.SFMLRendererViewModel.MaxFps, value, v => _vmMain.SFMLRendererViewModel.MaxFps = value);
        }

        public float Speed
        {
            get => _vmMain.SFMLRendererViewModel.Speed;
            set => SetProperty(_vmMain.SFMLRendererViewModel.Speed, value, v => _vmMain.SFMLRendererViewModel.Speed = value);
        }

        public bool ShowAxis
        {
            get => _vmMain.SFMLRendererViewModel.ShowAxis;
            set => SetProperty(_vmMain.SFMLRendererViewModel.ShowAxis, value, v => _vmMain.SFMLRendererViewModel.ShowAxis = value);
        }

        public Color BackgroundColor
        {
            get => _vmMain.SFMLRendererViewModel.BackgroundColor;
            set => SetProperty(_vmMain.SFMLRendererViewModel.BackgroundColor, value, v => _vmMain.SFMLRendererViewModel.BackgroundColor = value);
        }

        #endregion

        #region 程序选项

        public static ImmutableArray<AppLanguage> AppLanguageOptions { get; } = Enum.GetValues<AppLanguage>().ToImmutableArray();

        public bool RenderSelectedOnly
        {
            get => _vmMain.SFMLRendererViewModel.RenderSelectedOnly;
            set => SetProperty(_vmMain.SFMLRendererViewModel.RenderSelectedOnly, value, v => _vmMain.SFMLRendererViewModel.RenderSelectedOnly = v);
        }

        public AppLanguage AppLanguage
        {
            get => ((App)App.Current).Language;
            set => SetProperty(((App)App.Current).Language, value, v => ((App)App.Current).Language = v);
        }

        #endregion
    }
}
