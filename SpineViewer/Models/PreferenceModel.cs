using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spine;
using SpineViewer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpineViewer.Models
{
    /// <summary>
    /// 首选项参数模型, 用于对话框修改以及本地保存
    /// </summary>
    public partial class PreferenceModel : ObservableObject
    {

        #region 纹理加载首选项

        [ObservableProperty]
        private bool _forcePremul;

        [ObservableProperty]
        private bool _forceNearest;

        [ObservableProperty]
        private bool _forceMipmap;

        #endregion

        #region 模型加载首选项

        [ObservableProperty]
        private bool _isShown = true;

        [ObservableProperty]
        private bool _usePma;

        [ObservableProperty]
        private bool _debugTexture = true;

        [ObservableProperty]
        private bool _debugBounds;

        [ObservableProperty]
        private bool _debugBones;

        [ObservableProperty]
        private bool _debugRegions;

        [ObservableProperty]
        private bool _debugMeshHulls;

        [ObservableProperty]
        private bool _debugMeshes;

        [ObservableProperty]
        private bool _debugBoundingBoxes;

        [ObservableProperty]
        private bool _debugPaths;

        [ObservableProperty]
        private bool _debugPoints;

        [ObservableProperty]
        private bool _debugClippings;

        #endregion

        #region 预览画面首选项

        [ObservableProperty]
        private bool _renderSelectedOnly;

        [ObservableProperty]
        private HitTestLevel _hitTestLevel;

        [ObservableProperty]
        private bool _logHitSlots;

        [ObservableProperty]
        private uint _maxFps = 30;

        #endregion

        #region 程序选项

        public RelayCommand Cmd_SelectAutoRunWorkspaceConfigPath => _cmd_SelectAutoRunWorkspaceConfigPath ??= new(() =>
        {
            if (!DialogService.ShowOpenJsonDialog(out var fileName))
                return;
            AutoRunWorkspaceConfigPath = fileName;
        });
        private RelayCommand? _cmd_SelectAutoRunWorkspaceConfigPath;

        [ObservableProperty]
        private AppLanguage _appLanguage;

        [ObservableProperty]
        private AppSkin _appSkin;

        [ObservableProperty]
        private bool _wallpaperView;

        [ObservableProperty]
        private uint _wallpaperMaxFps = 30;

        [ObservableProperty]
        private bool _closeToTray;

        [ObservableProperty]
        private bool _autoRun;

        [ObservableProperty]
        private string _autoRunWorkspaceConfigPath;

        [ObservableProperty]
        private bool _associateFileSuffix;

        #endregion
    }
}
