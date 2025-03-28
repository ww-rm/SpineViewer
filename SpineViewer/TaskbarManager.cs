using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpineViewer
{
    internal enum TBPFLAG
    {
        TBPF_NOPROGRESS = 0,
        TBPF_INDETERMINATE = 0x1,
        TBPF_NORMAL = 0x2,
        TBPF_ERROR = 0x4,
        TBPF_PAUSED = 0x8
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport, Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
    internal interface ITaskbarList3
    {
        // ITaskbarList
        void HrInit();
        void AddTab(IntPtr hwnd);
        void DeleteTab(IntPtr hwnd);
        void ActivateTab(IntPtr hwnd);
        void SetActiveAlt(IntPtr hwnd);
        // ITaskbarList2
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
        // ITaskbarList3
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        void SetProgressState(IntPtr hwnd, TBPFLAG tbpFlags);
        //void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        //void UnregisterTab(IntPtr hwndTab);
        //void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        //void SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, uint dwReserved);
        //void ThumbBarAddButtons(IntPtr hwnd, uint cButtons, THUMBBUTTON[] pButton);
        //void ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, THUMBBUTTON[] pButton);
        //void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
        //void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, string pszDescription);
        //void SetThumbnailTooltip(IntPtr hwnd, string pszTip);
        //void SetThumbnailClip(IntPtr hwnd, ref RECT prcClip);
    }

    [ComImport, Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
    internal class TaskbarList { }

    internal static class TaskbarManager
    {
        private static readonly ITaskbarList3 taskbarList = (ITaskbarList3)new TaskbarList();

        static TaskbarManager()
        {
            taskbarList.HrInit();
        }

        public static void SetProgressState(IntPtr windowHandle, TBPFLAG state)
        {
            taskbarList.SetProgressState(windowHandle, state);
        }

        public static void SetProgressValue(IntPtr windowHandle, ulong completed, ulong total)
        {
            taskbarList.SetProgressValue(windowHandle, completed, total);
        }
    }
}
