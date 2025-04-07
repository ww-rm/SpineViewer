using SpineViewer.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers
{
    /// <summary>
    /// 用于在 PropertyGrid 上显示 SpinePreviewe 属性的包装类
    /// </summary>
    public class SpinePreviewerWrapper(SpinePreviewer previewer)
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
