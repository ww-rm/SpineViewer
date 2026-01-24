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
        private static T Get<T>(string key) => (T)App.Current.FindResource(key);

        #region Colors

        public static Color Color_LightPrimary => Get<Color>("LightPrimaryColor");
        public static Color Color_Primary => Get<Color>("PrimaryColor");
        public static Color Color_DarkPrimary => Get<Color>("DarkPrimaryColor");

        public static Color Color_LightDanger => Get<Color>("LightDangerColor");
        public static Color Color_Danger => Get<Color>("DangerColor");
        public static Color Color_DarkDanger => Get<Color>("DarkDangerColor");

        public static Color Color_LightWarning => Get<Color>("LightWarningColor");
        public static Color Color_Warning => Get<Color>("WarningColor");
        public static Color Color_DarkWarning => Get<Color>("DarkWarningColor");

        public static Color Color_LightInfo => Get<Color>("LightInfoColor");
        public static Color Color_Info => Get<Color>("InfoColor");
        public static Color Color_DarkInfo => Get<Color>("DarkInfoColor");

        public static Color Color_LightSuccess => Get<Color>("LightSuccessColor");
        public static Color Color_Success => Get<Color>("SuccessColor");
        public static Color Color_DarkSuccess => Get<Color>("DarkSuccessColor");

        public static Color Color_PrimaryText => Get<Color>("PrimaryTextColor");
        public static Color Color_SecondaryText => Get<Color>("SecondaryTextColor");
        public static Color Color_ThirdlyText => Get<Color>("ThirdlyTextColor");
        public static Color Color_ReverseText => Get<Color>("ReverseTextColor");
        public static Color Color_TextIcon => Get<Color>("TextIconColor");

        public static Color Color_Border => Get<Color>("BorderColor");
        public static Color Color_SecondaryBorder => Get<Color>("SecondaryBorderColor");
        public static Color Color_Background => Get<Color>("BackgroundColor");
        public static Color Color_Region => Get<Color>("RegionColor");
        public static Color Color_SecondaryRegion => Get<Color>("SecondaryRegionColor");
        public static Color Color_ThirdlyRegion => Get<Color>("ThirdlyRegionColor");
        public static Color Color_Title => Get<Color>("TitleColor");
        public static Color Color_SecondaryTitle => Get<Color>("SecondaryTitleColor");

        public static Color Color_Default => Get<Color>("DefaultColor");
        public static Color Color_DarkDefault => Get<Color>("DarkDefaultColor");

        public static Color Color_Accent => Get<Color>("AccentColor");
        public static Color Color_DarkAccent => Get<Color>("DarkAccentColor");

        public static Color Color_DarkMask => Get<Color>("DarkMaskColor");
        public static Color Color_DarkOpacity => Get<Color>("DarkOpacityColor");

        #endregion

        #region Brushes

        public static SolidColorBrush Brush_LightPrimary => Get<SolidColorBrush>("LightPrimaryBrush");
        public static LinearGradientBrush Brush_Primary => Get<LinearGradientBrush>("PrimaryBrush");
        public static SolidColorBrush Brush_DarkPrimary => Get<SolidColorBrush>("DarkPrimaryBrush");

        public static SolidColorBrush Brush_PrimaryText => Get<SolidColorBrush>("PrimaryTextBrush");
        public static SolidColorBrush Brush_SecondaryText => Get<SolidColorBrush>("SecondaryTextBrush");
        public static SolidColorBrush Brush_ThirdlyText => Get<SolidColorBrush>("ThirdlyTextBrush");
        public static SolidColorBrush Brush_ReverseText => Get<SolidColorBrush>("ReverseTextBrush");
        public static SolidColorBrush Brush_TextIcon => Get<SolidColorBrush>("TextIconBrush");

        public static SolidColorBrush Brush_Border => Get<SolidColorBrush>("BorderBrush");
        public static SolidColorBrush Brush_SecondaryBorder => Get<SolidColorBrush>("SecondaryBorderBrush");
        public static SolidColorBrush Brush_Background => Get<SolidColorBrush>("BackgroundBrush");
        public static SolidColorBrush Brush_Region => Get<SolidColorBrush>("RegionBrush");
        public static SolidColorBrush Brush_SecondaryRegion => Get<SolidColorBrush>("SecondaryRegionBrush");
        public static SolidColorBrush Brush_ThirdlyRegion => Get<SolidColorBrush>("ThirdlyRegionBrush");
        public static LinearGradientBrush Brush_Title => Get<LinearGradientBrush>("TitleBrush");

        public static SolidColorBrush Brush_Default => Get<SolidColorBrush>("DefaultBrush");
        public static SolidColorBrush Brush_DarkDefault => Get<SolidColorBrush>("DarkDefaultBrush");

        public static SolidColorBrush Brush_LightDanger => Get<SolidColorBrush>("LightDangerBrush");
        public static LinearGradientBrush Brush_Danger => Get<LinearGradientBrush>("DangerBrush");
        public static SolidColorBrush Brush_DarkDanger => Get<SolidColorBrush>("DarkDangerBrush");

        public static SolidColorBrush Brush_LightWarning => Get<SolidColorBrush>("LightWarningBrush");
        public static LinearGradientBrush Brush_Warning => Get<LinearGradientBrush>("WarningBrush");
        public static SolidColorBrush Brush_DarkWarning => Get<SolidColorBrush>("DarkWarningBrush");

        public static SolidColorBrush Brush_LightInfo => Get<SolidColorBrush>("LightInfoBrush");
        public static LinearGradientBrush Brush_Info => Get<LinearGradientBrush>("InfoBrush");
        public static SolidColorBrush Brush_DarkInfo => Get<SolidColorBrush>("DarkInfoBrush");

        public static SolidColorBrush Brush_LightSuccess => Get<SolidColorBrush>("LightSuccessBrush");
        public static LinearGradientBrush Brush_Success => Get<LinearGradientBrush>("SuccessBrush");
        public static SolidColorBrush Brush_DarkSuccess => Get<SolidColorBrush>("DarkSuccessBrush");

        public static SolidColorBrush Brush_Accent => Get<SolidColorBrush>("AccentBrush");
        public static SolidColorBrush Brush_DarkAccent => Get<SolidColorBrush>("DarkAccentBrush");

        public static SolidColorBrush Brush_DarkMask => Get<SolidColorBrush>("DarkMaskBrush");
        public static SolidColorBrush Brush_DarkOpacity => Get<SolidColorBrush>("DarkOpacityBrush");

        #endregion

        #region Strings

        public static string Str_GeneratePreviewsTitle => Get<string>("Str_GeneratePreviewsTitle");
        public static string Str_DeletePreviewsTitle => Get<string>("Str_DeletePreviewsTitle");
        public static string Str_AddSpineObjectsTitle => Get<string>("Str_AddSpineObjectsTitle");
        public static string Str_OpenSkelFileTitle => Get<string>("Str_OpenSkelFileTitle");
        public static string Str_OpenAtlasFileTitle => Get<string>("Str_OpenAtlasFileTitle");
        public static string Str_ReloadSpineObjectsTitle => Get<string>("Str_ReloadSpineObjectsTitle");
        public static string Str_CustomFFmpegExporterTitle => Get<string>("Str_CustomFFmpegExporterTitle");

        public static string Str_InfoPopup => Get<string>("Str_InfoPopup");
        public static string Str_WarnPopup => Get<string>("Str_WarnPopup");
        public static string Str_ErrorPopup => Get<string>("Str_ErrorPopup");
        public static string Str_QuestPopup => Get<string>("Str_QuestPopup");

        public static string Str_CancelQuest => Get<string>("Str_CancelQuest");
        public static string Str_TooManyItemsToAddQuest => Get<string>("Str_TooManyItemsToAddQuest");
        public static string Str_RemoveItemsQuest => Get<string>("Str_RemoveItemsQuest");
        public static string Str_DeleteItemsQuest => Get<string>("Str_DeleteItemsQuest");
        public static string Str_CloseToTrayQuest => Get<string>("Str_CloseToTrayQuest");

        public static string Str_FrameExporterTitle => Get<string>("Str_FrameExporterTitle");
        public static string Str_PsdExporterTitle => Get<string>("Str_PsdExporterTitle");
        public static string Str_FrameSequenceExporterTitle => Get<string>("Str_FrameSequenceExporterTitle");
        public static string Str_FFmpegVideoExporterTitle => Get<string>("Str_FFmpegVideoExporterTitle");

        public static string Str_InvalidOutputDir => Get<string>("Str_InvalidOutputDir");
        public static string Str_OutputDirNotFound => Get<string>("Str_OutputDirNotFound");
        public static string Str_OutputDirRequired => Get<string>("Str_OutputDirRequired");
        public static string Str_InvalidMaxResolution => Get<string>("Str_InvalidMaxResolution");
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
