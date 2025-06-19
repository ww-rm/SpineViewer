using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using SFMLRenderer;
using SpineViewer.Models;
using SpineViewer.Services;
using SpineViewer.Utils;
using System.Windows.Shell;

namespace SpineViewer.ViewModels.MainWindow
{
    /// <summary>
    /// MainWindow 上下文对象
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MainWindowViewModel(ISFMLRenderer sfmlRenderer)
        {
            _sfmlRenderer = sfmlRenderer;
            _explorerListViewModel = new(this);
            _spineObjectListViewModel = new(this);
            _sfmlRendererViewModel = new(this);
            _preferenceViewModel = new(this);
        }

        public string Title => $"SpineViewer - v{App.Version}";

        /// <summary>
        /// SFML 渲染对象
        /// </summary>
        public ISFMLRenderer SFMLRenderer => _sfmlRenderer;
        private readonly ISFMLRenderer _sfmlRenderer;

        public TaskbarItemProgressState ProgressState { get => _progressState; set => SetProperty(ref _progressState, value); }
        private TaskbarItemProgressState _progressState = TaskbarItemProgressState.None;

        public float ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }
        private float _progressValue = 0;

        /// <summary>
        /// 已加载的 Spine 对象
        /// </summary>
        public ObservableCollectionWithLock<SpineObjectModel> SpineObjects => _spineObjectModels;
        private readonly ObservableCollectionWithLock<SpineObjectModel> _spineObjectModels = [];

        public PreferenceViewModel PreferenceViewModel => _preferenceViewModel;
        private readonly PreferenceViewModel _preferenceViewModel;

        /// <summary>
        /// 浏览页列表 ViewModel
        /// </summary>
        public ExplorerListViewModel ExplorerListViewModel => _explorerListViewModel;
        private readonly ExplorerListViewModel _explorerListViewModel;

        /// <summary>
        /// 模型列表 ViewModel
        /// </summary>
        public SpineObjectListViewModel SpineObjectListViewModel => _spineObjectListViewModel;
        private readonly SpineObjectListViewModel _spineObjectListViewModel;

        /// <summary>
        /// 模型属性页 ViewModel
        /// </summary>
        public SpineObjectTabViewModel SpineObjectTabViewModel => _spineObjectTabViewModel;
        private readonly SpineObjectTabViewModel _spineObjectTabViewModel = new();

        /// <summary>
        /// SFML 渲染 ViewModel
        /// </summary>
        public SFMLRendererViewModel SFMLRendererViewModel => _sfmlRendererViewModel;
        private readonly SFMLRendererViewModel _sfmlRendererViewModel;

        /// <summary>
        /// 打开工作区
        /// </summary>
        public RelayCommand Cmd_OpenWorkspace => _cmd_OpenWorkspace ??= new(OpenWorkspace_Execute);
        private RelayCommand? _cmd_OpenWorkspace;

        private void OpenWorkspace_Execute()
        {
            if (!DialogService.ShowOpenJsonDialog(out var fileName)) return;
            if (JsonHelper.Deserialize<WorkspaceModel>(fileName, out var obj))
            {
                Workspace = obj;
            }
        }

        /// <summary>
        /// 保存工作区
        /// </summary>
        public RelayCommand Cmd_SaveWorkspace => _cmd_SaveWorkspace ??= new(SaveWorkspace_Execute);
        private RelayCommand? _cmd_SaveWorkspace;

        private void SaveWorkspace_Execute()
        {
            string fileName = "workspace.jcfg";
            if (!DialogService.ShowSaveJsonDialog(ref fileName)) return;
            JsonHelper.Serialize(Workspace, fileName);
        }

        /// <summary>
        /// 显示诊断信息对话框
        /// </summary>
        public RelayCommand Cmd_ShowDiagnosticsDialog => _cmd_ShowDiagnosticsDialog ??= new(() => { DialogService.ShowDiagnosticsDialog(); });
        private RelayCommand? _cmd_ShowDiagnosticsDialog;

        /// <summary>
        /// 显示关于对话框
        /// </summary>
        public RelayCommand Cmd_ShowAboutDialog => _cmd_ShowAboutDialog ??= new(() => { DialogService.ShowAboutDialog(); });
        private RelayCommand? _cmd_ShowAboutDialog;

        public WorkspaceModel Workspace
        {
            get
            {
                return new()
                {
                    ExploringDirectory = _explorerListViewModel.CurrentDirectory,
                    RendererConfig = _sfmlRendererViewModel.WorkspaceConfig,
                    LoadedSpineObjects = _spineObjectListViewModel.LoadedSpineObjects
                }; 
            }
            set
            {
                _explorerListViewModel.CurrentDirectory = value.ExploringDirectory;
                _sfmlRendererViewModel.WorkspaceConfig = value.RendererConfig;
                _spineObjectListViewModel.LoadedSpineObjects = value.LoadedSpineObjects;
            }
        }

        /// <summary>
        /// 调试命令
        /// </summary>
        public RelayCommand Cmd_Debug => _cmd_Debug ??= new(Debug_Execute);
        private RelayCommand? _cmd_Debug;

        private void Debug_Execute()
        {
#if DEBUG

            MessagePopupService.Quest("测试一下");
#endif
        }
    }
}