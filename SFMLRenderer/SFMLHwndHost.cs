using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SFMLRenderer
{
    public class SFMLHwndHost : HwndHost
    {
        private HwndSource? _hwndSource;
        private SFML.Graphics.RenderWindow? _renderWindow;

        public SFML.Graphics.RenderWindow? RenderWindow => _renderWindow;

        public event EventHandler? RenderWindowBuilded;
        public event EventHandler? RenderWindowDestroying;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var ps = new HwndSourceParameters(GetType().Name, (int)Width, (int)Height)
            {
                ParentWindow = hwndParent.Handle,
                WindowStyle = 0x40000000 | 0x10000000, // WS_CHILD | WS_VISIBLE
                HwndSourceHook = HwndMessageHook
            };
            _hwndSource = new HwndSource(ps);
            _renderWindow = new(_hwndSource.Handle);
            _renderWindow.SetActive(false);

            RenderWindowBuilded?.Invoke(this, EventArgs.Empty);
            return new HandleRef(this, _hwndSource.Handle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            RenderWindowDestroying?.Invoke(this, EventArgs.Empty);

            _renderWindow?.Close();
            var rw = _renderWindow;
            _renderWindow = null;
            rw?.Dispose();
            var hs = _hwndSource;
            _hwndSource = null;
            hs?.Dispose();
        }

        private nint HwndMessageHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            _renderWindow?.DispatchEvents();
            return nint.Zero;
        }
    }
}
