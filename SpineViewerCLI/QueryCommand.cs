using NLog;
using Spine;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewerCLI
{
    public class QueryCommand : Command
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly string _name = "query";
        private static readonly string _desc = "Query information of single model";

        private static readonly string HalfHeader = new('>', 15);
        private static readonly char Separator = '\t';

        public Argument<FileInfo> ArgSkel { get; } = new("skel")
        {
            Description = "Path of skel file.",
        };

        public Option<FileInfo> OptAtlas { get; } = new("--atlas")
        {
            Description = "Path to the atlas file that matches the skel file.",
        };

        public Option<bool> OptAll { get; } = new("--all")
        {
            Description = "Print all information",
        };

        public Option<bool> OptSkin { get; } = new("--skin")
        {
            Description = "Print skins",
        };

        public Option<bool> OptAnimation { get; } = new("--animation")
        {
            Description = "Print animations",
        };

        public Option<bool> OptSlot { get; } = new("--slot")
        {
            Description = "Print slots",
        };

        public QueryCommand() : base(_name, _desc)
        {
            this.AddArgsAndOpts();
            SetAction(QueryAction);
        }

        private void QueryAction(ParseResult result)
        {
            // 读取模型
            using var spine = new SpineObject(result.GetValue(ArgSkel)!.FullName, result.GetValue(OptAtlas)?.FullName);

            var all = result.GetValue(OptAll);

            if (all || result.GetValue(OptSkin))
            {
                SkinRecord[] data = spine.Data.SkinsByName.Keys.Select(v => new SkinRecord(v)).ToArray();
                PrintData("Skins", SkinRecord.Headers, data);
            }
            if (all || result.GetValue(OptAnimation))
            {
                AnimationRecord[] data = spine.Data.Animations.Select(v => new AnimationRecord(v.Name, v.Duration)).ToArray();
                PrintData("Animations", AnimationRecord.Headers, data);
            }
            if (all || result.GetValue(OptSlot))
            {
                SlotRecord[] data = spine.Data.SlotAttachments.Select(v => new SlotRecord(v.Key, v.Value.Keys.ToArray())).ToArray();
                PrintData("Slots", SlotRecord.Headers, data);
            }
        }

        private void PrintData(string dataName, string[] headers, RowRecord[] rows)
        {
            var header = $"{HalfHeader} {dataName} {HalfHeader}";
            var footer = new string('<', header.Length);

            Console.WriteLine(header);
            Console.WriteLine(string.Join(Separator, headers));
            foreach (var row in rows) 
                Console.WriteLine(string.Join(Separator, row.Values));
            Console.WriteLine(footer);
        }

    }

    public abstract record RowRecord
    {
        public abstract object[] Values { get; }
    }

    public record SkinRecord(string Name) : RowRecord
    {
        public static string[] Headers { get; } = [nameof(Name)];

        public override object[] Values => [Name];
    }

    public record AnimationRecord(string Name, float Duration) : RowRecord
    {
        public static string[] Headers { get; } = [nameof(Name), nameof(Duration)];

        public override object[] Values => [Name, Duration];
    }

    public record SlotRecord(string Name, string[] Attachments) : RowRecord
    {
        public static string[] Headers { get; } = [nameof(Name), nameof(Attachments)];

        public override object[] Values => [Name, string.Join(';', Attachments)];
    }
}
