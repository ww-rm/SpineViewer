using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SFMLRenderer
{
    public class SFMLRenderWindow : RenderWindow, ISFMLRenderer
    {
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(10) };

        public SFMLRenderWindow(VideoMode mode, string title, Styles style) : base(mode, title, style) 
        {
            SetActive(false);
            _timer.Tick += (s, e) => DispatchEvents();
            _timer.Start();

            SetVisible(false);

            var handle = SystemHandle;
            var exStyle = GetWindowLong(handle, GWL_EXSTYLE) | WS_EX_LAYERED;
            SetWindowLong(handle, GWL_EXSTYLE, exStyle);
            SetLayeredWindowAttributes(handle, 0, byte.MaxValue, LWA_ALPHA);

            RendererCreated?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? RendererCreated;

        public event EventHandler? RendererDisposing
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public event EventHandler<MouseMoveEventArgs>? CanvasMouseMoved
        {
            add { MouseMoved += value; }
            remove { MouseMoved -= value; }
        }

        public event EventHandler<MouseButtonEventArgs>? CanvasMouseButtonPressed
        {
            add { MouseButtonPressed += value; }
            remove { MouseButtonPressed -= value; }
        }

        public event EventHandler<MouseButtonEventArgs>? CanvasMouseButtonReleased
        {
            add { MouseButtonReleased += value; }
            remove { MouseButtonReleased -= value; }
        }

        public event EventHandler<MouseWheelScrollEventArgs>? CanvasMouseWheelScrolled
        {
            add { MouseWheelScrolled += value; }
            remove { MouseWheelScrolled -= value; }
        }

        public Vector2u Resolution
        { 
            get => Size; 
            set => Size = value; 
        }

        public Vector2f Center
        {
            get
            {
                using var view = GetView();
                return view.Center;
            }
            set
            {
                using var view = GetView();
                view.Center = value;
                SetView(view);
            }
        }

        public float Zoom
        {
            get
            {
                using var view = GetView();
                return Math.Abs(Size.X / view.Size.X); // XXX: 仅使用宽度进行缩放计算
            }
            set
            {
                value = Math.Abs(value);
                if (value <= 0) return;
                using var view = GetView();
                var signX = Math.Sign(view.Size.X);
                var signY = Math.Sign(view.Size.Y);
                var resolution = Size;
                view.Size = new(resolution.X / value * signX, resolution.Y / value * signY);
                SetView(view);
            }
        }

        public float Rotation
        {
            get
            {
                using var view = GetView();
                return view.Rotation;
            }
            set
            {
                using var view = GetView();
                view.Rotation = value;
                SetView(view);
            }
        }

        public bool FlipX
        {
            get
            {
                using var view = GetView();
                return view.Size.X < 0;
            }
            set
            {
                using var view = GetView();
                var size = view.Size;
                if (size.X > 0 && value || size.X < 0 && !value)
                    size.X *= -1;
                view.Size = size;
                SetView(view);
            }
        }

        public bool FlipY
        {
            get
            {
                using var view = GetView();
                return view.Size.Y < 0;
            }
            set
            {
                using var view = GetView();
                var size = view.Size;
                if (size.Y > 0 && value || size.Y < 0 && !value)
                    size.Y *= -1;
                view.Size = size;
                SetView(view);
            }
        }

        public uint MaxFps
        {
            get => _maxFps;
            set
            {
                SetFramerateLimit(value);
                _maxFps = value;
            }
        }
        private uint _maxFps = 0;

        public bool VerticalSync
        {
            get => _verticalSync;
            set
            {
                SetVerticalSyncEnabled(value);
                _verticalSync = value;
            }
        }
        private bool _verticalSync = false;

        #region Win32 Native APIs

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x00080000;
        private const uint LWA_ALPHA = 0x2;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint pcrKey, byte pbAlpha, uint pdwFlags);

        #endregion
    }
}
