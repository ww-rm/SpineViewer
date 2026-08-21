using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NLog;
using Spine;
using Spine.Implementations;
using Spine.Interfaces;
using SpineViewer.Models;
using SpineViewer.Services;
using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
        public static readonly string PreferenceFilePath = Path.Combine(App.ProcessDataDirectory, "preference.json");

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
            // 此处要加密 token
            if (!string.IsNullOrWhiteSpace(m.GitHubToken))
                m.GitHubToken = Secrets.User.Encrypt(m.GitHubToken);
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
            if (JsonHelper.Deserialize<PreferenceModel>(PreferenceFilePath, out var m, true))
            {
                try
                {
                    // 此处要解密 token
                    if (!string.IsNullOrWhiteSpace(m.GitHubToken))
                    {
                        try
                        {
                            m.GitHubToken = Secrets.User.Decrypt(m.GitHubToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.Debug(ex.ToString());
                            _logger.Warn("Failed to decrypt github token, {0}", ex.Message);
                            m.GitHubToken = null;
                        }
                    }
                    Preference = m;
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to load some prefereneces, {0}", ex.Message);
                }
            }
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
                    DisableAtlasLoading = DisableAtlasLoading,
                    ForcePremul = ForcePremul,
                    ForceNearest = ForceNearest,
                    ForceMipmap = ForceMipmap,

                    AvoidOverlapWhenAdding = AvoidOverlapWhenAdding,
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

                    RenderSelectedOnly = RenderSelectedOnly,
                    HighlightSelectedModel = HighlightSelectedModel,
                    HitTestLevel = HitTestLevel,
                    LogHitSlots = LogHitSlots,
                    MaxFps = MaxFps,

                    AppProxyUri = AppProxyUri,
                    GitHubToken = GitHubToken,

                    AppLanguage = AppLanguage,
                    AppSkin = AppSkin,
                    MaxParallelism = MaxParallelism,
                    WallpaperView = WallpaperView,
                    WallpaperMaxFps = WallpaperMaxFps,
                    CloseToTray = CloseToTray,
                    AutoRun = AutoRun,
                    AutoRunWorkspaceConfigPath = AutoRunWorkspaceConfigPath,
                    AssociateFileSuffix = AssociateFileSuffix,
                };
            }
            set
            {
                DisableAtlasLoading = value.DisableAtlasLoading;
                ForcePremul = value.ForcePremul;
                ForceNearest = value.ForceNearest;
                ForceMipmap = value.ForceMipmap;

                AvoidOverlapWhenAdding = value.AvoidOverlapWhenAdding;
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

                RenderSelectedOnly = value.RenderSelectedOnly;
                HighlightSelectedModel = value.HighlightSelectedModel;
                HitTestLevel = value.HitTestLevel;
                LogHitSlots = value.LogHitSlots;
                MaxFps = value.MaxFps;

                AppProxyUri = value.AppProxyUri;
                GitHubToken = value.GitHubToken;

                AppLanguage = value.AppLanguage;
                AppSkin = value.AppSkin;
                MaxParallelism = value.MaxParallelism;
                WallpaperView = value.WallpaperView;
                WallpaperMaxFps = value.WallpaperMaxFps;
                CloseToTray = value.CloseToTray;
                AutoRun = value.AutoRun;
                AutoRunWorkspaceConfigPath = value.AutoRunWorkspaceConfigPath;
                AssociateFileSuffix = value.AssociateFileSuffix;
            }
        }

        #region 纹理加载首选项

        public bool DisableAtlasLoading
        {
            get => SpineObjectData.DisableAtlasLoading;
            set => SetProperty(SpineObjectData.DisableAtlasLoading, value, v => SpineObjectData.DisableAtlasLoading = v);
        }

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

        public bool AvoidOverlapWhenAdding
        {
            get => _vmMain.SpineObjectListViewModel.AvoidOverlapWhenAdding;
            set => SetProperty(_vmMain.SpineObjectListViewModel.AvoidOverlapWhenAdding, value, v => _vmMain.SpineObjectListViewModel.AvoidOverlapWhenAdding = v);
        }

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

        public static ImmutableArray<HitTestLevel> HitTestLevelOptions { get; } = Enum.GetValues<HitTestLevel>().ToImmutableArray();

        public bool RenderSelectedOnly
        {
            get => _vmMain.SFMLRendererViewModel.RenderSelectedOnly;
            set => SetProperty(_vmMain.SFMLRendererViewModel.RenderSelectedOnly, value, v => _vmMain.SFMLRendererViewModel.RenderSelectedOnly = v);
        }

        public bool HighlightSelectedModel
        {
            get => _vmMain.SFMLRendererViewModel.HighlightSelectedModel;
            set => SetProperty(_vmMain.SFMLRendererViewModel.HighlightSelectedModel, value, v => _vmMain.SFMLRendererViewModel.HighlightSelectedModel = v);
        }

        public HitTestLevel HitTestLevel
        {
            get => SpineExtension.HitTestLevel;
            set => SetProperty(SpineExtension.HitTestLevel, value, v => SpineExtension.HitTestLevel = v);
        }

        public bool LogHitSlots
        {
            get => SpineExtension.LogHitSlots;
            set => SetProperty(SpineExtension.LogHitSlots, value, v => SpineExtension.LogHitSlots = v);
        }

        public uint MaxFps
        {
            get => _vmMain.SFMLRendererViewModel.MaxFps;
            set => SetProperty(_vmMain.SFMLRendererViewModel.MaxFps, value, v => _vmMain.SFMLRendererViewModel.MaxFps = v);
        }

        #endregion

        #region 网络连接选项

        public Uri? AppProxyUri 
        { 
            get => App.ProxyUri;
            set => SetProperty(App.ProxyUri, value, v => App.ProxyUri = v);
        }

        public string? GitHubToken
        {
            get => GitHubService.Token;
            set => SetProperty(GitHubService.Token, value?.Trim(), v => GitHubService.Token = v);
        }

        #endregion

        #region 应用程序选项

        public static ImmutableArray<AppLanguage> AppLanguageOptions { get; } = Enum.GetValues<AppLanguage>().ToImmutableArray();

        public static ImmutableArray<AppSkin> AppSkinOptions { get; } = Enum.GetValues<AppSkin>().ToImmutableArray();

        public AppLanguage AppLanguage
        {
            get => App.Language;
            set => SetProperty(App.Language, value, v => App.Language = v);
        }

        public AppSkin AppSkin
        {
            get => App.Skin;
            set => SetProperty(App.Skin, value, v => App.Skin = v);
        }

        public int MaxParallelism
        {
            get => _vmMain.SFMLRendererViewModel.MaxParallelism;
            set => SetProperty(_vmMain.SFMLRendererViewModel.MaxParallelism, value, v => _vmMain.SFMLRendererViewModel.MaxParallelism = v);
        }

        public bool WallpaperView
        {
            get => _vmMain.SFMLRendererViewModel.WallpaperView;
            set => SetProperty(_vmMain.SFMLRendererViewModel.WallpaperView, value, v => _vmMain.SFMLRendererViewModel.WallpaperView = v);
        }

        public uint WallpaperMaxFps
        {
            get => _vmMain.SFMLRendererViewModel.WallpaperMaxFps;
            set => SetProperty(_vmMain.SFMLRendererViewModel.WallpaperMaxFps, value, v => _vmMain.SFMLRendererViewModel.WallpaperMaxFps = v);
        }

        public bool CloseToTray
        {
            get => _vmMain.CloseToTray;
            set => SetProperty(_vmMain.CloseToTray, value, v => _vmMain.CloseToTray = v);
        }

        public bool AutoRun
        {
            get => App.AutoRun;
            set => SetProperty(App.AutoRun, value, v => App.AutoRun = v);
        }

        public string AutoRunWorkspaceConfigPath
        {
            get => _vmMain.AutoRunWorkspaceConfigPath;
            set => SetProperty(_vmMain.AutoRunWorkspaceConfigPath, value, v => _vmMain.AutoRunWorkspaceConfigPath = v);
        }

        public bool AssociateFileSuffix
        {
            get => App.AssociateFileSuffix;
            set => SetProperty(App.AssociateFileSuffix, value, v => App.AssociateFileSuffix = v);
        }

        #endregion
    }
}
