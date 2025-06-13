using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Spine.SpineWrappers;
using SpineViewer.Models;
using SpineViewer.Services;
using System;
using System.Collections.Generic;
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
                    DebugClippings = DebugClippings
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
        
        // TODO: 是否自动记忆模型参数

        #region 模型加载首选项

        public bool UsePma { get => _usePma; set => SetProperty(ref _usePma, value); }
        private bool _usePma;

        public bool DebugTexture { get => _debugTexture; set => SetProperty(ref _debugTexture, value); }
        private bool _debugTexture = true;

        public bool DebugBounds { get => _debugBounds; set => SetProperty(ref _debugBounds, value); }
        private bool _debugBounds;

        public bool DebugBones { get => _debugBones; set => SetProperty(ref _debugBones, value); }
        private bool _debugBones;

        public bool DebugRegions { get => _debugRegions; set => SetProperty(ref _debugRegions, value); }
        private bool _debugRegions;

        public bool DebugMeshHulls { get => _debugMeshHulls; set => SetProperty(ref _debugMeshHulls, value); }
        private bool _debugMeshHulls;

        public bool DebugMeshes { get => _debugMeshes; set => SetProperty(ref _debugMeshes, value); }
        private bool _debugMeshes;

        public bool DebugBoundingBoxes { get => _debugBoundingBoxes; set => SetProperty(ref _debugBoundingBoxes, value); }
        private bool _debugBoundingBoxes;

        public bool DebugPaths { get => _debugPaths; set => SetProperty(ref _debugPaths, value); }
        private bool _debugPaths;

        public bool DebugPoints { get => _debugPoints; set => SetProperty(ref _debugPoints, value); }
        private bool _debugPoints;

        public bool DebugClippings { get => _debugClippings; set => SetProperty(ref _debugClippings, value); }
        private bool _debugClippings;

        #endregion
    }
}
