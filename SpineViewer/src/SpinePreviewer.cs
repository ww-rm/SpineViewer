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

namespace SpineViewer
{
    public partial class SpinePreviewer : UserControl
    {
        /// <summary>
        /// 包装类, 用于 PropertyGrid 显示
        /// </summary>
        private class PreviewerProperty
        {
            private readonly SpinePreviewer previewer;

            public PreviewerProperty(SpinePreviewer previewer) { this.previewer = previewer; }

            /// <summary>
            /// 导出画面分辨率
            /// </summary>
            [TypeConverter(typeof(SizeTypeConverter))]
            [Category("属性"), DisplayName("分辨率")]
            public Size Resolution { get => previewer.Resolution; set => previewer.Resolution = value; }

            [TypeConverter(typeof(PointFTypeConverter))]
            [Category("属性"), DisplayName("画面中心点")]
            public PointF Center { get => previewer.Center; set => previewer.Center = value; }

            /// <summary>
            /// 画面缩放
            /// </summary>
            [Category("属性"), DisplayName("缩放")]
            public float Zoom { get => previewer.Zoom; set => previewer.Zoom = value; }

            /// <summary>
            /// 画面旋转
            /// </summary>
            [Category("属性"), DisplayName("旋转")]
            public float Rotation { get => previewer.Rotation; set => previewer.Rotation = value; }

            /// <summary>
            /// 画面旋转
            /// </summary>
            [Category("属性"), DisplayName("水平翻转")]
            public bool FlipX { get => previewer.FlipX; set => previewer.FlipX = value; }

            /// <summary>
            /// 画面旋转
            /// </summary>
            [Category("属性"), DisplayName("垂直翻转")]
            public bool FlipY { get => previewer.FlipY; set => previewer.FlipY = value; }
        }

        [Category("自定义"), Description("用于显示画面属性的属性页")]
        public PropertyGrid? PropertyGrid
        {
            get => propertyGrid;
            set
            {
                propertyGrid = value;
                if (propertyGrid is not null)
                    propertyGrid.SelectedObject = new PreviewerProperty(this);
            }
        }
        private PropertyGrid? propertyGrid;

        [Category("自定义"), Description("帧渲染事件")]
        public event EventHandler<RenderFrameEventArgs>? RenderFrame;
        private void OnRenderFrame(float delta) { RenderFrame?.Invoke(this, new(RenderWindow, delta)); }

        private readonly SFML.Graphics.RenderWindow RenderWindow;
        private readonly SFML.System.Clock Clock = new();
        private readonly SFML.Graphics.Color BackgroundColor = SFML.Graphics.Color.Green;

        public SpinePreviewer()
        {
            InitializeComponent();
            RenderWindow = new(panel.Handle);
            RenderWindow.SetFramerateLimit(30);
            RenderWindow.SetActive(false);
            Resolution = new(1280, 720);
            Center = new(0, 0);
            FlipY = true;
        }

        public const float ZOOM_MAX = 1000f;
        public const float ZOOM_MIN = 0.001f;

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

                float parentX = Width;
                float parentY = Height;
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
                var view = RenderWindow.GetView();
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
                var center = RenderWindow.GetView().Center;
                return new(center.X, center.Y);
            }
            set
            {
                var view = RenderWindow.GetView();
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
            get => resolution.Width / Math.Abs(RenderWindow.GetView().Size.X);
            set
            {
                value = Math.Clamp(value, ZOOM_MIN, ZOOM_MAX);
                var view = RenderWindow.GetView();
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
            get => RenderWindow.GetView().Rotation;
            set
            {
                var view = RenderWindow.GetView();
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
            get => RenderWindow.GetView().Size.X < 0;
            set
            {
                var view = RenderWindow.GetView();
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
            get => RenderWindow.GetView().Size.Y < 0;
            set
            {
                var view = RenderWindow.GetView();
                var size = view.Size;
                if (size.Y > 0 && value || size.Y < 0 && !value)
                    size.Y *= -1;
                view.Size = size;
                RenderWindow.SetView(view);
            }
        }

        public void StartPreview()
        {
            if (!backgroundWorker.IsBusy)
                backgroundWorker.RunWorkerAsync();
        }

        public void StopPreview()
        {
            if (backgroundWorker.IsBusy)
                backgroundWorker.CancelAsync();
        }

        private void SpinePreviewer_SizeChanged(object sender, EventArgs e)
        {
            if (RenderWindow is null)
                return;

            float parentX = Width;
            float parentY = Height;
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

        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void panel_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void panel_MouseWheel(object sender, MouseEventArgs e)
        {
            Zoom *= (e.Delta > 0 ? 1.1f : 0.9f);
            PropertyGrid?.Refresh();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RenderWindow.SetActive(true);

            while (!backgroundWorker.CancellationPending)
            {
                var delta = Clock.ElapsedTime.AsSeconds();
                Clock.Restart();

                // TODO: 绘制网格线
                RenderWindow.Clear(BackgroundColor);

                OnRenderFrame(delta);
                RenderWindow.Display();
            }

            RenderWindow.SetActive(false);
        }
    }

    /// <summary>
    /// RenderFrame 事件参数
    /// </summary>
    public class RenderFrameEventArgs : EventArgs
    {
        /// <summary>
        /// 渲染目标
        /// </summary>
        public SFML.Graphics.RenderTarget RenderTarget { get; }

        /// <summary>
        /// 距离上一帧经过的时间, 单位秒
        /// </summary>
        public float Delta {  get; }

        public RenderFrameEventArgs(SFML.Graphics.RenderTarget renderTarget, float delta)
        {
            RenderTarget = renderTarget;
            Delta = delta;
        }
    }
}
