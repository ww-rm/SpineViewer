using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Policy;
using System.Diagnostics;
using NLog;
using SpineViewer.Utils;
using System.Drawing.Design;

namespace SpineViewer.Controls
{
    public partial class SpinePreviewPanel : UserControl
    {
        public SpinePreviewPanel()
        {
            InitializeComponent();
            renderWindow = new(panel_Render.Handle);
            renderWindow.SetActive(false);
            wallpaperTexture = new(1, 1);
            _wallpaperFormHandle = wallpaperForm.Handle;

            // 设置默认参数
            Resolution = new(2048, 2048);
            Center = new(0, 0);
            FlipY = true;
            MaxFps = 30;
        }

        /// <summary>
        /// 日志器
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 要绑定的 Spine 列表控件
        /// </summary>
        [Category("自定义"), Description("相关联的 SpineListView")]
        public SpineListView? SpineListView { get; set; }

        /// <summary>
        /// 属性信息面板
        /// </summary>
        [Category("自定义"), Description("用于显示画面属性的属性页")]
        public PropertyGrid? PropertyGrid
        {
            get => propertyGrid;
            set
            {
                propertyGrid = value;
                if (propertyGrid is not null)
                    propertyGrid.SelectedObject = new SpinePreviewPanelProperty(this);
            }
        }
        private PropertyGrid? propertyGrid;

        #region 参数属性

        /// <summary>
        /// 分辨率
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Size Resolution
        {
            get => resolution;
            set
            {
                if (value.Width <= 0) value.Width = 100;
                if (value.Height <= 0) value.Height = 100;

                float parentX = panel_Render.Parent.Width;
                float parentY = panel_Render.Parent.Height;
                float sizeX = value.Width;
                float sizeY = value.Height;

                if ((sizeY / sizeX) < (parentY / parentX))
                {
                    // 相同的 X, 子窗口 Y 更小
                    sizeY = parentX * sizeY / sizeX;
                    sizeX = parentX;
                }
                else
                {
                    // 相同的 Y, 子窗口 X 更小
                    sizeX = parentY * sizeX / sizeY;
                    sizeY = parentY;
                }

                // 必须通过 SFML 的方法调整窗口
                renderWindow.Position = new((int)(parentX - sizeX) / 2, (int)(parentY - sizeY) / 2);
                renderWindow.Size = new((uint)sizeX, (uint)sizeY);

                // 将 view 的大小设置成于 resolution 相同的大小, 其余属性都不变
                using var view = renderWindow.GetView();
                var signX = Math.Sign(view.Size.X);
                var signY = Math.Sign(view.Size.Y);
                view.Size = new(value.Width * signX, value.Height * signY);
                renderWindow.SetView(view);

                resolution = value;

                // 设置壁纸窗口分辨率
                wallpaperForm.Size = value;
                _memDCsize = new() { cx = value.Width, cy = value.Height };
                wallpaperTexture.Dispose();
                wallpaperTexture = new((uint)value.Width, (uint)value.Height);
                wallpaperTexture.SetView(view);
            }
        }
        private Size resolution = new(0, 0);

        /// <summary>
        /// 画面中心点
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public PointF Center
        {
            get
            {
                using var view = renderWindow.GetView();
                var center = view.Center;
                return new(center.X, center.Y);
            }
            set
            {
                using var view = renderWindow.GetView();
                view.Center = new(value.X, value.Y);
                renderWindow.SetView(view);
                wallpaperTexture.SetView(view);
            }
        }

        /// <summary>
        /// 画面缩放
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public float Zoom
        {
            get
            {
                using var view = renderWindow.GetView();
                return resolution.Width / Math.Abs(view.Size.X);
            }
            set
            {
                value = Math.Clamp(value, 0.001f, 1000f);
                using var view = renderWindow.GetView();
                var signX = Math.Sign(view.Size.X);
                var signY = Math.Sign(view.Size.Y);
                view.Size = new(resolution.Width / value * signX, resolution.Height / value * signY);
                renderWindow.SetView(view);
                wallpaperTexture.SetView(view);
            }
        }

        /// <summary>
        /// 画面旋转
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Rotation
        {
            get
            {
                using var view = renderWindow.GetView();
                return view.Rotation;
            }
            set
            {
                using var view = renderWindow.GetView();
                view.Rotation = value;
                renderWindow.SetView(view);
                wallpaperTexture.SetView(view);
            }
        }

        /// <summary>
        /// 水平翻转
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool FlipX
        {
            get
            {
                using var view = renderWindow.GetView();
                return view.Size.X < 0;
            }
            set
            {
                using var view = renderWindow.GetView();
                var size = view.Size;
                if (size.X > 0 && value || size.X < 0 && !value)
                    size.X *= -1;
                view.Size = size;
                renderWindow.SetView(view);
                wallpaperTexture.SetView(view);
            }
        }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool FlipY
        {
            get
            {
                using var view = renderWindow.GetView();
                return view.Size.Y < 0;
            }
            set
            {
                using var view = renderWindow.GetView();
                var size = view.Size;
                if (size.Y > 0 && value || size.Y < 0 && !value)
                    size.Y *= -1;
                view.Size = size;
                renderWindow.SetView(view);
                wallpaperTexture.SetView(view);
            }
        }

        /// <summary>
        /// 仅渲染选中
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool RenderSelectedOnly { get; set; } = false;

        /// <summary>
        /// 显示坐标轴
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool ShowAxis { get; set; } = true;

        /// <summary>
        /// 最大帧率
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public uint MaxFps { get => maxFps; set { renderWindow.SetFramerateLimit(value); maxFps = value; } }
        private uint maxFps = 60;

        /// <summary>
        /// 获取 View
        /// </summary>
        public SFML.Graphics.View GetView() => renderWindow.GetView();

        /// <summary>
        /// 是否开启桌面投影
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool EnableDesktopProjection
        {
            get => enableDesktopProjection;
            set
            {
                if (enableDesktopProjection == value) return;
                if (value)
                {
                    screenDC = Win32.GetDC(IntPtr.Zero);
                    memDC = Win32.CreateCompatibleDC(screenDC);
                    wallpaperForm.Show();
                }
                else
                {
                    wallpaperForm.Hide();
                    Win32.DeleteDC(memDC);
                    Win32.ReleaseDC(IntPtr.Zero, screenDC);
                    memDC = 0;
                    screenDC = 0;
                }
                enableDesktopProjection = value;
            }
        }
        private bool enableDesktopProjection = false;

        #endregion

        #region 渲染管理

        /// <summary>
        /// 预览画面背景色
        /// </summary>
        public SFML.Graphics.Color BackgroundColor { get; set; } = new(105, 105, 105);

        /// <summary>
        /// 预览画面坐标轴颜色
        /// </summary>
        private static readonly SFML.Graphics.Color AxisColor = new(220, 220, 220);

        /// <summary>
        /// 坐标轴顶点缓冲区
        /// </summary>
        private readonly SFML.Graphics.VertexArray axisVertices = new(SFML.Graphics.PrimitiveType.Lines, 2); // XXX: 暂时未使用 Dispose 释放

        /// <summary>
        /// 渲染窗口
        /// </summary>
        private readonly SFML.Graphics.RenderWindow renderWindow;

        /// <summary>
        /// 屏幕 DC
        /// </summary>
        private IntPtr screenDC;

        /// <summary>
        /// 用于壁纸窗口的内存 DC
        /// </summary>
        private IntPtr memDC;
        private readonly IntPtr _wallpaperFormHandle;
        private Win32.SIZE _memDCsize = new();
        private Win32.POINT _memDCptSrc = new() { x = 0, y = 0 };
        private Win32.BLENDFUNCTION _memDCblend = new() { BlendOp = 0, BlendFlags = 0, SourceConstantAlpha = 255, AlphaFormat = Win32.AC_SRC_ALPHA };

        /// <summary>
        /// 壁纸窗口
        /// </summary>
        private SFML.Graphics.RenderTexture wallpaperTexture;

        /// <summary>
        /// 帧间隔计时器
        /// </summary>
        private readonly SFML.System.Clock clock = new();

        /// <summary>
        /// 渲染任务
        /// </summary>
        private Task? task = null;
        private CancellationTokenSource? cancelToken = null;

        /// <summary>
        /// 是否更新画面
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsUpdating
        {
            get => isUpdating;
            private set
            {
                if (value == isUpdating) return;
                if (value)
                {
                    button_Start.ImageKey = "pause";
                }
                else
                {
                    button_Start.ImageKey = "start";
                }
                isUpdating = value;
            }
        }
        private bool isUpdating = true;

        /// <summary>
        /// 快进时间量
        /// </summary>
        private float forwardDelta = 0;
        private object _forwardDeltaLock = new();

        /// <summary>
        /// 开始渲染
        /// </summary>
        public void StartRender()
        {
            if (task is not null) return;
            cancelToken = new();
            task = Task.Run(RenderTask, cancelToken.Token);
            IsUpdating = true;
            if (enableDesktopProjection) wallpaperForm.Show();
        }

        /// <summary>
        /// 停止渲染
        /// </summary>
        public void StopRender()
        {
            wallpaperForm.Hide();
            IsUpdating = false;
            if (task is null || cancelToken is null)
                return;
            cancelToken.Cancel();
            task.Wait();
            cancelToken = null;
            task = null;
        }

        /// <summary>
        /// 渲染任务
        /// </summary>
        private void RenderTask()
        {
            try
            {
                renderWindow.SetActive(true);
                wallpaperTexture.SetActive(true);

                float delta;
                while (cancelToken is not null && !cancelToken.IsCancellationRequested)
                {
                    delta = clock.ElapsedTime.AsSeconds();
                    clock.Restart();

                    // 停止更新的时候只是时间不前进, 但是坐标变换还是要更新, 否则无法移动对象
                    if (!IsUpdating) delta = 0;

                    // 加上要快进的量
                    lock (_forwardDeltaLock)
                    {
                        delta += forwardDelta;
                        forwardDelta = 0;
                    }

                    renderWindow.Clear(BackgroundColor);
                    if (enableDesktopProjection) wallpaperTexture.Clear(SFML.Graphics.Color.Transparent);

                    if (ShowAxis)
                    {
                        // 画一个很长的坐标轴, 用 1e9 比较合适
                        axisVertices[0] = new(new(-1e9f, 0), AxisColor);
                        axisVertices[1] = new(new(1e9f, 0), AxisColor);
                        renderWindow.Draw(axisVertices);
                        axisVertices[0] = new(new(0, -1e9f), AxisColor);
                        axisVertices[1] = new(new(0, 1e9f), AxisColor);
                        renderWindow.Draw(axisVertices);
                    }

                    // 渲染 Spine
                    if (SpineListView is not null)
                    {
                        lock (SpineListView.Spines)
                        {
                            var spines = SpineListView.Spines.Where(sp => !sp.IsHidden).ToArray();
                            for (int i = spines.Length - 1; i >= 0; i--)
                            {
                                if (cancelToken is not null && cancelToken.IsCancellationRequested)
                                    break; // 提前中止

                                var spine = spines[i];

                                spine.Update(delta);

                                if (RenderSelectedOnly && !spine.IsSelected)
                                    continue;

                                spine.EnableDebug = true;
                                renderWindow.Draw(spine);
                                spine.EnableDebug = false;

                                if (enableDesktopProjection) wallpaperTexture.Draw(spine);
                            }
                        }
                    }

                    renderWindow.Display();
                    
                    // 桌面投影
                    if (enableDesktopProjection)
                    {
                        wallpaperTexture.Display();
                        using var img = wallpaperTexture.Texture.CopyToImage();
                        img.SaveToMemory(out var imgBuffer, "bmp");
                        using var stream = new MemoryStream(imgBuffer);
                        using var bitmap = new Bitmap(stream);

                        var newBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                        var oldBitmap = Win32.SelectObject(memDC, newBitmap);

                        Win32.UpdateLayeredWindow(_wallpaperFormHandle, screenDC, IntPtr.Zero, ref _memDCsize, memDC, ref _memDCptSrc, 0, ref _memDCblend, Win32.ULW_ALPHA);

                        Win32.SelectObject(memDC, oldBitmap);
                        Win32.DeleteObject(newBitmap);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                logger.Fatal("Render task stopped");
                MessagePopup.Error(ex.ToString(), "预览画面已停止渲染");
            }
            finally
            {
                renderWindow.SetActive(false);
                wallpaperTexture.SetActive(false);
            }
        }

        #endregion

        /// <summary>
        /// 画面拖放对象世界坐标源点
        /// </summary>
        private SFML.System.Vector2f? draggingSrc = null;

        private void SpinePreviewPanel_SizeChanged(object sender, EventArgs e)
        {
            if (renderWindow is null)
                return;

            float parentW = panel_Render.Parent.Width;
            float parentH = panel_Render.Parent.Height;
            float renderW = panel_Render.Width;
            float renderH = panel_Render.Height;
            float scale = Math.Min(parentW / renderW, parentH / renderH); // 两方向取较小值, 保证 parent 覆盖 render
            renderH *= scale;
            renderW *= scale;

            // 必须通过 SFML 的方法调整窗口
            renderWindow.Position = new((int)(parentW - renderW) / 2, (int)(parentH - renderH) / 2);
            renderWindow.Size = new((uint)renderW, (uint)renderH);
        }

        private void panel_Render_MouseDown(object sender, MouseEventArgs e)
        {
            // 右键优先级高, 进入画面拖动模式, 需要重新记录源点
            if ((e.Button & MouseButtons.Right) != 0)
            {
                draggingSrc = renderWindow.MapPixelToCoords(new(e.X, e.Y));
                Cursor = Cursors.Hand;
            }
            // 按下了左键并且右键是松开的
            else if ((e.Button & MouseButtons.Left) != 0 && (MouseButtons & MouseButtons.Right) == 0)
            {
                draggingSrc = renderWindow.MapPixelToCoords(new(e.X, e.Y));
                var src = new PointF(((SFML.System.Vector2f)draggingSrc).X, ((SFML.System.Vector2f)draggingSrc).Y);

                if (SpineListView is null)
                    return;

                lock (SpineListView.Spines)
                {
                    var spines = SpineListView.Spines;

                    // 仅渲染选中模式禁止在画面里选择对象
                    if (RenderSelectedOnly)
                    {
                        // 只在被选中的对象里判断是否有效命中
                        bool hit = false;
                        foreach (int i in SpineListView.SelectedIndices)
                        {
                            if (spines[i].IsHidden) continue;
                            if (!spines[i].GetCurrentBounds().Contains(src)) continue;
                            hit = true;
                            break;
                        }

                        // 如果没点到被选中的模型, 则不允许拖动
                        if (!hit) draggingSrc = null;
                    }
                    else
                    {
                        if ((ModifierKeys & Keys.Control) == 0)
                        {
                            // 没按 Ctrl 的情况下, 如果命中了已选中对象, 则就算普通命中
                            bool hit = false;
                            for (int i = 0; i < spines.Count; i++)
                            {
                                if (spines[i].IsHidden) continue;
                                if (!spines[i].GetCurrentBounds().Contains(src)) continue;

                                hit = true;

                                // 如果点到了没被选中的东西, 则清空原先选中的, 改为只选中这一次点的
                                if (!SpineListView.SelectedIndices.Contains(i))
                                {
                                    SpineListView.SelectedIndices.Clear();
                                    SpineListView.SelectedIndices.Add(i);
                                }
                                break;
                            }

                            // 如果点了空白的地方, 就清空选中列表
                            if (!hit) SpineListView.SelectedIndices.Clear();
                        }
                        else
                        {
                            // 按下 Ctrl 的情况就执行多选, 并且点空白处也不会清空选中
                            for (int i = 0; i < spines.Count; i++)
                            {
                                if (spines[i].IsHidden) continue;
                                if (!spines[i].GetCurrentBounds().Contains(src)) continue;

                                SpineListView.SelectedIndices.Add(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void panel_Render_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingSrc is null)
                return;

            var src = (SFML.System.Vector2f)draggingSrc;
            var dst = renderWindow.MapPixelToCoords(new(e.X, e.Y));
            var _delta = dst - src;
            var delta = new SizeF(_delta.X, _delta.Y);

            if ((e.Button & MouseButtons.Right) != 0)
            {
                Center -= delta;
            }
            else if ((e.Button & MouseButtons.Left) != 0)
            {
                if (SpineListView is not null)
                {
                    lock (SpineListView.Spines)
                    {
                        var spines = SpineListView.Spines;
                        foreach (int i in SpineListView.SelectedIndices)
                        {
                            if (spines[i].IsHidden) continue;
                            spines[i].Position += delta;
                        }
                    }
                }
                draggingSrc = dst;
            }
        }

        private void panel_Render_MouseUp(object sender, MouseEventArgs e)
        {
            // 右键高优先级, 结束画面拖动模式
            if ((e.Button & MouseButtons.Right) != 0)
            {
                SpineListView?.SpinePropertyGrid?.Refresh();

                draggingSrc = null;
                Cursor = Cursors.Default;
                PropertyGrid?.Refresh();
            }
            // 按下了左键并且右键是松开的
            else if ((e.Button & MouseButtons.Left) != 0 && (MouseButtons & MouseButtons.Right) == 0)
            {
                draggingSrc = null;
                SpineListView?.SpinePropertyGrid?.Refresh();
            }
        }

        private void panel_Render_MouseWheel(object sender, MouseEventArgs e)
        {
            var factor = (e.Delta > 0 ? 1.1f : 0.9f);
            if ((ModifierKeys & Keys.Control) == 0)
            {
                Zoom *= factor;
                PropertyGrid?.Refresh();
            }
            else
            {
                if (SpineListView is not null)
                {
                    lock (SpineListView.Spines)
                    {
                        var spines = SpineListView.Spines;
                        foreach (int i in SpineListView.SelectedIndices)
                        {
                            if (spines[i].IsHidden) continue;
                            spines[i].Scale *= factor;
                        }
                    }
                    SpineListView.SpinePropertyGrid?.Refresh();
                }
            }
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            IsUpdating = false;
            if (SpineListView is not null)
            {
                lock (SpineListView.Spines)
                {
                    foreach (var spine in SpineListView.Spines)
                        spine.ResetAnimationsTime();
                }
            }
        }

        private void button_Restart_Click(object sender, EventArgs e)
        {
            if (SpineListView is not null)
            {
                lock (SpineListView.Spines)
                {
                    foreach (var spine in SpineListView.Spines)
                        spine.ResetAnimationsTime();
                }
            }
            IsUpdating = true;
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            IsUpdating = !IsUpdating;
        }

        private void button_ForwardStep_Click(object sender, EventArgs e)
        {
            lock (_forwardDeltaLock)
            {
                if (maxFps > 0)
                    forwardDelta += 1f / maxFps;
                else
                    forwardDelta += 0.001f;
            }
        }

        private void button_ForwardFast_Click(object sender, EventArgs e)
        {
            lock (_forwardDeltaLock)
            {
                if (maxFps > 0)
                    forwardDelta += 10f / maxFps;
                else
                    forwardDelta += 0.01f;
            }
        }

        //public void ClickStopButton() => button_Stop_Click(button_Stop, EventArgs.Empty);
        //public void ClickRestartButton() => button_Restart_Click(button_Restart, EventArgs.Empty);
        //public void ClickStartButton() => button_Start_Click(button_Start, EventArgs.Empty);
        //public void ClickForwardStepButton() => button_ForwardStep_Click(button_ForwardStep, EventArgs.Empty);
        //public void ClickForwardFastButton() => button_ForwardFast_Click(button_ForwardFast, EventArgs.Empty);
    }

    /// <summary>
    /// 用于在 PropertyGrid 上显示 <see cref="SpinePreviewPanel"/> 属性的包装类, 提供用户操作接口
    /// </summary>
    public class SpinePreviewPanelProperty(SpinePreviewPanel previewPanel)
    {
        [Browsable(false)]
        public SpinePreviewPanel PreviewPanel { get; } = previewPanel;

        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(ResolutionConverter))]
        [Category("[0] 导出"), DisplayName("分辨率")]
        public Size Resolution { get => PreviewPanel.Resolution; set => PreviewPanel.Resolution = value; }

        [TypeConverter(typeof(PointFConverter))]
        [Category("[0] 导出"), DisplayName("画面中心点")]
        public PointF Center { get => PreviewPanel.Center; set => PreviewPanel.Center = value; }

        [Category("[0] 导出"), DisplayName("缩放")]
        public float Zoom { get => PreviewPanel.Zoom; set => PreviewPanel.Zoom = value; }

        [Category("[0] 导出"), DisplayName("旋转")]
        public float Rotation { get => PreviewPanel.Rotation; set => PreviewPanel.Rotation = value; }

        [Category("[0] 导出"), DisplayName("水平翻转")]
        public bool FlipX { get => PreviewPanel.FlipX; set => PreviewPanel.FlipX = value; }

        [Category("[0] 导出"), DisplayName("垂直翻转")]
        public bool FlipY { get => PreviewPanel.FlipY; set => PreviewPanel.FlipY = value; }

        [Category("[0] 导出"), DisplayName("仅渲染选中")]
        public bool RenderSelectedOnly { get => PreviewPanel.RenderSelectedOnly; set => PreviewPanel.RenderSelectedOnly = value; }

        [Category("[1] 预览"), DisplayName("显示坐标轴")]
        public bool ShowAxis { get => PreviewPanel.ShowAxis; set => PreviewPanel.ShowAxis = value; }

        [Category("[1] 预览"), DisplayName("最大帧率")]
        public uint MaxFps { get => PreviewPanel.MaxFps; set => PreviewPanel.MaxFps = value; }

        [Editor(typeof(SFMLColorEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(SFMLColorConverter))]
        [Category("[1] 预览"), DisplayName("背景颜色")]
        public SFML.Graphics.Color BackgroundColor { get => PreviewPanel.BackgroundColor; set => PreviewPanel.BackgroundColor = value; }
    }
}
