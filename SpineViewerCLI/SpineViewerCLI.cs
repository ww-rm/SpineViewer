using NLog;
using SkiaSharp;
using Spectre.Console;
using Spine;
using Spine.Exporters;
using System.CommandLine;
using System.Globalization;

namespace SpineViewerCLI
{
    public static class SpineViewerCLI
    {
        public static Option<bool> OptQuiet { get; } = new("--quiet", "-q")
        {
            Description = "Suppress console logging (quiet mode).",
            Recursive = true,
        };

        public static int Main(string[] args)
        {
            InitializeFileLog();

            var cmdRoot = new RootCommand("Root Command")
            {
                OptQuiet,
                new QueryCommand(),
                new PreviewCommand(),
                new ExportCommand(),
            };

            var result = cmdRoot.Parse(args);

            if (!result.GetValue(OptQuiet))
                InitializeConsoleLog();

            return result.Invoke();
        }

        private static void InitializeFileLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var fileTarget = new NLog.Targets.FileTarget("fileTarget")
            {
                Encoding = System.Text.Encoding.UTF8,
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} - ${level:uppercase=true} - ${processid} - ${callsite-filename:includeSourcePath=false}:${callsite-linenumber} - ${message}",
                AutoFlush = true,
                CreateDirs = true,
                FileName = "${basedir}/logs/cli.log",
                ArchiveFileName = "${basedir}/logs/cli.{#}.log",
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Rolling,
                ArchiveAboveSize = 1048576,
                MaxArchiveFiles = 5,
                ConcurrentWrites = true,
                KeepFileOpen = false,
            };

            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);
            LogManager.Configuration = config;
        }

        private static void InitializeConsoleLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var consoleTarget = new NLog.Targets.ColoredConsoleTarget("consoleTarget")
            {
                Encoding = System.Text.Encoding.UTF8,
                Layout = "[${level:format=OneLetter}]${date:format=yyyy-MM-dd HH\\:mm\\:ss} - ${message}",
                AutoFlush = true,
                DetectConsoleAvailable = true,
                StdErr = true,
                DetectOutputRedirected = true,
            };

            consoleTarget.RowHighlightingRules.Add(new("level == LogLevel.Info", NLog.Targets.ConsoleOutputColor.DarkGray, NLog.Targets.ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new("level == LogLevel.Warn", NLog.Targets.ConsoleOutputColor.DarkYellow, NLog.Targets.ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new("level == LogLevel.Error", NLog.Targets.ConsoleOutputColor.Red, NLog.Targets.ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new("level == LogLevel.Fatal", NLog.Targets.ConsoleOutputColor.White, NLog.Targets.ConsoleOutputColor.DarkRed));

            config.AddTarget(consoleTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
            LogManager.Configuration = config;
        }
    }
}
