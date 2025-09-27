using NLog;
using NLog.Layouts;
using NLog.Targets;
using SFMLRenderer;
using Spine;
using SpineViewer.Models;
using SpineViewer.Natives;
using SpineViewer.Resources;
using SpineViewer.Utils;
using SpineViewer.ViewModels.MainWindow;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SpineViewer.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 上一次状态文件保存路径
    /// </summary>
    public static readonly string LastStateFilePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "laststate.json");

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private ListViewItem? _listViewDragSourceItem = null;
    private Point _listViewDragSourcePoint;

    private readonly SFMLRenderWindow _wallpaperRenderWindow;
    private readonly MainWindowViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        InitializeLogConfiguration();

        _wallpaperRenderWindow = new(new(500, 500), "SpineViewerWallpaper", SFML.Window.Styles.None);
        _wallpaperRenderWindow.SetVisible(false);
        DataContext = _vm = new(_renderPanel, _wallpaperRenderWindow);

        // XXX: hc 的 NotifyIcon 的 Text 似乎没法双向绑定
        _notifyIcon.Text = _vm.Title;

        _vm.SpineObjectListViewModel.RequestSelectionChanging += SpinesListView_RequestSelectionChanging;
        _vm.SFMLRendererViewModel.RequestSelectionChanging += SpinesListView_RequestSelectionChanging;
        Loaded += MainWindow_Loaded;
        ContentRendered += MainWindow_ContentRendered;
        Closed += MainWindow_Closed;
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

        LoadLastState();
    }

    private void MainWindow_ContentRendered(object? sender, EventArgs e)
    {
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            string[] filePaths = args.Skip(1).ToArray();
            _vm.SpineObjectListViewModel.AddSpineObjectFromFileList(filePaths);
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        SaveLastState();

        var vm = _vm.SFMLRendererViewModel;
        vm.StopRender();
    }

    /// <summary>
    /// 给管道通信提供的打开文件外部调用方法
    /// </summary>
    public void OpenFiles(IEnumerable<string> filePaths)
    {
        _vm.SpineObjectListViewModel.AddSpineObjectFromFileList(filePaths);
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
            WindowName = _mainWindow.Name,
            ControlName = _loggerRichTextBox.Name,
            AutoScroll = true,
            MaxLines = 3000,
            Layout = "[${level:format=OneLetter}]${date:format=yyyy-MM-dd HH\\:mm\\:ss} - ${message}",
        };

        rtbTarget.WordColoringRules.Add(new("[D]", "Gray", "Empty"));
        rtbTarget.WordColoringRules.Add(new("[I]", "DimGray", "Empty"));
        rtbTarget.WordColoringRules.Add(new("[W]", "DarkOrange", "Empty"));
        rtbTarget.WordColoringRules.Add(new("[E]", "Red", "Empty"));
        rtbTarget.WordColoringRules.Add(new("[F]", "DarkRed", "Empty"));

        LogManager.Configuration.AddTarget(rtbTarget);
        LogManager.Configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, rtbTarget);
        LogManager.ReconfigExistingLoggers();
    }

    private void LoadLastState()
    {
        if (JsonHelper.Deserialize<LastStateModel>(LastStateFilePath, out var m, true))
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

            _rootGrid.ColumnDefinitions[0].Width = new(m.RootGridCol0Width);
            _modelListGrid.RowDefinitions[0].Height = new(m.ModelListRow0Height);
            if (m.ExplorerGridRow0Height > 0) _explorerGrid.RowDefinitions[0].Height = new(m.ExplorerGridRow0Height);
            _rightPanelGrid.RowDefinitions[0].Height = new(m.RightPanelGridRow0Height);

            _vm.SFMLRendererViewModel.SetResolution(m.ResolutionX, m.ResolutionY);
            _vm.SFMLRendererViewModel.MaxFps = m.MaxFps;
            _vm.SFMLRendererViewModel.Speed = m.Speed;
            _vm.SFMLRendererViewModel.ShowAxis = m.ShowAxis;
            _vm.SFMLRendererViewModel.BackgroundColor = m.BackgroundColor;
            _vm.SFMLRendererViewModel.BackgroundImagePath = m.BackgroundImagePath;
            _vm.SFMLRendererViewModel.BackgroundImageMode = m.BackgroundImageMode;
        }
    }

    private void SaveLastState()
    {
        var m = new LastStateModel()
        {
            WindowLeft = Left,
            WindowTop = Top,
            WindowWidth = Width,
            WindowHeight = Height,
            WindowState = WindowState,

            RootGridCol0Width = _rootGrid.ColumnDefinitions[0].ActualWidth,
            ModelListRow0Height = _modelListGrid.RowDefinitions[0].ActualHeight,
            ExplorerGridRow0Height = _explorerGrid.RowDefinitions[0].ActualHeight,
            RightPanelGridRow0Height = _rightPanelGrid.RowDefinitions[0].ActualHeight,

            ResolutionX = _vm.SFMLRendererViewModel.ResolutionX,
            ResolutionY = _vm.SFMLRendererViewModel.ResolutionY,
            MaxFps = _vm.SFMLRendererViewModel.MaxFps,
            Speed = _vm.SFMLRendererViewModel.Speed,
            ShowAxis = _vm.SFMLRendererViewModel.ShowAxis,
            BackgroundColor = _vm.SFMLRendererViewModel.BackgroundColor,
            BackgroundImagePath = _vm.SFMLRendererViewModel.BackgroundImagePath,
            BackgroundImageMode = _vm.SFMLRendererViewModel.BackgroundImageMode,
        };

        JsonHelper.Serialize(m, LastStateFilePath);
    }

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

    private static T? VisualUpwardSearch<T>(DependencyObject? source) where T : DependencyObject
    {
        while (source != null && source is not T)
            source = VisualTreeHelper.GetParent(source);
        return source as T;
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

    }

    #endregion

    #region 切换全屏布局事件处理

    private void SwitchToFullScreenLayout()
    {
        // XXX: 操作系统设置里关闭窗口化游戏优化选项可以避免恰好全屏时的闪烁问题

        if (_fullScreenLayout.Visibility == Visibility.Visible) return;

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

    private void DebugMenuItem_Click(object sender, RoutedEventArgs e)
    {
#if DEBUG
        var www = _wallpaperRenderWindow;
        User32.GetPrimaryScreenResolution(out var sw, out var sh);
        www.Position = new(0, 0);
        www.Size = new(sw, sh);

        var handle = www.SystemHandle;
        Debug.WriteLine($"Handle: {handle:x8}");

        var style = User32.GetWindowLong(handle, User32.GWL_STYLE) | User32.WS_POPUP;
        var exStyle = User32.GetWindowLong(handle, User32.GWL_EXSTYLE) | User32.WS_EX_LAYERED | User32.WS_EX_TOOLWINDOW | User32.WS_EX_TOPMOST;
        User32.SetWindowLong(handle, User32.GWL_STYLE, style);
        User32.SetWindowLong(handle, User32.GWL_EXSTYLE, exStyle);
        User32.SetLayeredWindowAttributes(handle, 0, 200, User32.LWA_ALPHA);

        var workerw = User32.GetWorkerW();
        var res = User32.SetParent(handle, workerw);
        Debug.WriteLine($"SetParent: {res:x8}");

        User32.SetLayeredWindowAttributes(handle, 0, 255, User32.LWA_ALPHA);
        var s = www.Size;
        www.Size = new(s.X + 1, s.Y + 1);
        www.Size = s;
        www.SetVisible(true);
#endif
    }
}