using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Win32Natives;

namespace SpineViewer.Utils
{
    public static class WorkerWDebugger
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private class WndNode
        {
            public IntPtr Hwnd;
            public string ClassName = "";
            public List<WndNode> Children = [];
            public WndNode? Parent;
        }

        private static readonly HashSet<string> TargetClasses = [
            "Progman",
            "WorkerW",
            "SHELLDLL_DefView"
        ];

        public static void LogWorkerWSearchInfo()
        {
            _logger.Debug("========== Begin outputting WorkerW debug information ==========");

            var progman = User32.FindWindow("Progman", null);
            if (progman == IntPtr.Zero)
            {
                _logger.Debug("Failed to find Progman");
                return;
            }
            _logger.Debug("HWND(Progman): 0x{0:x8}", progman);


            // Send 0x052C to Progman. This message directs Progman to spawn a 
            // WorkerW behind the desktop icons. If it is already there, nothing 
            // happens.
            Marshal.SetLastPInvokeError(0);
            var ret = User32.SendMessageTimeout(progman, User32.WM_SPAWN_WORKER, 0, 0, User32.SMTO_NORMAL, 1000, out _);
            var lastErr = Marshal.GetLastPInvokeError();
            var lastErrMsg = Marshal.GetLastPInvokeErrorMessage();

            _logger.Debug("SendMessageTimeout returned 0x{0:x8}", ret);
            _logger.Debug("ErrCode: 0x{0:x8}, ErrMsg: {1}", lastErr, lastErrMsg);

            // Spy++ output
            // .....
            // 0x00010190 "" WorkerW
            //   ...
            //   0x000100EE "" SHELLDLL_DefView
            //     0x000100F0 "FolderView" SysListView32
            // 0x00100B8A "" WorkerW       <-- This is the WorkerW instance we are after!
            // 0x000100EC "Program Manager" Progman
            var workerw = IntPtr.Zero;

            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            User32.EnumWindows(new User32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr shellDefView = User32.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (shellDefView != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one.
                    _logger.Debug("SHELLDLL_DefView found in window 0x{0:x8}", tophandle);
                    _logger.Debug("HWND(SHELLDLL_DefView): 0x{0:x8}", shellDefView);
                    workerw = User32.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);

                    // Stop enumeration.
                    return false;
                }

                return true;
            }), IntPtr.Zero);

            // Some Windows 11 builds have a different Progman window layout.
            // If the above code failed to find WorkerW, we should try this.
            // Spy++ output
            // 0x000100EC "Program Manager" Progman
            //   0x000100EE "" SHELLDLL_DefView
            //     0x000100F0 "FolderView" SysListView32
            //   0x00100B8A "" WorkerW       <-- This is the WorkerW instance we are after!
            if (workerw == IntPtr.Zero)
            {
                _logger.Debug("Try to find WorkerW in progman directly");
                workerw = User32.FindWindowEx(progman, IntPtr.Zero, "WorkerW", null);
            }

            _logger.Debug("HWND(WorkerW): 0x{0:x8}", workerw);

            _logger.Debug("========== End outputting WorkerW debug information ==========");
        }

        public static void LogWorkerWWindowTree()
        {
            _logger.Debug("========== Begin outputting WorkerW window tree ==========");

            // 1. 构建完整窗口树
            var roots = BuildWindowTree();

            // 2. 查找所有命中节点并输出
            foreach (var root in roots)
            {
                if (HasTargetWindow(root))
                    PrintFullTree(root, 0);
            }

            _logger.Debug("========== End outputting WorkerW tree window tree ==========");
        }

        private static List<WndNode> BuildWindowTree()
        {
            Dictionary<IntPtr, WndNode> map = [];

            // 枚举顶级窗口（roots）
            var roots = new List<WndNode>();

            User32.EnumWindows((hwnd, lParam) =>
            {
                var node = GetOrCreate(map, hwnd);
                roots.Add(node);
                BuildSubTree(node, map);
                return true;
            }, IntPtr.Zero);

            return roots;
        }

        private static void BuildSubTree(WndNode parent, Dictionary<IntPtr, WndNode> map)
        {
            foreach (var child in User32.EnumDirectChildWindow(parent.Hwnd))
            {
                var node = GetOrCreate(map, child);
                node.Parent = parent;
                parent.Children.Add(node);
                BuildSubTree(node, map);
            }
        }

        private static WndNode GetOrCreate(Dictionary<IntPtr, WndNode> map, IntPtr hwnd)
        {
            if (map.TryGetValue(hwnd, out var n))
                return n;

            var node = new WndNode
            {
                Hwnd = hwnd,
                ClassName = User32.GetWindowClassName(hwnd)
            };
            map[hwnd] = node;
            return node;
        }

        private static bool HasTargetWindow(WndNode root)
        {
            if (TargetClasses.Contains(root.ClassName))
                return true;
            foreach (var child in root.Children)
            {
                if (HasTargetWindow(child))
                    return true;
            }
            return false;
        }

        private static void PrintFullTree(WndNode node, int depth)
        {
            string prefix = new(' ', depth * 4);
            _logger.Debug($"{prefix}0x{node.Hwnd.ToInt64():x8}({node.ClassName})");

            foreach (var child in node.Children)
                PrintFullTree(child, depth + 1);
        }
    }
}
