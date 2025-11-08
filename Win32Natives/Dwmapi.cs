using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Win32Natives
{
    /// <summary>
    /// dwmapi.dll 包装类
    /// </summary>
    public static class Dwmapi
    {
        private const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const uint DWMWA_CAPTION_COLOR = 35;
        private const uint DWMWA_TEXT_COLOR = 36;
        private const uint DWMWA_COLOR_DEFAULT = 0xFFFFFFFF;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint dwAttribute, ref uint pvAttribute, int cbAttribute);

        public static bool SetWindowCaptionColor(IntPtr hwnd, byte r, byte g, byte b)
        {
            int c = r | (g << 8) | (b << 16);
            return 0 == DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref c, sizeof(uint));
        }

        public static bool SetWindowTextColor(IntPtr hwnd, byte r, byte g, byte b)
        {
            int c = r | (g << 8) | (b << 16);
            return 0 == DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref c, sizeof(uint));
        }

        public static bool SetWindowDarkMode(IntPtr hwnd, bool darkMode)
        {
            int b = darkMode ? 1 : 0;
            uint c = DWMWA_COLOR_DEFAULT;
            DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref c, sizeof(uint));
            DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref c, sizeof(uint));
            return 0 == DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref b, sizeof(int));
        }
    }
}
