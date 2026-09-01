using Spine;
using Spine.Exporters;
using Spine.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpineViewer.Resources
{
    public static class EnumValues
    {
        public static IEnumerable Enum_AppLanguage { get; } = Enum.GetValues<AppLanguage>();
        public static IEnumerable Enum_AppSkin { get; } = Enum.GetValues<AppSkin>();
        public static IEnumerable Enum_Stretch { get; } = Enum.GetValues<Stretch>();
        public static IEnumerable Enum_Physics { get; } = Enum.GetValues<ISkeleton.Physics>();
        public static IEnumerable Enum_HitTestLevel { get; } = Enum.GetValues<HitTestLevel>();

        public static IEnumerable Enum_VideoFormat { get; } = Enum.GetValues<FFmpegVideoExporter.VideoFormat>();
        public static IEnumerable Enum_ApngPredMethod { get; } = Enum.GetValues<FFmpegVideoExporter.ApngPredMethod>();
        public static IEnumerable Enum_MovProfile { get; } = Enum.GetValues<FFmpegVideoExporter.MovProfile>();
    }
}
