using NLog;
using SpineViewer.Utils;
using SpineViewer.Utils.Localize;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace SpineViewer
{
	internal static class Program
    {
        ///// <summary>
        ///// 程序路径
        ///// </summary>
        //public static readonly string FilePath = Environment.ProcessPath;

        ///// <summary>
        ///// 程序名
        ///// </summary>
        //public static readonly string Name = Path.GetFileNameWithoutExtension(FilePath);

        ///// <summary>
        ///// 程序目录
        ///// </summary>
        //public static readonly string RootDir = Path.GetDirectoryName(FilePath);

        ///// <summary>
        ///// 程序临时目录
        ///// </summary>
        //public static readonly string TempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Name)).FullName;

        public static string Version => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        /// <summary>
        /// 程序日志器
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 应用入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 此处先初始化全局配置再触发静态字段 Logger 引用构造, 才能将配置应用到新的日志器上
            InitializeLogConfiguration();
            logger.Info("Program Started");            

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
            LocalizeConfiguration.SetCulture();

			try
            {
                Application.Run(new SpineViewerForm() { Text = $"SpineViewer - v{Version}"});
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                MessagePopup.Error(ex.ToString(), "程序已崩溃", Properties.Resources.msgBoxError);
            }
        }

        /// <summary>
        /// 初始化日志配置
        /// </summary>
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
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
            LogManager.Configuration = config;
        }
    }
}