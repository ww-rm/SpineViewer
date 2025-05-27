using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLRenderer
{
    /// <summary>
    /// SFMLRenderPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SFMLRenderPanel : System.Windows.Controls.UserControl, ISFMLRenderer
    {
        private RenderWindow? RenderWindow => _hwndHost.RenderWindow;

        public SFMLRenderPanel()
        {
            InitializeComponent();
        }

        public event EventHandler? RendererCreated
        {
            add => _hwndHost.RenderWindowBuilded += value;
            remove => _hwndHost.RenderWindowBuilded -= value;
        }

        public event EventHandler? RendererDisposing
        {
            add => _hwndHost.RenderWindowDestroying += value;
            remove => _hwndHost.RenderWindowDestroying -= value;
        }

        public event EventHandler<MouseMoveEventArgs>? CanvasMouseMove 
        {
            add { if (RenderWindow is RenderWindow w) w.MouseMoved += value; }
            remove { if (RenderWindow is RenderWindow w) w.MouseMoved -= value; }
        }

        public event EventHandler<MouseButtonEventArgs>? CanvasMouseButtonPressed
        {
            add { if (RenderWindow is RenderWindow w) w.MouseButtonPressed += value; }
            remove { if (RenderWindow is RenderWindow w) w.MouseButtonPressed -= value; }
        }

        public event EventHandler<MouseButtonEventArgs>? CanvasMouseButtonReleased
        {
            add { if (RenderWindow is RenderWindow w) w.MouseButtonReleased += value; }
            remove { if (RenderWindow is RenderWindow w) w.MouseButtonReleased -= value; }
        }

        public event EventHandler<MouseWheelScrollEventArgs>? CanvasMouseWheelScrolled
        {
            add { if (RenderWindow is RenderWindow w) w.MouseWheelScrolled += value; }
            remove { if (RenderWindow is RenderWindow w) w.MouseWheelScrolled -= value; }
        }

        public Vector2u Resolution
        {
            get => _resolution;
            set
            {
                if (RenderWindow is null) return;
                if (value == _resolution) return;
                if (value.X <= 0 || value.Y <= 0) return;

                var zoom = Zoom;

                float parentW = (float)ActualWidth;
                float parentH = (float)ActualHeight;
                float renderW = value.X;
                float renderH = value.Y;
                float scale = Math.Min(parentW / renderW, parentH / renderH); // 两方向取较小值, 保证 parent 覆盖 render
                renderW *= scale;
                renderH *= scale;

                _hwndHost.Width = renderW;
                _hwndHost.Height = renderH;

                _resolution = value;

                // 设置完 resolution 后还原缩放比例
                Zoom = zoom;
            }
        }
        private Vector2u _resolution = new(100, 100);

        public Vector2f Center 
        { 
            get
            {
                if (RenderWindow is null) return default;
                using var view = RenderWindow.GetView();
                return view.Center;
            }
            set
            {
                if (RenderWindow is null) return;
                using var view = RenderWindow.GetView();
                view.Center = value;
                RenderWindow.SetView(view);
            }
        }

        public float Zoom
        {
            get
            {
                if (RenderWindow is null) return 1;
                using var view = RenderWindow.GetView();
                return Math.Abs(_resolution.X / view.Size.X); // XXX: 仅使用宽度进行缩放计算
            }
            set
            {
                value = Math.Abs(value);
                if (RenderWindow is null || value <= 0) return;
                using var view = RenderWindow.GetView();
                var signX = Math.Sign(view.Size.X);
                var signY = Math.Sign(view.Size.Y);
                view.Size = new(_resolution.X / value * signX, _resolution.Y / value * signY);
                RenderWindow.SetView(view);
            }
        }

        public float Rotation 
        {
            get
            {
                if (RenderWindow is null) return default;
                using var view = RenderWindow.GetView();
                return view.Rotation;
            }
            set
            {
                if (RenderWindow is null) return;
                using var view = RenderWindow.GetView();
                view.Rotation = value;
                RenderWindow.SetView(view);
            }
        }

        public bool FlipX 
        { 
            get
            {
                if (RenderWindow is null) return false;
                using var view = RenderWindow.GetView();
                return view.Size.X < 0;
            }
            set
            {
                if (RenderWindow is null) return;

                using var view = RenderWindow.GetView();
                var size = view.Size;
                if (size.X > 0 && value || size.X < 0 && !value)
                    size.X *= -1;
                view.Size = size;
                RenderWindow.SetView(view);
            }
        }

        public bool FlipY 
        {
            get
            {
                if (RenderWindow is null) return false;
                using var view = RenderWindow.GetView();
                return view.Size.Y < 0;
            }
            set
            {
                if (RenderWindow is null) return;

                using var view = RenderWindow.GetView();
                var size = view.Size;
                if (size.Y > 0 && value || size.Y < 0 && !value)
                    size.Y *= -1;
                view.Size = size;
                RenderWindow.SetView(view);
            }
        }

        public uint MaxFps 
        {
            get => _maxFps;
            set 
            {
                if (RenderWindow is null) return;
                RenderWindow.SetFramerateLimit(value);
                _maxFps = value;
            }
        }
        private uint _maxFps = 0;

        public bool VerticalSync 
        { 
            get => _verticalSync; 
            set
            {
                if (RenderWindow is null) return;
                RenderWindow.SetVerticalSyncEnabled(value);
                _verticalSync = value;
            }
        }
        private bool _verticalSync = false;

        public void Clear() => RenderWindow?.Clear();

        public void Clear(Color color) => RenderWindow?.Clear(color);

        public void Display() => RenderWindow?.Display();

        public void Draw(Drawable drawable) => RenderWindow?.Draw(drawable);

        public void Draw(Drawable drawable, RenderStates states) => RenderWindow?.Draw(drawable, states);

        public void Draw(Vertex[] vertices, PrimitiveType type) => RenderWindow?.Draw(vertices, type);

        public void Draw(Vertex[] vertices, PrimitiveType type, RenderStates states) => RenderWindow?.Draw(vertices, type, states);

        public void Draw(Vertex[] vertices, uint start, uint count, PrimitiveType type) => RenderWindow?.Draw(vertices, start, count, type);

        public View GetView() => RenderWindow?.GetView() ?? new();

        public Vector2i MapCoordsToPixel(Vector2f point) => RenderWindow?.MapCoordsToPixel(point) ?? default;

        public Vector2f MapPixelToCoords(Vector2i point) => RenderWindow?.MapPixelToCoords(point) ?? default;

        public bool SetActive(bool active) => RenderWindow?.SetActive(active) ?? false;

        public void SetView(View view) => RenderWindow?.SetView(view);

        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (RenderWindow is null) return;
            float parentW = (float)sizeInfo.NewSize.Width;
            float parentH = (float)sizeInfo.NewSize.Height;
            float renderW = (float)_hwndHost.ActualWidth;
            float renderH = (float)_hwndHost.ActualHeight;
            float scale = Math.Min(parentW / renderW, parentH / renderH); // 两方向取较小值, 保证 parent 覆盖 render
            renderW *= scale;
            renderH *= scale;

            _hwndHost.Width = renderW;
            _hwndHost.Height = renderH;
        }
    }
}
