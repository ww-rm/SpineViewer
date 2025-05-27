using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using NLog;
using SFMLRenderer;
using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Services;
using SpineViewer.ViewModels.Exporters;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shell;

namespace SpineViewer.ViewModels
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
        }

        public string Title => $"SpineViewer - {App.Version}";

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
        /// 显示诊断信息对话框
        /// </summary>
        public RelayCommand Cmd_ShowDiagnosticsDialog => _cmd_ShowDiagnosticsDialog ??= new(() => { DiagnosticsDialogService.ShowDiagnosticsDialog(); });
        private RelayCommand? _cmd_ShowDiagnosticsDialog;

        /// <summary>
        /// 显示关于对话框
        /// </summary>
        public RelayCommand Cmd_ShowAboutDialog => _cmd_ShowAboutDialog ??= new(() => { AboutDialogService.ShowAboutDialog(); });
        private RelayCommand? _cmd_ShowAboutDialog;

        /// <summary>
        /// 调试命令
        /// </summary>
        public RelayCommand Cmd_Debug => _cmd_Debug ??= new(Debug_Execute);
        private RelayCommand? _cmd_Debug;

        private void Debug_Execute()
        {
#if DEBUG
            var path = @"C:\Users\ljh\Desktop\a.mp4";

            using var exporter = new FFmpegVideoExporter(_sfmlRenderer.Resolution);
            using var view = _sfmlRenderer.GetView();
            exporter.Center = view.Center;
            exporter.Size = view.Size;
            exporter.Rotation = view.Rotation;
            exporter.Viewport = view.Viewport;

            SpineObject[] spines;
            lock (_spineObjectModels.Lock)
            {
                spines = _spineObjectModels.Select(it => it.GetSpineObject(true)).ToArray();
            }

            exporter.Fps = 60;
            exporter.Format = FFmpegVideoExporter.VideoFormat.Webm;
            exporter.Duration = 3;
            exporter.BackgroundColor = new(0, 0, 0, 0);
            ProgressService.RunAsync((pr, ct) =>
            {
                exporter.ProgressReporter = (total, done, text) =>
                { pr.Total = total; pr.Done = done; pr.ProgressText = text; };
                exporter.Export(path, ct, spines);
            }, "测试一下");
#endif
        }
    }
}