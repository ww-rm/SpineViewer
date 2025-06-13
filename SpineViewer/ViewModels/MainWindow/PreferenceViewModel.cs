using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine.SpineWrappers;
using SpineViewer.Models;
using SpineViewer.Services;
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
            try
            {
                m.Serialize(PreferenceFilePath);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save preference to {0}, {1}", PreferenceFilePath, ex.Message);
                _logger.Trace(ex.ToString());
            }
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
            if (!File.Exists(PreferenceFilePath)) return;

            try
            {
                var m = PreferenceModel.Deserialize(PreferenceFilePath);
                Preference = m;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load preference from {0}, {1}", PreferenceFilePath, ex.Message);
                _logger.Trace(ex.ToString());
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
                    ForcePremul = ForcePremul,
                    ForceNearest = ForceNearest,
                    ForceMipmap = ForceMipmap,
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
                    AppLanguage = AppLanguage,
                };
            }
            set
            {
                ForcePremul = value.ForcePremul;
                ForceNearest = value.ForceNearest;
                ForceMipmap = value.ForceMipmap;
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

        // TODO: 是否自动记忆模型参数

        public bool UsePma 
        { 
            get => SpineObjectListViewModel.LoadOptions.UsePma; 
            set => SetProperty(SpineObjectListViewModel.LoadOptions.UsePma, value, v => SpineObjectListViewModel.LoadOptions.UsePma = v); 
        }

        public bool DebugTexture
        {
            get => SpineObjectListViewModel.LoadOptions.DebugTexture;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugTexture, value, v => SpineObjectListViewModel.LoadOptions.DebugTexture = v);
        }

        public bool DebugBounds
        {
            get => SpineObjectListViewModel.LoadOptions.DebugBounds;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugBounds, value, v => SpineObjectListViewModel.LoadOptions.DebugBounds = v);
        }

        public bool DebugBones
        {
            get => SpineObjectListViewModel.LoadOptions.DebugBones;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugBones, value, v => SpineObjectListViewModel.LoadOptions.DebugBones = v);
        }

        public bool DebugRegions
        {
            get => SpineObjectListViewModel.LoadOptions.DebugRegions;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugRegions, value, v => SpineObjectListViewModel.LoadOptions.DebugRegions = v);
        }

        public bool DebugMeshHulls
        {
            get => SpineObjectListViewModel.LoadOptions.DebugMeshHulls;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugMeshHulls, value, v => SpineObjectListViewModel.LoadOptions.DebugMeshHulls = v);
        }

        public bool DebugMeshes
        {
            get => SpineObjectListViewModel.LoadOptions.DebugMeshes;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugMeshes, value, v => SpineObjectListViewModel.LoadOptions.DebugMeshes = v);
        }

        public bool DebugBoundingBoxes
        {
            get => SpineObjectListViewModel.LoadOptions.DebugBoundingBoxes;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugBoundingBoxes, value, v => SpineObjectListViewModel.LoadOptions.DebugBoundingBoxes = v);
        }

        public bool DebugPaths
        {
            get => SpineObjectListViewModel.LoadOptions.DebugPaths;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugPaths, value, v => SpineObjectListViewModel.LoadOptions.DebugPaths = v);
        }

        public bool DebugPoints
        {
            get => SpineObjectListViewModel.LoadOptions.DebugPoints;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugPoints, value, v => SpineObjectListViewModel.LoadOptions.DebugPoints = v);
        }

        public bool DebugClippings
        {
            get => SpineObjectListViewModel.LoadOptions.DebugClippings;
            set => SetProperty(SpineObjectListViewModel.LoadOptions.DebugClippings, value, v => SpineObjectListViewModel.LoadOptions.DebugClippings = v);
        }

        #endregion

        #region 程序选项

        public static ImmutableArray<AppLanguage> AppLanguageOptions { get; } = Enum.GetValues<AppLanguage>().ToImmutableArray();

        public AppLanguage AppLanguage
        {
            get => ((App)App.Current).Language;
            set => SetProperty(((App)App.Current).Language, value, v => ((App)App.Current).Language = v);
        }

        #endregion
    }
}
