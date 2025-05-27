using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Expression.Shapes;
using NLog;
using SFMLRenderer;
using SpineViewer.Extensions;
using SpineViewer.Models;
using SpineViewer.Resources;
using SpineViewer.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SpineViewer.ViewModels
{
    public class SFMLRendererViewModel : ObservableObject
    {
        /// <summary>
        /// 日志器
        /// </summary>
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly MainWindowViewModel _vmMain;
        private readonly ObservableCollectionWithLock<SpineObjectModel> _models;
        private readonly ISFMLRenderer _renderer;

        /// <summary>
        /// 预览画面坐标轴颜色
        /// </summary>
        private static readonly SFML.Graphics.Color _axisColor = new(220, 220, 220);

        /// <summary>
        /// 坐标轴顶点缓冲区
        /// </summary>
        private readonly SFML.Graphics.VertexArray _axisVertices = new(SFML.Graphics.PrimitiveType.Lines, 2); // XXX: 暂时未使用 Dispose 释放

        /// <summary>
        /// 帧间隔计时器
        /// </summary>
        private readonly SFML.System.Clock _clock = new();

        /// <summary>
        /// 渲染任务
        /// </summary>
        private Task? _renderTask = null;
        private CancellationTokenSource? _cancelToken = null;

        /// <summary>
        /// 快进时间量
        /// </summary>
        private float _forwardDelta = 0;
        private readonly object _forwardDeltaLock = new();

        /// <summary>
        /// 临时变量, 记录拖放世界源点
        /// </summary>
        private SFML.System.Vector2f? _draggingSrc = null;

        public SFMLRendererViewModel(MainWindowViewModel vmMain)
        {
            _vmMain = vmMain;
            _models = _vmMain.SpineObjects;
            _renderer = _vmMain.SFMLRenderer;
        }

        /// <summary>
        /// 请求选中项发生变化
        /// </summary>
        public event NotifyCollectionChangedEventHandler? RequestSelectionChanging;

        public uint ResolutionX
        {
            get => _renderer.Resolution.X;
            set => SetProperty(_renderer.Resolution.X, value, _renderer, (r, v) => r.Resolution = new(v, r.Resolution.Y));
        }

        public uint ResolutionY
        {
            get => _renderer.Resolution.Y;
            set => SetProperty(_renderer.Resolution.Y, value, _renderer, (r, v) => r.Resolution = new(r.Resolution.X, v));
        }

        public float CenterX
        {
            get => _renderer.Center.X;
            set => SetProperty(_renderer.Center.X, value, _renderer, (r, v) => r.Center = new(v, r.Center.Y));
        }

        public float CenterY
        {
            get => _renderer.Center.Y;
            set => SetProperty(_renderer.Center.Y, value, _renderer, (r, v) => r.Center = new(r.Center.X, v));
        }

        public float Zoom
        {
            get => _renderer.Zoom;
            set => SetProperty(_renderer.Zoom, value, _renderer, (r, v) => r.Zoom = value);
        }

        public float Rotation
        {
            get => _renderer.Rotation;
            set => SetProperty(_renderer.Rotation, value, _renderer, (r, v) => r.Rotation = value);
        }

        public bool FlipX
        {
            get => _renderer.FlipX;
            set => SetProperty(_renderer.FlipX, value, _renderer, (r, v) => r.FlipX = value);
        }

        public bool FlipY
        {
            get => _renderer.FlipY;
            set => SetProperty(_renderer.FlipY, value, _renderer, (r, v) => r.FlipY = value);
        }

        public uint MaxFps
        {
            get => _renderer.MaxFps;
            set => SetProperty(_renderer.MaxFps, value, _renderer, (r, v) => r.MaxFps = value);
        }

        public bool ShowAxis
        {
            get => _showAxis;
            set => SetProperty(ref _showAxis, value);
        }
        private bool _showAxis = true;

        public Color BackgroundColor
        {
            get => Color.FromRgb(_backgroundColor.R, _backgroundColor.G, _backgroundColor.B);
            set => SetProperty(BackgroundColor, value, this, (m, v) => m._backgroundColor = new(value.R, value.G, value.B));
        }
        private SFML.Graphics.Color _backgroundColor = new(105, 105, 105);

        public bool RenderSelectedOnly
        {
            get => _renderSelectedOnly;
            set => SetProperty(ref _renderSelectedOnly, value);
        }
        private bool _renderSelectedOnly = false;

        public bool IsUpdating
        {
            get => _isUpdating;
            private set
            {
                if (value == _isUpdating) return;
                _isUpdating = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Geo_PlayPause));
            }
        }
        private bool _isUpdating = true;

        public RelayCommand Cmd_Stop => _cmd_Stop ??= new(() =>
        {
            IsUpdating = false;
            lock (_models) foreach (var sp in _models) sp.ResetAnimationsTime();
        });
        private RelayCommand? _cmd_Stop;

        public RelayCommand Cmd_PlayPause => _cmd_PlayPause ??= new(() => IsUpdating = !IsUpdating);
        private RelayCommand? _cmd_PlayPause;

        public Geometry Geo_PlayPause => _isUpdating ? AppResource.Geo_Pause : AppResource.Geo_Play;

        public RelayCommand Cmd_Restart => _cmd_Restart ??= new(() =>
        {
            lock (_models) foreach (var sp in _models) sp.ResetAnimationsTime();
            IsUpdating = true;
        });
        private RelayCommand? _cmd_Restart;

        public RelayCommand Cmd_ForwardStep => _cmd_ForwardStep ??= new(() => 
        { 
            lock (_forwardDeltaLock) _forwardDelta += _renderer.MaxFps > 0 ? 1f / _renderer.MaxFps : 0.001f; 
        });
        private RelayCommand? _cmd_ForwardStep;

        public RelayCommand Cmd_ForwardFast => _cmd_ForwardFast ??= new(() =>
        { 
            lock (_forwardDeltaLock) _forwardDelta += _renderer.MaxFps > 0 ? 10f / _renderer.MaxFps : 0.01f; 
        });
        private RelayCommand? _cmd_ForwardFast;

        public void CanvasMouseWheelScrolled(object? s, SFML.Window.MouseWheelScrollEventArgs e)
        {
            var factor = e.Delta > 0 ? 1.1f : 0.9f;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                Zoom = Math.Clamp(Zoom * factor, 0.001f, 1000f); // 滚轮缩放限制一下缩放范围
            }
            else
            {
                lock (_models.Lock)
                {
                    foreach (var sp in _models.Where(it => it.IsShown && it.IsSelected))
                    {
                        sp.Scale = Math.Clamp(sp.Scale * factor, 0.001f, 1000f); // 滚轮缩放限制一下缩放范围
                    }
                }
            }
        }

        public void CanvasMouseButtonPressed(object? s, SFML.Window.MouseButtonEventArgs e)
        {
            if (e.Button == SFML.Window.Mouse.Button.Right)
            {
                _draggingSrc = _renderer.MapPixelToCoords(new(e.X, e.Y));
            }
            else if (e.Button == SFML.Window.Mouse.Button.Left && !SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Right))
            {
                var _src = _renderer.MapPixelToCoords(new(e.X, e.Y));
                var src = new Point(_src.X, _src.Y);
                _draggingSrc = _src;

                lock (_models.Lock)
                {
                    // 仅渲染选中模式禁止在画面里选择对象
                    if (_renderSelectedOnly)
                    {
                        // 只在被选中的对象里判断是否有效命中
                        bool hit = _models.Any(m => m.IsSelected && m.GetCurrentBounds().Contains(src));

                        // 如果没点到被选中的模型, 则不允许拖动
                        if (!hit) _draggingSrc = null;
                    }
                    else
                    {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                        {
                            // 没按 Ctrl 的情况下, 如果命中了已选中对象, 则就算普通命中
                            bool hit = false;
                            foreach (var sp in _models)
                            {
                                if (!sp.IsShown) continue;
                                if (!sp.GetCurrentBounds().Contains(src)) continue;

                                hit = true;

                                // 如果点到了没被选中的东西, 则清空原先选中的, 改为只选中这一次点的
                                if (!sp.IsSelected)
                                {
                                    RequestSelectionChanging?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
                                    RequestSelectionChanging?.Invoke(this, new(NotifyCollectionChangedAction.Add, sp));
                                }
                                break;
                            }

                            // 如果点了空白的地方, 就清空选中列表
                            if (!hit) RequestSelectionChanging?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
                        }
                        else
                        {
                            // 按下 Ctrl 的情况就执行多选, 并且点空白处也不会清空选中, 如果点击了本来就是选中的则取消选中
                            if (_models.FirstOrDefault(m => m.IsShown && m.GetCurrentBounds().Contains(src), null) is SpineObjectModel sp)
                            {
                                if (sp.IsSelected)
                                    RequestSelectionChanging?.Invoke(this, new(NotifyCollectionChangedAction.Remove, sp));
                                else
                                    RequestSelectionChanging?.Invoke(this, new(NotifyCollectionChangedAction.Add, sp));
                            }
                        }
                    }
                }
            }
        }

        public void CanvasMouseMove(object? s, SFML.Window.MouseMoveEventArgs e)
        {
            if (_draggingSrc is null) return;

            var src = (SFML.System.Vector2f)_draggingSrc;
            var dst = _renderer.MapPixelToCoords(new(e.X, e.Y));
            var delta = dst - src;

            if (SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Right))
            {
                _renderer.Center -= delta;
                OnPropertyChanged(nameof(CenterX));
                OnPropertyChanged(nameof(CenterY));
            }
            else if (SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Left))
            {
                lock (_models.Lock)
                {
                    foreach (var sp in _models.Where(it => it.IsShown && it.IsSelected))
                    {
                        sp.X += delta.X;
                        sp.Y += delta.Y;
                    }
                }
                _draggingSrc = dst;
            }
        }

        public void CanvasMouseButtonReleased(object? s, SFML.Window.MouseButtonEventArgs e)
        {
            if (e.Button == SFML.Window.Mouse.Button.Right)
            {
                _draggingSrc = null;
            }
            else if (e.Button == SFML.Window.Mouse.Button.Left && !SFML.Window.Mouse.IsButtonPressed(SFML.Window.Mouse.Button.Right))
            {
                _draggingSrc = null;
            }
        }

        public void StartRender()
        {
            if (_renderTask is not null) return;
            _cancelToken = new();
            _renderTask = new Task(RenderTask, _cancelToken.Token, TaskCreationOptions.LongRunning);
            _renderTask.Start();
            IsUpdating = true;
        }

        public void StopRender()
        {
            IsUpdating = false;
            if (_renderTask is null || _cancelToken is null) return;
            _cancelToken.Cancel();
            _renderTask.Wait();
            _cancelToken = null;
            _renderTask = null;
        }

        private void RenderTask()
        {
            try
            {
                _renderer.SetActive(true);

                float delta;
                while (!_cancelToken?.IsCancellationRequested ?? false)
                {
                    delta = _clock.ElapsedTime.AsSeconds();
                    _clock.Restart();

                    // 停止更新的时候只是时间不前进, 但是坐标变换还是要更新, 否则无法移动对象
                    if (!_isUpdating) delta = 0;

                    // 加上要快进的量
                    lock (_forwardDeltaLock)
                    {
                        delta += _forwardDelta;
                        _forwardDelta = 0;
                    }

                    _renderer.Clear(_backgroundColor);

                    if (_showAxis)
                    {
                        // 画一个很长的坐标轴, 用 1e9 比较合适
                        _axisVertices[0] = new(new(-1e9f, 0), _axisColor);
                        _axisVertices[1] = new(new(1e9f, 0), _axisColor);
                        _renderer.Draw(_axisVertices);
                        _axisVertices[0] = new(new(0, -1e9f), _axisColor);
                        _axisVertices[1] = new(new(0, 1e9f), _axisColor);
                        _renderer.Draw(_axisVertices);
                    }

                    // 渲染 Spine
                    lock (_models.Lock)
                    {
                        foreach (var sp in _models.Where(sp => sp.IsShown && (!_renderSelectedOnly || sp.IsSelected)).Reverse())
                        {
                            if (_cancelToken?.IsCancellationRequested ?? true) break; // 提前中止

                            sp.Update(0); // 避免物理效果出现问题
                            sp.Update(delta);

                            sp.EnableDebug = true;
                            _renderer.Draw(sp);
                            sp.EnableDebug = false;
                        }
                    }

                    _renderer.Display();
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(ex.ToString());
                _logger.Fatal("Render task stopped, {0}", ex.Message);
                MessagePopupService.Error(ex.ToString());
            }
            finally
            {
                _renderer.SetActive(false);
            }
        }
    }
}
