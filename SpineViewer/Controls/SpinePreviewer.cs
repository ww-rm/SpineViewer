﻿using System;
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

namespace SpineViewer.Controls
{
    public partial class SpinePreviewer : UserControl
    {
        /// <summary>
        /// 日志器
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SpinePreviewer()
        {
            InitializeComponent();
            RenderWindow = new(panel.Handle);
            RenderWindow.SetActive(false);

            // 设置默认参数
            Resolution = new(2048, 2048);
            Center = new(0, 0);
            FlipY = true;
            MaxFps = 30;
        }

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
                    propertyGrid.SelectedObject = new SpinePreviewerProperty(this);
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

                float parentX = panel.Parent.Width;
                float parentY = panel.Parent.Height;
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
                RenderWindow.Position = new((int)(parentX - sizeX) / 2, (int)(parentY - sizeY) / 2);
                RenderWindow.Size = new((uint)sizeX, (uint)sizeY);

                // 将 view 的大小设置成于 resolution 相同的大小, 其余属性都不变
                using var view = RenderWindow.GetView();
                var signX = Math.Sign(view.Size.X);
                var signY = Math.Sign(view.Size.Y);
                view.Size = new(value.Width * signX, value.Height * signY);
                RenderWindow.SetView(view);

                resolution = value;
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
                using var view = RenderWindow.GetView();
                var center = view.Center;
                return new(center.X, center.Y);
            }
            set
            {
                using var view = RenderWindow.GetView();
                view.Center = new(value.X, value.Y);
                RenderWindow.SetView(view);
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
                using var view = RenderWindow.GetView();
                return resolution.Width / Math.Abs(view.Size.X);
            }
            set
            {
                value = Math.Clamp(value, 0.001f, 1000f);
                using var view = RenderWindow.GetView();
                var signX = Math.Sign(view.Size.X);
                var signY = Math.Sign(view.Size.Y);
                view.Size = new(resolution.Width / value * signX, resolution.Height / value * signY);
                RenderWindow.SetView(view);
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
                using var view = RenderWindow.GetView();
                return view.Rotation;
            }
            set
            {
                using var view = RenderWindow.GetView();
                view.Rotation = value;
                RenderWindow.SetView(view);
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
                using var view = RenderWindow.GetView();
                return view.Size.X < 0;
            }
            set
            {
                using var view = RenderWindow.GetView();
                var size = view.Size;
                if (size.X > 0 && value || size.X < 0 && !value)
                    size.X *= -1;
                view.Size = size;
                RenderWindow.SetView(view);
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
                using var view = RenderWindow.GetView();
                return view.Size.Y < 0;
            }
            set
            {
                using var view = RenderWindow.GetView();
                var size = view.Size;
                if (size.Y > 0 && value || size.Y < 0 && !value)
                    size.Y *= -1;
                view.Size = size;
                RenderWindow.SetView(view);
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
        public uint MaxFps { get => maxFps; set { RenderWindow.SetFramerateLimit(value); maxFps = value; } }
        private uint maxFps = 60;

        /// <summary>
        /// 获取 View
        /// </summary>
        public SFML.Graphics.View GetView() => RenderWindow.GetView();

        #endregion

        #region 渲染管理

        /// <summary>
        /// 预览画面背景色
        /// </summary>
        private static readonly SFML.Graphics.Color BackgroundColor = new(105, 105, 105);

        /// <summary>
        /// 预览画面坐标轴颜色
        /// </summary>
        private static readonly SFML.Graphics.Color AxisColor = new(220, 220, 220);

        /// <summary>
        /// 坐标轴顶点缓冲区
        /// </summary>
        private readonly SFML.Graphics.VertexArray AxisVertex = new(SFML.Graphics.PrimitiveType.Lines, 2);

        /// <summary>
        /// 渲染窗口
        /// </summary>
        private readonly SFML.Graphics.RenderWindow RenderWindow;

        /// <summary>
        /// 帧间隔计时器
        /// </summary>
        private readonly SFML.System.Clock Clock = new();

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
            if (task is not null)
                return;
            cancelToken = new();
            task = Task.Run(RenderTask, cancelToken.Token);
            IsUpdating = true;
        }

        /// <summary>
        /// 停止渲染
        /// </summary>
        public void StopRender()
        {
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
                RenderWindow.SetActive(true);

                float delta;
                while (cancelToken is not null && !cancelToken.IsCancellationRequested)
                {
                    delta = Clock.ElapsedTime.AsSeconds();
                    Clock.Restart();

                    // 停止更新的时候只是时间不前进, 但是坐标变换还是要更新, 否则无法移动对象
                    if (!IsUpdating) delta = 0;

                    // 加上要快进的量
                    lock (_forwardDeltaLock)
                    {
                        delta += forwardDelta;
                        forwardDelta = 0;
                    }

                    RenderWindow.Clear(BackgroundColor);

                    if (ShowAxis)
                    {
                        // 画一个很长的坐标轴, 用 1e9 比较合适
                        AxisVertex[0] = new(new(-1e9f, 0), AxisColor);
                        AxisVertex[1] = new(new(1e9f, 0), AxisColor);
                        RenderWindow.Draw(AxisVertex);
                        AxisVertex[0] = new(new(0, -1e9f), AxisColor);
                        AxisVertex[1] = new(new(0, 1e9f), AxisColor);
                        RenderWindow.Draw(AxisVertex);
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

                                spine.IsDebug = true;
                                RenderWindow.Draw(spine);
                                spine.IsDebug = false;
                            }
                        }
                    }

                    RenderWindow.Display();
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                logger.Fatal("Render task stopped");
                MessagePopup.Error(ex.ToString(), "预览画面已停止渲染");
            }
            finally
            {
                RenderWindow.SetActive(false);
            }
        }

        #endregion

        /// <summary>
        /// 画面拖放对象世界坐标源点
        /// </summary>
        private SFML.System.Vector2f? draggingSrc = null;

        private void SpinePreviewer_SizeChanged(object sender, EventArgs e)
        {
            if (RenderWindow is null)
                return;

            float parentX = panel.Parent.Width;
            float parentY = panel.Parent.Height;
            float sizeX = panel.Width;
            float sizeY = panel.Height;

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
            RenderWindow.Position = new((int)(parentX - sizeX) / 2, (int)(parentY - sizeY) / 2);
            RenderWindow.Size = new((uint)sizeX, (uint)sizeY);
        }

        private void panel_MouseDown(object sender, MouseEventArgs e)
        {
            // 右键优先级高, 进入画面拖动模式, 需要重新记录源点
            if ((e.Button & MouseButtons.Right) != 0)
            {
                draggingSrc = RenderWindow.MapPixelToCoords(new(e.X, e.Y));
                Cursor = Cursors.Hand;
            }
            // 按下了左键并且右键是松开的
            else if ((e.Button & MouseButtons.Left) != 0 && (MouseButtons & MouseButtons.Right) == 0)
            {
                draggingSrc = RenderWindow.MapPixelToCoords(new(e.X, e.Y));
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
                            if (!spines[i].Bounds.Contains(src)) continue;
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
                                if (!spines[i].Bounds.Contains(src)) continue;

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
                                if (!spines[i].Bounds.Contains(src)) continue;

                                SpineListView.SelectedIndices.Add(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingSrc is null)
                return;

            var src = (SFML.System.Vector2f)draggingSrc;
            var dst = RenderWindow.MapPixelToCoords(new(e.X, e.Y));
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

        private void panel_MouseUp(object sender, MouseEventArgs e)
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

        private void panel_MouseWheel(object sender, MouseEventArgs e)
        {
            Zoom *= (e.Delta > 0 ? 1.1f : 0.9f);
            PropertyGrid?.Refresh();
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
                forwardDelta += 1f / maxFps;
            }
        }

        private void button_ForwardFast_Click(object sender, EventArgs e)
        {
            lock (_forwardDeltaLock)
            {
                forwardDelta += 10f / maxFps;
            }
        }

        //public void ClickStopButton() => button_Stop_Click(button_Stop, EventArgs.Empty);
        //public void ClickRestartButton() => button_Restart_Click(button_Restart, EventArgs.Empty);
        //public void ClickStartButton() => button_Start_Click(button_Start, EventArgs.Empty);
        //public void ClickForwardStepButton() => button_ForwardStep_Click(button_ForwardStep, EventArgs.Empty);
        //public void ClickForwardFastButton() => button_ForwardFast_Click(button_ForwardFast, EventArgs.Empty);
    }

    /// <summary>
    /// 用于在 PropertyGrid 上显示 SpinePreviewe 属性的包装类, 提供用户操作接口
    /// </summary>
    public class SpinePreviewerProperty(SpinePreviewer previewer)
    {
        [Browsable(false)]
        public SpinePreviewer Previewer { get; } = previewer;

        [TypeConverter(typeof(SizeConverter))]
        [Category("[0] 导出"), DisplayName("分辨率")]
        public Size Resolution { get => Previewer.Resolution; set => Previewer.Resolution = value; }

        [TypeConverter(typeof(PointFConverter))]
        [Category("[0] 导出"), DisplayName("画面中心点")]
        public PointF Center { get => Previewer.Center; set => Previewer.Center = value; }

        [Category("[0] 导出"), DisplayName("缩放")]
        public float Zoom { get => Previewer.Zoom; set => Previewer.Zoom = value; }

        [Category("[0] 导出"), DisplayName("旋转")]
        public float Rotation { get => Previewer.Rotation; set => Previewer.Rotation = value; }

        [Category("[0] 导出"), DisplayName("水平翻转")]
        public bool FlipX { get => Previewer.FlipX; set => Previewer.FlipX = value; }

        [Category("[0] 导出"), DisplayName("垂直翻转")]
        public bool FlipY { get => Previewer.FlipY; set => Previewer.FlipY = value; }

        [Category("[0] 导出"), DisplayName("仅渲染选中")]
        public bool RenderSelectedOnly { get => Previewer.RenderSelectedOnly; set => Previewer.RenderSelectedOnly = value; }

        [Category("[1] 预览"), DisplayName("显示坐标轴")]
        public bool ShowAxis { get => Previewer.ShowAxis; set => Previewer.ShowAxis = value; }

        [Category("[1] 预览"), DisplayName("最大帧率")]
        public uint MaxFps { get => Previewer.MaxFps; set => Previewer.MaxFps = value; }
    }
}
