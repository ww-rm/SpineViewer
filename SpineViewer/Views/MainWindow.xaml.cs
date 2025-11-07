using NLog;
using SFMLRenderer;
using Spine;
using SpineViewer.Models;
using SpineViewer.Natives;
using SpineViewer.Resources;
using SpineViewer.Services;
using SpineViewer.Utils;
using SpineViewer.ViewModels.Exporters;
using SpineViewer.ViewModels.MainWindow;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace SpineViewer.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 上一次状态文件保存路径
    /// </summary>
    public static readonly string UserStateFilePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "userstate.json");

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private ListViewItem? _listViewDragSourceItem = null;
    private Point _listViewDragSourcePoint;

    private readonly SFMLRenderWindow _wallpaperRenderWindow;
    private readonly MainWindowViewModel _vm;

    private readonly List<IDisposable> _userStateWatchers = [];
    private DispatcherTimer _saveUserStateTimer;
    private readonly TimeSpan _saveTimerDelay = TimeSpan.FromSeconds(1);

    public bool RootGridCol0Folded
    {
        get => ((ContentPresenter)_mainTabControl.Template.FindName("PART_SelectedContentHost", _mainTabControl)).Visibility != Visibility.Visible;
        set
        {
            var mainTabContentHost = (ContentPresenter)_mainTabControl.Template.FindName("PART_SelectedContentHost", _mainTabControl);
            if (mainTabContentHost is null)
            {
                _mainTabControl.ApplyTemplate();
                mainTabContentHost = (ContentPresenter)_mainTabControl.Template.FindName("PART_SelectedContentHost", _mainTabControl);
            }
            if (mainTabContentHost is null)
            {
                _logger.Warn("Failed to set property {0}", nameof(RootGridCol0Folded));
                return;
            }
            if ((mainTabContentHost.Visibility != Visibility.Visible) == value)
                return;

            if (value)
            {
                // 寄存折叠前的宽度比例
                _rootGrid.ColumnDefinitions[0].Tag = _rootGrid.ColumnDefinitions[0].Width;
                _rootGrid.ColumnDefinitions[1].Tag = _rootGrid.ColumnDefinitions[1].Width;
                _rootGrid.ColumnDefinitions[2].Tag = _rootGrid.ColumnDefinitions[2].Width;

                // 进行折叠
                mainTabContentHost.Visibility = Visibility.Collapsed;
                _rootGrid.ColumnDefinitions[0].Width = GridLength.Auto;
                _rootGrid.ColumnDefinitions[1].Width = new(0);
            }
            else
            {
                // 解除折叠
                _rootGrid.ColumnDefinitions[0].Width = (GridLength)_rootGrid.ColumnDefinitions[0].Tag;
                _rootGrid.ColumnDefinitions[1].Width = (GridLength)_rootGrid.ColumnDefinitions[1].Tag;
                _rootGrid.ColumnDefinitions[2].Width = (GridLength)_rootGrid.ColumnDefinitions[2].Tag;
                mainTabContentHost.Visibility = Visibility.Visible;
            }
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        InitializeLogConfiguration();

        // Initialize Wallpaper RenderWindow
        _wallpaperRenderWindow = new(new(1, 1), "SpineViewerWallpaper", SFML.Window.Styles.None);
        _wallpaperRenderWindow.SetVisible(false);
        var handle = _wallpaperRenderWindow.SystemHandle;
        var style = User32.GetWindowLong(handle, User32.GWL_STYLE) | User32.WS_POPUP;
        var exStyle = User32.GetWindowLong(handle, User32.GWL_EXSTYLE) | User32.WS_EX_LAYERED | User32.WS_EX_TOOLWINDOW;
        User32.SetWindowLong(handle, User32.GWL_STYLE, style);
        User32.SetWindowLong(handle, User32.GWL_EXSTYLE, exStyle);
        User32.SetLayeredWindowAttributes(handle, 0, byte.MaxValue, User32.LWA_ALPHA);

        DataContext = _vm = new(_renderPanel, _wallpaperRenderWindow);

        // XXX: hc 的 NotifyIcon 的 Text 似乎没法双向绑定
        _notifyIcon.Text = _vm.Title;

        SourceInitialized += MainWindow_SourceInitialized;
        Loaded += MainWindow_Loaded;
        ContentRendered += MainWindow_ContentRendered;
        Closing += MainWindow_Closing;
        Closed += MainWindow_Closed;
        _vm.SpineObjectListViewModel.RequestSelectionChanging += SpinesListView_RequestSelectionChanging;
        _vm.SFMLRendererViewModel.RequestSelectionChanging += SpinesListView_RequestSelectionChanging;

        _vm.SFMLRendererViewModel.PropertyChanged += SFMLRendererViewModel_PropertyChanged;
    }

    /// <summary>
    /// 初始化窗口日志器
    /// </summary>
    private void InitializeLogConfiguration()
    {
        // 窗口日志
        var rtbTarget = new NLog.Windows.Wpf.RichTextBoxTarget
        {
            Name = "rtbTarget",
            Layout = "[${level:format=OneLetter}]${date:format=yyyy-MM-dd HH\\:mm\\:ss} - ${message}",
            WindowName = _mainWindow.Name,
            ControlName = _loggerRichTextBox.Name,
            AutoScroll = true,
            MaxLines = 3000,
        };

        rtbTarget.WordColoringRules.Add(new("[D]", "Gray", "Empty"));
        rtbTarget.WordColoringRules.Add(new("[I]", "DimGray", "Empty"));
        rtbTarget.WordColoringRules.Add(new("[W]", "DarkOrange", "Empty"));
        rtbTarget.WordColoringRules.Add(new("[E]", "Red", "Empty"));
        rtbTarget.WordColoringRules.Add(new("[F]", "White", "DarkRed"));

        LogManager.Configuration.AddTarget(rtbTarget);
        LogManager.Configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, rtbTarget);
        LogManager.ReconfigExistingLoggers();
    }

    #region MainWindow 事件处理

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        Dwmapi.SetWindowTextColor(hwnd, AppResource.Color_PrimaryText);
        Dwmapi.SetWindowCaptionColor(hwnd, AppResource.Color_Region);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var vm = _vm.SFMLRendererViewModel;
        _renderPanel.CanvasMouseWheelScrolled += vm.CanvasMouseWheelScrolled;
        _renderPanel.CanvasMouseButtonPressed += (s, e) => { vm.CanvasMouseButtonPressed(s, e); _spinesListView.Focus(); }; // 用户点击画布后强制转移焦点至列表
        _renderPanel.CanvasMouseMove += vm.CanvasMouseMove;
        _renderPanel.CanvasMouseButtonReleased += vm.CanvasMouseButtonReleased;

        // 设置默认参数并启动渲染
        vm.SetResolution(1500, 1000);
        vm.Zoom = 0.75f;
        vm.CenterX = 0;
        vm.CenterY = 0;
        vm.FlipY = true;
        vm.MaxFps = 30;
        vm.StartRender();

        // 加载首选项
        _vm.PreferenceViewModel.LoadPreference();

        // 还原上一次用户历史状态并开启监听器
        LoadUserState();
        AddUserStateListeners();
    }

    private void MainWindow_ContentRendered(object? sender, EventArgs e)
    {
        string[] args = Environment.GetCommandLineArgs();

        // 不带参数启动
        if (args.Length <= 1)
            return;

        // 带一个参数启动, 允许提供一些启动选项
        if (args.Length == 2)
        {
            if (args[1] == App.AutoRunFlag)
            {
                var autoPath = _vm.AutoRunWorkspaceConfigPath;
                if (!string.IsNullOrWhiteSpace(autoPath) && JsonHelper.Deserialize<WorkspaceModel>(autoPath, out var obj))
                    _vm.Workspace = obj;
                return;
            }
        }

        // 其余提供了任意参数的情况
        string[] filePaths = args.Skip(1).ToArray();
        _vm.SpineObjectListViewModel.AddSpineObjectFromFileList(filePaths);
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!_vm.IsShuttingDownFromTray)
        {
            if (_vm.CloseToTray is true)
            {
                Hide();
                e.Cancel = true;
                return;
            }
        }

        // 移除监听器并保存当前用户状态
        RemoveUserStateListensers();
        SaveUserState();

        _vm.SFMLRendererViewModel.StopRender();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {

    }

    private void LoadUserState()
    {
        if (JsonHelper.Deserialize<UserStateModel>(UserStateFilePath, out var m, true))
        {
            Left = m.WindowLeft;
            Top = m.WindowTop;
            Width = m.WindowWidth;
            Height = m.WindowHeight;
            if (m.WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }

            if (m.RootGridCol0Folded)
            {
                RootGridCol0Folded = true;
                _rootGrid.ColumnDefinitions[0].Tag = new GridLength(m.RootGridCol0Width, GridUnitType.Star);
                _rootGrid.ColumnDefinitions[2].Tag = new GridLength(m.RootGridCol2Width, GridUnitType.Star);
            }
            else
            {
                RootGridCol0Folded = false;
                _rootGrid.ColumnDefinitions[0].Width = new(m.RootGridCol0Width, GridUnitType.Star);
                _rootGrid.ColumnDefinitions[2].Width = new(m.RootGridCol2Width, GridUnitType.Star);
            }

            _modelListGrid.RowDefinitions[0].Height = new(m.ModelListRow0Height, GridUnitType.Star);
            _modelListGrid.RowDefinitions[2].Height = new(m.ModelListRow2Height, GridUnitType.Star);

            _explorerGrid.RowDefinitions[0].Height = new(m.ExplorerGridRow0Height, GridUnitType.Star);
            _explorerGrid.RowDefinitions[2].Height = new(m.ExplorerGridRow2Height, GridUnitType.Star);

            _rightPanelGrid.RowDefinitions[0].Height = new(m.RightPanelGridRow0Height, GridUnitType.Star);
            _rightPanelGrid.RowDefinitions[2].Height = new(m.RightPanelGridRow2Height, GridUnitType.Star);

            _vm.ExplorerListViewModel.CurrentDirectory = m.ExploringDirectory;

            _vm.SFMLRendererViewModel.SetResolution(m.ResolutionX, m.ResolutionY);
            _vm.SFMLRendererViewModel.MaxFps = m.MaxFps;
            _vm.SFMLRendererViewModel.Speed = m.Speed;
            _vm.SFMLRendererViewModel.ShowAxis = m.ShowAxis;
            _vm.SFMLRendererViewModel.BackgroundColor = m.BackgroundColor;
            _vm.SFMLRendererViewModel.BackgroundImageMode = m.BackgroundImageMode;
        }
    }

    private void SaveUserState()
    {
        var rb = RestoreBounds;
        var m = new UserStateModel()
        {
            WindowLeft = rb.Left,
            WindowTop = rb.Top,
            WindowWidth = rb.Width,
            WindowHeight = rb.Height,
            WindowState = WindowState,

            RootGridCol0Folded = RootGridCol0Folded,
            RootGridCol0Width = _rootGrid.ColumnDefinitions[0].Width.Value,
            RootGridCol2Width = _rootGrid.ColumnDefinitions[2].Width.Value,

            ModelListRow0Height = _modelListGrid.RowDefinitions[0].Height.Value,
            ModelListRow2Height = _modelListGrid.RowDefinitions[2].Height.Value,

            ExplorerGridRow0Height = _explorerGrid.RowDefinitions[0].Height.Value,
            ExplorerGridRow2Height = _explorerGrid.RowDefinitions[2].Height.Value,

            RightPanelGridRow0Height = _rightPanelGrid.RowDefinitions[0].Height.Value,
            RightPanelGridRow2Height = _rightPanelGrid.RowDefinitions[2].Height.Value,

            ExploringDirectory = _vm.ExplorerListViewModel.CurrentDirectory,

            ResolutionX = _vm.SFMLRendererViewModel.ResolutionX,
            ResolutionY = _vm.SFMLRendererViewModel.ResolutionY,
            MaxFps = _vm.SFMLRendererViewModel.MaxFps,
            Speed = _vm.SFMLRendererViewModel.Speed,
            ShowAxis = _vm.SFMLRendererViewModel.ShowAxis,
            BackgroundColor = _vm.SFMLRendererViewModel.BackgroundColor,
            BackgroundImageMode = _vm.SFMLRendererViewModel.BackgroundImageMode,
        };

        if (m.RootGridCol0Folded)
        {
            m.RootGridCol0Width = ((GridLength)_rootGrid.ColumnDefinitions[0].Tag).Value;
            m.RootGridCol2Width = ((GridLength)_rootGrid.ColumnDefinitions[2].Tag).Value;
        }

        JsonHelper.Serialize(m, UserStateFilePath);
    }

    /// <summary>
    /// <see cref="SaveUserState"/> 的延时版本, 避免一次性大量执行
    /// </summary>
    private void DelayedSaveUserState()
    {
        // 第一次调用时创建定时器
        if (_saveUserStateTimer == null)
        {
            _saveUserStateTimer = new() { Interval = _saveTimerDelay };
            _saveUserStateTimer.Tick += (s, e) =>
            {
                _saveUserStateTimer.Stop();
                SaveUserState();
            };
        }

        // 每次触发都重置间隔和计时
        _saveUserStateTimer.Stop();
        _saveUserStateTimer.Start();
    }

    private void AddUserStateListeners()
    {
        // 添加用户状态监听器
        _userStateWatchers.Add(PropertyWatcher.Watch(this, MainWindow.WidthProperty, DelayedSaveUserState));
        _userStateWatchers.Add(PropertyWatcher.Watch(this, MainWindow.HeightProperty, DelayedSaveUserState));
        _userStateWatchers.Add(PropertyWatcher.Watch(this, MainWindow.LeftProperty, DelayedSaveUserState));
        _userStateWatchers.Add(PropertyWatcher.Watch(this, MainWindow.TopProperty, DelayedSaveUserState));
        _userStateWatchers.Add(PropertyWatcher.Watch(this, MainWindow.WindowStateProperty, DelayedSaveUserState));

        _userStateWatchers.Add(PropertyWatcher.Watch(_rootGrid.ColumnDefinitions[0], ColumnDefinition.WidthProperty, DelayedSaveUserState));
        _userStateWatchers.Add(PropertyWatcher.Watch(_rootGrid.ColumnDefinitions[2], ColumnDefinition.WidthProperty, DelayedSaveUserState));

        _userStateWatchers.Add(PropertyWatcher.Watch(_modelListGrid.RowDefinitions[0], RowDefinition.HeightProperty, DelayedSaveUserState));
        _userStateWatchers.Add(PropertyWatcher.Watch(_modelListGrid.RowDefinitions[2], RowDefinition.HeightProperty, DelayedSaveUserState));

        _userStateWatchers.Add(PropertyWatcher.Watch(_explorerGrid.RowDefinitions[0], RowDefinition.HeightProperty, DelayedSaveUserState));
        _userStateWatchers.Add(PropertyWatcher.Watch(_explorerGrid.RowDefinitions[2], RowDefinition.HeightProperty, DelayedSaveUserState));

        _userStateWatchers.Add(PropertyWatcher.Watch(_rightPanelGrid.RowDefinitions[0], RowDefinition.HeightProperty, DelayedSaveUserState));
        _userStateWatchers.Add(PropertyWatcher.Watch(_rightPanelGrid.RowDefinitions[2], RowDefinition.HeightProperty, DelayedSaveUserState));

        _vm.ExplorerListViewModel.PropertyChanged += ExplorerListUserStateChanged;
        _vm.SFMLRendererViewModel.PropertyChanged += SFMLRendererUserStateChanged;
    }

    private void RemoveUserStateListensers()
    {
        // 撤除所有状态监听器
        _vm.SFMLRendererViewModel.PropertyChanged -= SFMLRendererUserStateChanged;
        _vm.ExplorerListViewModel.PropertyChanged -= ExplorerListUserStateChanged;
        foreach (var w in _userStateWatchers) w.Dispose();
        _userStateWatchers.Clear();

    }

    private void ExplorerListUserStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ExplorerListViewModel.CurrentDirectory):
                DelayedSaveUserState();
                break;
            default:
                break;
        }
    }

    private void SFMLRendererUserStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SFMLRendererViewModel.ResolutionX):
            case nameof(SFMLRendererViewModel.ResolutionY):
            case nameof(SFMLRendererViewModel.MaxFps):
            case nameof(SFMLRendererViewModel.Speed):
            case nameof(SFMLRendererViewModel.ShowAxis):
            case nameof(SFMLRendererViewModel.BackgroundColor):
            case nameof(SFMLRendererViewModel.BackgroundImageMode):
                DelayedSaveUserState();
                break;
            default:
                break;
        }
    }

    #endregion

    #region ColorPicker 弹窗事件处理

    private void ButtonPickColor_Click(object sender, RoutedEventArgs e)
    {
        _colorPopup.IsOpen = !_colorPopup.IsOpen;
    }

    private void ColorPicker_Confirmed(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
    {
        _colorPopup.IsOpen = false;
        var color = e.Info;
        var vm = ((MainWindowViewModel)DataContext).SFMLRendererViewModel;
        vm.BackgroundColor = color;
    }

    private void ColorPicker_Canceled(object sender, EventArgs e)
    {
        _colorPopup.IsOpen = false;
    }

    #endregion

    #region ViewModel PropertyChanged 事件处理

    private void SFMLRendererViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // XXX: 资源管理器重启后窗口会有问题无法重新显示, 需要重启应用, 否则要重新创建窗口
        if (e.PropertyName == nameof(SFMLRendererViewModel.WallpaperView))
        {
            var wnd = _wallpaperRenderWindow;
            if (_vm.SFMLRendererViewModel.WallpaperView)
            {
                var workerw = User32.GetWorkerW();
                if (workerw == IntPtr.Zero)
                {
                    _logger.Error("Failed to enable wallpaper view, WorkerW not found");
                    return;
                }
                var handle = wnd.SystemHandle;

                User32.GetPrimaryScreenResolution(out var sw, out var sh);

                User32.SetParent(handle, workerw);
                User32.SetLayeredWindowAttributes(handle, 0, byte.MaxValue, User32.LWA_ALPHA);

                _vm.SFMLRendererViewModel.SetResolution(sw, sh);
                wnd.Position = new(0, 0);
                wnd.Size = new(sw + 1, sh);
                wnd.Size = new(sw, sh);
                wnd.SetVisible(true);
            }
            else
            {
                wnd.SetVisible(false);
            }
        }
    }

    #endregion

    #region _mainTabControl 事件处理

    private void MainTabControlHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 全屏状态忽略该功能
        if (_fullScreenLayout.Visibility == Visibility.Visible)
            return;

        if (sender is not FrameworkElement fe)
            return;

        // 找到包含这个 Border 的 TabItem
        var tabItem = VisualFindParent<TabItem>(fe);
        if (tabItem is null) 
            return;

        if (_mainTabControl.SelectedItem == tabItem)
        {
            RootGridCol0Folded = !RootGridCol0Folded;
        }
        else
        {
            RootGridCol0Folded = false;
        }
    }

    #endregion

    #region _spinesListView 事件处理

    private void SpinesListView_RequestSelectionChanging(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _spinesListView.SelectedItems.Add(e.NewItems[0]);
                break;
            case NotifyCollectionChangedAction.Remove:
                _spinesListView.SelectedItems.Remove(e.OldItems[0]);
                break;
            case NotifyCollectionChangedAction.Replace:
                _spinesListView.SelectedItems[e.NewStartingIndex] = e.NewItems[0];
                break;
            case NotifyCollectionChangedAction.Move:
                _spinesListView.SelectedItems.RemoveAt(e.OldStartingIndex);
                _spinesListView.SelectedItems.Insert(e.NewStartingIndex, e.NewItems[0]);
                break;
            case NotifyCollectionChangedAction.Reset:
                _spinesListView.SelectedItems.Clear();
                break;
            default:
                break;
        }

        // 如果选中项发生变化也强制转移焦点
        _spinesListView.Focus();
    }

    private void SpinesListView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var list = (ListView)sender;
        if (VisualUpwardSearch<ListViewItem>(e.OriginalSource as DependencyObject) is null)
            list.SelectedItems.Clear();
    }

    private void SpinesListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var list = (ListView)sender;

        if (list.SelectedItems.Count > 1) return;

        // 找到鼠标当前所在的 ListViewItem
        _listViewDragSourceItem = VisualUpwardSearch<ListViewItem>(e.OriginalSource as DependencyObject);
        _listViewDragSourcePoint = e.GetPosition(null);

        // 如果点到的是空白也让其获取焦点
        if (_listViewDragSourceItem is null)
            list.Focus();
    }

    private void SpinesListView_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (_listViewDragSourceItem is null) return;

        var diff = _listViewDragSourcePoint - e.GetPosition(null);
        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        var list = (ListView)sender;
        if (list.SelectedItems.Count > 1) return;

        var sp = list.ItemContainerGenerator.ItemFromContainer(_listViewDragSourceItem);
        DragDrop.DoDragDrop(_listViewDragSourceItem, sp, DragDropEffects.Move);
    }

    private void SpinesListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _listViewDragSourceItem = null;
        _listViewDragSourcePoint = default;
    }

    private void SpinesListView_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(Models.SpineObjectModel)))
            e.Effects = DragDropEffects.Move;
        else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void SpinesListView_Drop(object sender, DragEventArgs e)
    {
        var list = (ListView)sender;
        var vm = (SpineObjectListViewModel)list.DataContext;

        if (e.Data.GetDataPresent(typeof(Models.SpineObjectModel)))
        {
            var srcObject = (Models.SpineObjectModel)e.Data.GetData(typeof(Models.SpineObjectModel))!;
            int srcIdx = list.Items.IndexOf(srcObject);
            if (srcIdx < 0) return;

            Point pt = e.GetPosition(list);
            var obj = list.InputHitTest(pt) as DependencyObject;
            // 找到当前鼠标下的 ListViewItem
            var dstListViewItem = VisualUpwardSearch<ListViewItem>(obj);
            int dstIdx = -1;
            if (dstListViewItem != null)
            {
                var dstObject = (Models.SpineObjectModel)list.ItemContainerGenerator.ItemFromContainer(dstListViewItem);
                dstIdx = list.Items.IndexOf(dstObject);
            }
            if (dstIdx < 0) return;

            lock (vm.SpineObjects.Lock) vm.SpineObjects.Move(srcIdx, dstIdx);
        }
        else if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var paths = e.Data.GetData(DataFormats.FileDrop);
            vm.AddSpineObjectFromFileList((string[])paths);
        }
        e.Handled = true;
    }

    #endregion

    #region _spineFilesListBox 事件

    private void SpineFilesListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var list = (ListBox)sender;
        if (VisualUpwardSearch<ListBoxItem>(e.OriginalSource as DependencyObject) is null)
            list.SelectedItems.Clear();
    }

    #endregion

    #region _nofityIcon 事件处理

    private void _notifyIcon_Click(object sender, RoutedEventArgs e)
    {
        
    }

    private void _notifyIcon_MouseDoubleClick(object sender, RoutedEventArgs e)
    {
        Show();
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }
        Activate();
    }

    #endregion

    #region 切换全屏布局事件处理

    private void SwitchToFullScreenLayout()
    {
        // XXX: 操作系统设置里关闭窗口化游戏优化选项可以避免恰好全屏时的闪烁问题

        if (_fullScreenLayout.Visibility == Visibility.Visible) return;

        RootGridCol0Folded = false; // 取消折叠

        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        if (User32.GetScreenResolution(hwnd, out var resX, out var resY))
        {
            _vm.SFMLRendererViewModel.SetResolution(resX, resY);
        }

        HandyControl.Controls.IconElement.SetGeometry(_fullScreenButton, AppResource.Geo_ArrowsMinimize);

        Topmost = true;
        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Maximized;
        _normalLayout.Visibility = Visibility.Collapsed;
        _fullScreenLayout.Visibility = Visibility.Visible;

        _leftEdgePopup.IsOpen = true;
        _topEdgePopup.IsOpen = true;
        _rightEdgePopup.IsOpen = true;
        _bottomEdgePopup.IsOpen = true;

        _renderPanelContainer.Child = null;
        _renderPanelContainerFull.Child = _renderPanel;

        _mainTabControlContainer.Child = null;
        _mainTabControlPopupContainer.Child = _mainTabControl;

        _mainMenuContainer.Child = null;
        _mainMenuPopupContainer.Child = _mainMenu;

        _renderPanelButtonsContainer.Child = null;
        _renderPanelButtonsPopupContainer.Child = _renderPanelButtonsPanel;

        _loggerBoxContainer.Child = null;
        _loggerBoxPopupContainer.Child = _loggerRichTextBox;
    }

    private void SwitchToNormalLayout()
    {
        if (_fullScreenLayout.Visibility != Visibility.Visible) return;

        HandyControl.Controls.IconElement.SetGeometry(_fullScreenButton, AppResource.Geo_ArrowsMaximize);

        _loggerBoxPopupContainer.Child = null;
        _loggerBoxContainer.Child = _loggerRichTextBox;

        _renderPanelButtonsPopupContainer.Child = null;
        _renderPanelButtonsContainer.Child = _renderPanelButtonsPanel;

        _mainMenuPopupContainer.Child = null;
        _mainMenuContainer.Child = _mainMenu;

        _mainTabControlPopupContainer.Child = null;
        _mainTabControlContainer.Child = _mainTabControl;

        _renderPanelContainerFull.Child = null;
        _renderPanelContainer.Child = _renderPanel;

        _leftEdgePopup.IsOpen = false;
        _topEdgePopup.IsOpen = false;
        _rightEdgePopup.IsOpen = false;
        _bottomEdgePopup.IsOpen = false;

        _fullScreenLayout.Visibility = Visibility.Collapsed;
        _normalLayout.Visibility = Visibility.Visible;
        WindowState = WindowState.Normal;
        WindowStyle = WindowStyle.SingleBorderWindow;
        Topmost = false;
    }

    private void ButtonFullScreen_Click(object sender, RoutedEventArgs e)
    {
        if (_fullScreenLayout.Visibility != Visibility.Visible)
            SwitchToFullScreenLayout();
        else
            SwitchToNormalLayout();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (_fullScreenLayout.Visibility == Visibility.Visible)
            {
                SwitchToNormalLayout();
                e.Handled = true;
            }
        }
        else if (e.Key == Key.F11)
        {
            if (_fullScreenLayout.Visibility != Visibility.Visible)
                SwitchToFullScreenLayout();
            else
                SwitchToNormalLayout();
            e.Handled = true;
        }
    }

    private void MainWindow_LocationChanged(object sender, EventArgs e)
    {
        if (_fullScreenLayout.Visibility != Visibility.Visible) return;

        var source = PresentationSource.FromVisual(this);
        if (source is null) return;

        var toDip = source.CompositionTarget.TransformFromDevice;

        var leftTop = toDip.Transform(_fullScreenLayout.PointToScreen(new(0, 0)));
        var rightBottom = toDip.Transform(_fullScreenLayout.PointToScreen(new(_fullScreenLayout.ActualWidth, _fullScreenLayout.ActualHeight)));

        _leftEdgePopup.HorizontalOffset = leftTop.X;
        _leftEdgePopup.VerticalOffset = leftTop.Y;

        _topEdgePopup.HorizontalOffset = leftTop.X;
        _topEdgePopup.VerticalOffset = leftTop.Y;

        _rightEdgePopup.HorizontalOffset = rightBottom.X - _rightEdgePopup.Width;
        _rightEdgePopup.VerticalOffset = leftTop.Y;

        _bottomEdgePopup.HorizontalOffset = leftTop.X;
        _bottomEdgePopup.VerticalOffset = rightBottom.Y - _bottomEdgePopup.Height;
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_fullScreenLayout.Visibility != Visibility.Visible) return;

        var source = PresentationSource.FromVisual(this);
        if (source is null) return;

        var toDip = source.CompositionTarget.TransformFromDevice;

        var leftTop = toDip.Transform(_fullScreenLayout.PointToScreen(new(0, 0)));
        var rightBottom = toDip.Transform(_fullScreenLayout.PointToScreen(new(_fullScreenLayout.ActualWidth, _fullScreenLayout.ActualHeight)));

        _leftEdgePopup.HorizontalOffset = leftTop.X;
        _leftEdgePopup.VerticalOffset = leftTop.Y;

        _topEdgePopup.HorizontalOffset = leftTop.X;
        _topEdgePopup.VerticalOffset = leftTop.Y;

        _rightEdgePopup.HorizontalOffset = rightBottom.X - _rightEdgePopup.Width;
        _rightEdgePopup.VerticalOffset = leftTop.Y;

        _bottomEdgePopup.HorizontalOffset = leftTop.X;
        _bottomEdgePopup.VerticalOffset = rightBottom.Y - _bottomEdgePopup.Height;
    }

    private void LeftPopup_Opened(object sender, EventArgs e)
    {
        if (_fullScreenLayout.Visibility != Visibility.Visible) return;

        var source = PresentationSource.FromVisual(this);
        if (source is null) return;

        var toDip = source.CompositionTarget.TransformFromDevice;
        var leftTop = toDip.Transform(_fullScreenLayout.PointToScreen(new(0, 0)));

        var popup = (Popup)sender;

        popup.HorizontalOffset = leftTop.X;
        popup.VerticalOffset = leftTop.Y;
    }

    private void TopPopup_Opened(object sender, EventArgs e)
    {
        if (_fullScreenLayout.Visibility != Visibility.Visible) return;

        var source = PresentationSource.FromVisual(this);
        if (source is null) return;

        var toDip = source.CompositionTarget.TransformFromDevice;
        var leftTop = toDip.Transform(_fullScreenLayout.PointToScreen(new(0, 0)));

        var popup = (Popup)sender;

        popup.HorizontalOffset = leftTop.X;
        popup.VerticalOffset = leftTop.Y;
    }

    private void RightPopup_Opened(object sender, EventArgs e)
    {
        if (_fullScreenLayout.Visibility != Visibility.Visible) return;

        var source = PresentationSource.FromVisual(this);
        if (source is null) return;

        var toDip = source.CompositionTarget.TransformFromDevice;

        var leftTop = toDip.Transform(_fullScreenLayout.PointToScreen(new(0, 0)));
        var rightBottom = toDip.Transform(_fullScreenLayout.PointToScreen(new(_fullScreenLayout.ActualWidth, _fullScreenLayout.ActualHeight)));

        var popup = (Popup)sender;

        popup.HorizontalOffset = rightBottom.X - popup.Width;
        popup.VerticalOffset = leftTop.Y;
    }

    private void BottomPopup_Opened(object sender, EventArgs e)
    {
        if (_fullScreenLayout.Visibility != Visibility.Visible) return;

        var source = PresentationSource.FromVisual(this);
        if (source is null) return;

        var toDip = source.CompositionTarget.TransformFromDevice;

        var leftTop = toDip.Transform(_fullScreenLayout.PointToScreen(new(0, 0)));
        var rightBottom = toDip.Transform(_fullScreenLayout.PointToScreen(new(_fullScreenLayout.ActualWidth, _fullScreenLayout.ActualHeight)));

        var popup = (Popup)sender;

        popup.HorizontalOffset = leftTop.X;
        popup.VerticalOffset = rightBottom.Y - popup.Height;
    }

    private void LeftEdgePopup_MouseEnter(object sender, MouseEventArgs e)
    {
        _mainTabControlPopupContainer.IsOpen = true;
    }

    private void TopEdgePopup_MouseEnter(object sender, MouseEventArgs e)
    {
        _mainMenuPopupContainer.IsOpen = true;
    }

    private void RightEdgePopup_MouseEnter(object sender, MouseEventArgs e)
    {

    }

    private void BottomEdgePopup_MouseEnter(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(_fullScreenLayout);
        var width = _fullScreenLayout.ActualWidth;
        if (pos.X < width * 0.8)
        {
            _renderPanelButtonsPopupContainer.IsOpen = true;
        }
        else
        {
            _loggerBoxPopupContainer.IsOpen = true;
        }
    }

    private void PopupContainer_MouseLeave(object sender, MouseEventArgs e)
    {
        ((Popup)sender).IsOpen = false;
    }

    #endregion

    private static T? VisualUpwardSearch<T>(DependencyObject? source) where T : DependencyObject
    {
        while (source != null && source is not T)
            source = VisualTreeHelper.GetParent(source);
        return source as T;
    }

    public static T? VisualFindParent<T>(DependencyObject child) where T : DependencyObject 
        => VisualUpwardSearch<T>(VisualTreeHelper.GetParent(child));

    private void DebugMenuItem_Click(object sender, RoutedEventArgs e)
    {
#if DEBUG
        _logger.Debug("Debug");
        _logger.Info("Info");
        _logger.Warn("Warn");
        _logger.Error("Error");
        _logger.Fatal("Fatal");
        return;
#endif
    }

}