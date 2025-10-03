using Microsoft.Win32;
using NLog;
using SpineViewer.Natives;
using SpineViewer.Resources;
using SpineViewer.ViewModels.MainWindow;
using SpineViewer.Views;
using System.Collections.Frozen;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

namespace SpineViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if DEBUG
        public const string AppName = "SpineViewer_D";
        public const string ProgId = "SpineViewer_D.skel";
#else
        public const string AppName = "SpineViewer";
        public const string ProgId = "SpineViewer.skel";
#endif

        public const string AutoRunFlag = "--autorun";
        private const string MutexName = "__SpineViewerInstance__";
        private const string PipeName = "__SpineViewerPipe__";

        public static readonly string ProcessPath = Environment.ProcessPath;
        public static readonly string ProcessDirectory = Path.GetDirectoryName(Environment.ProcessPath);
        public static readonly string ProcessName = Process.GetCurrentProcess().ProcessName;
        public static readonly string Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        private static readonly string AutoRunCommand = $"\"{ProcessPath}\" {AutoRunFlag}";

        private static readonly string SkelFileDescription = $"SpineViewer File";
        private static readonly string SkelIconFilePath = Path.Combine(ProcessDirectory, "Resources\\Images\\skel.ico");
        private static readonly string ShellOpenCommand = $"\"{ProcessPath}\" \"%1\"";

        private static readonly Logger _logger;
        private static readonly Mutex _instanceMutex;

        static App()
        {
            InitializeLogConfiguration();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Application Started, v{0}", Version);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                _logger.Fatal("Unhandled exception: {0}", e.ExceptionObject);
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                _logger.Trace(e.Exception.ToString());
                _logger.Error("Unobserved task exception: {0}", e.Exception.Message);
                e.SetObserved();
            };

            // 单例模式加 IPC 通信
            _instanceMutex = new Mutex(true, MutexName, out var createdNew);
            if (!createdNew)
            {
                ShowExistedInstance();
                SendCommandLineArgs();
                Environment.Exit(0); // 不再启动新实例
                return;
            }
            StartPipeServer();
        }

        private static void InitializeLogConfiguration()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // 文件日志
            var fileTarget = new NLog.Targets.FileTarget("fileTarget")
            {
                Encoding = System.Text.Encoding.UTF8,
                FileName = "${basedir}/logs/app.log",
                ArchiveFileName = "${basedir}/logs/app.{#}.log",
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Rolling,
                ArchiveAboveSize = 1048576,
                MaxArchiveFiles = 5,
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} - ${level:uppercase=true} - ${processid} - ${callsite-filename:includeSourcePath=false}:${callsite-linenumber} - ${message}",
                ConcurrentWrites = true,
                KeepFileOpen = false,
            };

            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);
            LogManager.Configuration = config;
        }

        private static void ShowExistedInstance()
        {
            try
            {
                // 遍历同名进程
                var processes = Process.GetProcessesByName(ProcessName);
                foreach (var p in processes)
                {
                    // 跳过当前进程
                    if (p.Id == Process.GetCurrentProcess().Id)
                        continue;

                    IntPtr hWnd = p.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        // 3. 显示并置顶窗口
                        if (User32.IsIconic(hWnd))
                        {
                            User32.ShowWindow(hWnd, User32.SW_RESTORE);
                        }
                        User32.SetForegroundWindow(hWnd);
                        break; // 找到一个就可以退出
                    }
                }
            }
            catch
            {
                // 忽略异常，不影响当前进程退出
            }
        }

        private static void SendCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            if (args.Length <= 0)
                return;

            _logger.Info("Send command line args to existed instance, \"{0}\"", string.Join(", ", args));
            try
            {
                // 已有实例在运行，把参数通过命名管道发过去
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    client.Connect(10000); // 10 秒超时
                    using (var writer = new StreamWriter(client))
                    {
                        foreach (var v in args)
                        {
                            writer.WriteLine(v);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(ex.ToString());
                _logger.Error("Failed to pass command line args to existed instance, {0}", ex.Message);
            }
        }

        private static void StartPipeServer()
        {
            var t = new Task(() =>
            {
                while (Current is null) Thread.Sleep(10);
                while (true)
                {
                    var windowCreated = false;
                    Current.Dispatcher.Invoke(() => windowCreated = Current.MainWindow is MainWindow);
                    if (windowCreated)
                        break;
                    else
                        Thread.Sleep(100);
                }
                while (true)
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        server.WaitForConnection();
                        using (var reader = new StreamReader(server))
                        {
                            var args = new List<string>();
                            string? line;
                            while ((line = reader.ReadLine()) != null)
                                args.Add(line);

                            if (args.Count > 0)
                            {
                                Current.Dispatcher.Invoke(() =>
                                {
                                    var vm = (MainWindowViewModel)((MainWindow)Current.MainWindow).DataContext;
                                    vm.SpineObjectListViewModel.AddSpineObjectFromFileList(args);
                                });
                            }
                        }
                    }
                }
            }, default, TaskCreationOptions.LongRunning);
            t.Start();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 正式启动窗口
            base.OnStartup(e);
            var uiCulture = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
            _logger.Info("Current UI Culture: {0}", uiCulture);
            if (uiCulture.StartsWith("zh")) { } // 默认就是中文, 无需操作
            else if (uiCulture.StartsWith("ja")) Language = AppLanguage.JA;
            else Language = AppLanguage.EN;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Trace(e.Exception.ToString());
            _logger.Error("Dispatcher unhandled exception: {0}", e.Exception.Message);
            e.Handled = true;
        }

        public bool AutoRun
        {
            get
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                    {
                        var command = key?.GetValue(AppName) as string;
                        return string.Equals(command, AutoRunCommand, StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to query autorun registry key, {0}", ex.Message);
                    return false;
                }
            }
            set
            {
                try
                {
                    if (value)
                    {
                        // 写入自启命令
                        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                        {
                            key?.SetValue(AppName, AutoRunCommand);
                        }
                    }
                    else
                    {
                        // 删除自启命令
                        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                        {
                            key?.DeleteValue(AppName, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to set autorun registry key, {0}", ex.Message);
                }
            }
        }

        public bool AssociateFileSuffix
        {
            get
            {
                try
                {
                    // 检查 .skel 的 ProgID
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.skel"))
                    {
                        var progIdValue = key?.GetValue("") as string;
                        if (!string.Equals(progIdValue, App.ProgId, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }

                    // 检查 command 指令是否相同
                    using (var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{App.ProgId}\shell\open\command"))
                    {
                        var command = key?.GetValue("") as string;
                        if (string.IsNullOrWhiteSpace(command))
                            return false;
                        return command == ShellOpenCommand;
                    }
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    // 文件关联
                    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.skel"))
                    {
                        key?.SetValue("", App.ProgId);
                    }

                    using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{App.ProgId}"))
                    {
                        key?.SetValue("", SkelFileDescription);
                        using (var iconKey = key?.CreateSubKey("DefaultIcon"))
                        {
                            iconKey?.SetValue("", $"\"{SkelIconFilePath}\"");
                        }
                        using (var shellKey = key?.CreateSubKey(@"shell\open\command"))
                        {
                            shellKey?.SetValue("", ShellOpenCommand);
                        }
                    }
                }
                else
                {
                    // 删除关联
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.skel", false);
                    Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{App.ProgId}", false);
                }

                Shell32.NotifyAssociationChanged();
            }
        }

        /// <summary>
        /// 程序语言
        /// </summary>
        public AppLanguage Language
        {
            get => _language;
            set
            {
                var uri = $"Resources/Strings/{value.ToString().ToLower()}.xaml";
                try
                {
                    Resources.MergedDictionaries.Add(new() { Source = new(uri, UriKind.Relative) });
                    _language = value;
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to switch language to {0}, {1}", value, ex.Message);
                }
            }
        }
        private AppLanguage _language = AppLanguage.ZH;

        public AppSkin Skin
        {
            get => _skin;
            set
            {
                var uri = $"Resources/Skins/{value.ToString().ToLower()}.xaml";
                try
                {
                    Resources.MergedDictionaries.Add(new() { Source = new(uri, UriKind.Relative) });
                    Resources.MergedDictionaries.Add(new() { Source = new("Resources/Theme.xaml", UriKind.Relative) });
                    var hwnd = new WindowInteropHelper(Current.MainWindow).Handle;
                    Dwmapi.SetWindowTextColor(hwnd, AppResource.Color_PrimaryText);
                    Dwmapi.SetWindowCaptionColor(hwnd, AppResource.Color_Region);
                    _skin = value;
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to switch skin to {0}, {1}", value, ex.Message);
                }
            }
        }
        private AppSkin _skin = AppSkin.Light;
    }

    public enum AppLanguage
    {
        ZH,
        EN,
        JA
    }

    public enum AppSkin
    {
        Light,
        Dark,
        Violet,
    }
}
