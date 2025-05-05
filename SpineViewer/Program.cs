using NLog;
using SpineViewer.Spine;
using SpineViewer.Spine.Implementations.AltasConvertor;
using SpineViewer.Spine.Implementations.AtlasConvertor;
using SpineViewer.Spine.Implementations.SkeletonConverter;
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
            string loadPath = "E:\\desktop1\\work\\NIkkeAssetsUnpack\\failed\\c500_cover_00.json";
            SkeletonConverter38 tmp = new SkeletonConverter38();
            SkeletonConverter42 tm = new SkeletonConverter42();
            var a = tm.ReadJson(loadPath);
            tm.WriteJson(SkeletonConverter.V4XToV38(a, true), "E:\\desktop1\\work\\NIkkeAssetsUnpack\\failed\\c500_cover_01.json");
            //var a = tmp.ReadJson(loadPath);
            //tmp.WriteBinary(a, "E:\\desktop1\\work\\NIkkeAssetsUnpack\\failed\\c500_cover_01.skel");
            //tmp.ReadBinary("E:\\desktop1\\work\\NIkkeAssetsUnpack\\failed\\c500_cover_01.skel");
            //string loadP = "C:\\Users\\plmnb\\Desktop\\Atsuko_swimsuit\\test\\CH0267_home.atlas";
            //AtlasConverter38 tmp = new AtlasConverter38();
            //AtlasConverter4X tm = new AtlasConverter4X();
            //////tm.ToFile("E:\\desktop1\\work\\NIkkeAssetsUnpack\\failed\\c500_cover_02.atlas", tmp.ReadAltas(loadP));
            //tmp.ToFile("C:\\Users\\plmnb\\Desktop\\Atsuko_swimsuit\\test\\1.atlas", tm.ReadAltas(loadP));


            // 此处先初始化全局配置再触发静态字段 Logger 引用构造, 才能将配置应用到新的日志器上
            //InitializeLogConfiguration();
            //logger.Info("Program Started");

            //// To customize application configuration such as set high DPI settings or default font,
            //// see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //LocalizeConfiguration.SetCulture();

            //try
            //{
            //    Application.Run(new SpineViewerForm() { Text = $"SpineViewer - v{Version}" });
            //}
            //catch (Exception ex)
            //{
            //    logger.Fatal(ex.ToString());
            //    MessagePopup.Error(ex.ToString(), Properties.Resources.programCrashed);
            //}
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