using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpineViewer.Resources
{
    /// <summary>
    /// 请勿缓存该类下的资源, 都是动态获取的
    /// </summary>
    public static class AppResource
    {
        public static T Get<T>(string key) => (T)App.Current.FindResource(key);

        #region Strings

        public static string Str_GeneratePreviewsTitle => Get<string>("Str_GeneratePreviewsTitle");
        public static string Str_DeletePreviewsTitle => Get<string>("Str_DeletePreviewsTitle");
        public static string Str_AddSpineObjectsTitle => Get<string>("Str_AddSpineObjectsTitle");
        public static string Str_CustomFFmpegExporterTitle => Get<string>("Str_CustomFFmpegExporterTitle");

        public static string Str_InfoPopup => Get<string>("Str_InfoPopup");
        public static string Str_WarnPopup => Get<string>("Str_WarnPopup");
        public static string Str_ErrorPopup => Get<string>("Str_ErrorPopup");
        public static string Str_QuestPopup => Get<string>("Str_QuestPopup");

        public static string Str_CancelQuest => Get<string>("Str_CancelQuest");
        public static string Str_TooManyItemsToAddQuest => Get<string>("Str_TooManyItemsToAddQuest");
        public static string Str_RemoveItemsQuest => Get<string>("Str_RemoveItemsQuest");
        public static string Str_DeleteItemsQuest => Get<string>("Str_DeleteItemsQuest");

        public static string Str_FrameExporterTitle => Get<string>("Str_FrameExporterTitle");
        public static string Str_FrameSequenceExporterTitle => Get<string>("Str_FrameSequenceExporterTitle");
        public static string Str_FFmpegVideoExporterTitle => Get<string>("Str_FFmpegVideoExporterTitle");

        public static string Str_InvalidOutputDir => Get<string>("Str_InvalidOutputDir");
        public static string Str_OutputDirNotFound => Get<string>("Str_OutputDirNotFound");
        public static string Str_OutputDirRequired => Get<string>("Str_OutputDirRequired");
        public static string Str_InvalidMaxResolution => Get<string>("Str_InvalidMaxResolution");
        public static string Str_InvalidDuration => Get<string>("Str_InvalidDuration");
        public static string Str_FFmpegFormatRequired => Get<string>("Str_FFmpegFormatRequired");

        public static string Str_Copied => Get<string>("Str_Copied");

        #endregion

        #region Geometries

        public static Geometry Geo_Ban => Get<Geometry>("Geo_Ban");
        public static Geometry Geo_TrashXmark => Get<Geometry>("Geo_TrashXmark");
        public static Geometry Geo_ArrowsMaximize => Get<Geometry>("Geo_ArrowsMaximize");
        public static Geometry Geo_ArrowsMinimize => Get<Geometry>("Geo_ArrowsMinimize");
        public static Geometry Geo_ForwardFast => Get<Geometry>("Geo_ForwardFast");
        public static Geometry Geo_ForwardStep => Get<Geometry>("Geo_ForwardStep");
        public static Geometry Geo_Pause => Get<Geometry>("Geo_Pause");
        public static Geometry Geo_RotateLeft => Get<Geometry>("Geo_RotateLeft");
        public static Geometry Geo_Play => Get<Geometry>("Geo_Play");
        public static Geometry Geo_Stop => Get<Geometry>("Geo_Stop");
        public static Geometry Geo_Folder => Get<Geometry>("Geo_Folder");
        public static Geometry Geo_ArrowRotateRight => Get<Geometry>("Geo_ArrowRotateRight");

        #endregion
    }
}
