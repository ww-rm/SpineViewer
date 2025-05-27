using NLog;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Exporters
{
    /// <summary>
    /// 导出类基类, 提供基本的帧渲染功能
    /// </summary>
    public abstract class BaseExporter : IDisposable
    {
        /// <summary>
        /// 日志器
        /// </summary>
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 用于渲染的画布
        /// </summary>
        protected RenderTexture _renderTexture;

        /// <summary>
        /// 初始化导出器
        /// </summary>
        /// <param name="width">画布宽像素值</param>
        /// <param name="height">画布高像素值</param>
        public BaseExporter(uint width , uint height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException($"Invalid resolution: {width}, {height}");
            _renderTexture = new(width, height);
            _renderTexture.SetActive(false);
        }

        /// <summary>
        /// 初始化导出器
        /// </summary>
        public BaseExporter(Vector2u resolution)
        {
            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentException($"Invalid resolution: {resolution}");
            _renderTexture = new(resolution.X, resolution.Y);
            _renderTexture.SetActive(false);
        }

        /// <summary>
        /// 可选的进度回调函数
        /// <list type="number">
        /// <item><c>total</c>: 任务总量</item>
        /// <item><c>done</c>: 已完成量</item>
        /// <item><c>progressText</c>: 需要设置的进度提示文本</item>
        /// </list>
        /// </summary>
        public Action<float, float, string>? ProgressReporter { get => _progressReporter; set => _progressReporter = value; }
        protected Action<float, float, string>? _progressReporter;

        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                var bcPma = value;
                var a = bcPma.A / 255f;
                bcPma.R = (byte)(bcPma.R * a);
                bcPma.G = (byte)(bcPma.G * a);
                bcPma.B = (byte)(bcPma.B * a);
                _backgroundColorPma = bcPma;
            }
        }
        protected Color _backgroundColor = Color.Transparent;

        /// <summary>
        /// 预乘后的背景颜色
        /// </summary>
        protected Color _backgroundColorPma = Color.Transparent;

        /// <summary>
        /// 画面分辨率
        /// <inheritdoc cref="RenderTexture.Size"/>
        /// </summary>
        public Vector2u Resolution
        {
            get => _renderTexture.Size;
            set
            {
                if (value.X <= 0 || value.Y <= 0)
                {
                    _logger.Warn("Omit invalid exporter resolution: {0}", value);
                    return;
                }
                if (_renderTexture.Size != value)
                {
                    using var old = _renderTexture;
                    using var view = old.GetView();
                    var renderTexture = new RenderTexture(value.X, value.Y);
                    renderTexture.SetActive(false);
                    renderTexture.SetView(view);
                    _renderTexture = renderTexture;
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="View.Viewport"/>
        /// </summary>
        public FloatRect Viewport
        {
            get { using var view = _renderTexture.GetView(); return view.Viewport; }
            set { using var view = _renderTexture.GetView(); view.Viewport = value; _renderTexture.SetView(view); }
        }

        /// <summary>
        /// <inheritdoc cref="View.Center"/>
        /// </summary>
        public Vector2f Center
        {
            get { using var view = _renderTexture.GetView(); return view.Center; }
            set { using var view = _renderTexture.GetView(); view.Center = value; _renderTexture.SetView(view); }
        }

        /// <summary>
        /// <inheritdoc cref="View.Size"/>
        /// </summary>
        public Vector2f Size
        {
            get { using var view = _renderTexture.GetView(); return view.Size; }
            set { using var view = _renderTexture.GetView(); view.Size = value; _renderTexture.SetView(view); }
        }

        /// <summary>
        /// <inheritdoc cref="View.Rotation"/>
        /// </summary>
        public float Rotation
        {
            get { using var view = _renderTexture.GetView(); return view.Rotation; }
            set { using var view = _renderTexture.GetView(); view.Rotation = value; _renderTexture.SetView(view); }
        }

        /// <summary>
        /// 获取的一帧, 结果是预乘的
        /// </summary>
        protected virtual SFMLImageVideoFrame GetFrame(SpineObject[] spines)
        {
            _renderTexture.SetActive(true);
            _renderTexture.Clear(_backgroundColorPma);
            foreach (var sp in spines.Reverse()) _renderTexture.Draw(sp);
            _renderTexture.Display();
            _renderTexture.SetActive(false);
            return new(_renderTexture.Texture.CopyToImage());
        }

        /// <summary>
        /// 导出给定的模型, 从前往后对应从上往下的渲染顺序
        /// </summary>
        /// <param name="output">输出路径, 一般而言都是文件路径, 少数情况指定的是文件夹</param>
        /// <param name="spines">要导出的模型, 从前往后对应从上往下的渲染顺序</param>
        public abstract void Export(string output, params SpineObject[] spines);

        #region IDisposable 接口实现

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _renderTexture.Dispose();
            }
            _disposed = true;
        }

        ~BaseExporter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            if (_disposed)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
