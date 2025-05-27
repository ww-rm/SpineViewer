using NLog;
using SpineViewer.Views;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;

namespace SpineViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string Version => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    private static readonly Logger _logger;

    static App()
    {
        InitializeLogConfiguration();
        _logger = LogManager.GetCurrentClassLogger();
        _logger.Info("Application Started");

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
            Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} - ${level:uppercase=true} - ${callsite-filename:includeSourcePath=false}:${callsite-linenumber} - ${message}"
        };

        config.AddTarget(fileTarget);
        config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);
        LogManager.Configuration = config;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dict = new ResourceDictionary();

        var uiCulture = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
        _logger.Info("Current UI Culture: {0}", uiCulture);

        if (uiCulture.StartsWith("zh"))
        {
            ; // 默认就是中文, 无需操作
        }
        else if (uiCulture.StartsWith("ja"))
        {
            dict.Source = new("Resources/Strings/ja-jp.xaml", UriKind.Relative);
        }
        else
        {
            dict.Source = new("Resources/Strings/en-us.xaml", UriKind.Relative);
        }

        Resources.MergedDictionaries.Add(dict);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.Trace(e.Exception.ToString());
        _logger.Error("Dispatcher unhandled exception: {0}", e.Exception.Message);
        e.Handled = true;
    }
}

