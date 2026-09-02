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
    /// 定义了 SFML 渲染器的基本功能和事件, 基本上是对 <see cref="RenderWindow"/> 的抽象
    /// <para>实现示例可以见 <see cref="SFMLRenderPanel"/></para>
    /// </summary>
    public interface ISFMLRenderer
    {
        /// <summary>
        /// 发生在资源首次创建完成后, 该事件发生之后渲染器才是可用的, 操作才会生效
        /// </summary>
        public event EventHandler? RendererCreated;

        /// <summary>
        /// 发生在资源即将不可用之前, 该事件发生之后对渲染器的操作将被忽略
        /// </summary>
        public event EventHandler? RendererDisposing;

        public event EventHandler<MouseMoveEventArgs>? CanvasMouseMoved;
        public event EventHandler<MouseButtonEventArgs>? CanvasMouseButtonPressed;
        public event EventHandler<MouseButtonEventArgs>? CanvasMouseButtonReleased;
        public event EventHandler<MouseWheelScrollEventArgs>? CanvasMouseWheelScrolled;

        /// <summary>
        /// 分辨率, 影响画面的相对比例
        /// </summary>
        public Vector2u Resolution { get; set; }

        /// <summary>
        /// 快捷设置视区中心点
        /// </summary>
        public Vector2f Center { get; set; }

        /// <summary>
        /// 快捷设置视区缩放
        /// </summary>
        public float Zoom { get; set; }

        /// <summary>
        /// 快捷设置视区旋转
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// 快捷设置视区水平翻转
        /// </summary>
        public bool FlipX { get; set; }

        /// <summary>
        /// 快捷设置视区垂直翻转
        /// </summary>
        public bool FlipY { get; set; }

        /// <summary>
        /// 最大帧率, 影响 Draw 的最大调用频率, <see cref="RenderWindow.SetFramerateLimit(uint)"/>
        /// </summary>
        public uint MaxFps { get; set; }

        /// <summary>
        /// 垂直同步, <see cref="RenderWindow.SetVerticalSyncEnabled(bool)"/>
        /// </summary>
        public bool VerticalSync { get; set; }

        /// <inheritdoc cref="RenderWindow.SetActive(bool)"/>
        public bool SetActive(bool active);

        /// <inheritdoc cref="RenderWindow.GetView"/>
        public View GetView();

        /// <inheritdoc cref="RenderWindow.SetView(View)"/>
        public void SetView(View view);

        /// <inheritdoc cref="RenderWindow.MapPixelToCoords(Vector2i)"/>
        public Vector2f MapPixelToCoords(Vector2i point);

        /// <inheritdoc cref="RenderWindow.MapCoordsToPixel(Vector2f)"/>
        public Vector2i MapCoordsToPixel(Vector2f point);

        /// <inheritdoc cref="RenderWindow.Clear()"/>
        public void Clear();

        /// <inheritdoc cref="RenderWindow.Clear(Color)"/>
        public void Clear(Color color);

        /// <inheritdoc cref="RenderWindow.Draw(Drawable)"/>
        public void Draw(Drawable drawable);

        /// <inheritdoc cref="RenderWindow.Draw(Drawable, RenderStates)"/>
        public void Draw(Drawable drawable, RenderStates states);

        /// <inheritdoc cref="RenderWindow.Draw(Vertex[], PrimitiveType)"/>
        public void Draw(Vertex[] vertices, PrimitiveType type);

        /// <inheritdoc cref="RenderWindow.Draw(Vertex[], PrimitiveType, RenderStates)"/>
        public void Draw(Vertex[] vertices, PrimitiveType type, RenderStates states);

        /// <inheritdoc cref="RenderWindow.Draw(Vertex[], uint, uint, PrimitiveType)"/>
        public void Draw(Vertex[] vertices, uint start, uint count, PrimitiveType type);

        /// <inheritdoc cref="RenderWindow.Display"/>
        public void Display();
    }
}
