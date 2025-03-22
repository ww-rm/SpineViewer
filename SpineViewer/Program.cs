using NLog;
using System.Diagnostics;

namespace SpineViewer
{
    internal static class Program
    {
        /// <summary>
        /// 程序路径
        /// </summary>
        public static readonly string FilePath = Environment.ProcessPath;

        /// <summary>
        /// 程序名
        /// </summary>
        public static readonly string Name = Path.GetFileNameWithoutExtension(FilePath);

        /// <summary>
        /// 程序目录
        /// </summary>
        public static readonly string RootDir = Path.GetDirectoryName(FilePath);

        /// <summary>
        /// 程序临时目录
        /// </summary>
        public static readonly string TempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Name)).FullName;

        /// <summary>
        /// 程序进程
        /// </summary>
        public static readonly Process Process = Process.GetCurrentProcess();

        /// <summary>
        /// 程序日志器
        /// </summary>
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 应用入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitializeLogConfiguration();
            Logger.Info("Program Started");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                MessageBox.Show(ex.ToString(), "程序已崩溃", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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