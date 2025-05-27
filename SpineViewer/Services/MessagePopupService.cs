using SpineViewer.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Services
{
    public static class MessagePopupService
    {
        public static void Info(string text, string? title = null)
        {
            title ??= AppResource.Str_InfoPopup;
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Warn(string text, string? title = null)
        {
            title ??= AppResource.Str_WarnPopup;
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void Error(string text, string? title = null)
        {
            title ??= AppResource.Str_ErrorPopup;
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool Quest(string text, string? title = null)
        {
            title ??= AppResource.Str_QuestPopup;
            return MessageBox.Show(text, title, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;
        }
    }
}
