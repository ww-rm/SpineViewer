using NLog;
using SpineViewer.Extensions;
using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.SpineExporter
{
    /// <summary>
    /// 导出器基类
    /// </summary>
    public abstract class Exporter : IDisposable
    {
        /// <summary>
        /// 日志器
        /// </summary>
        protected readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 可用于文件名的时间戳字符串
        /// </summary>
        protected readonly string timestamp = DateTime.Now.ToString("yyMMddHHmmss");

        ~Exporter() { Dispose(false); }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing) { View.Dispose(); }

        /// <summary>
        /// 输出文件夹
        /// </summary>
        public string? OutputDir { get; set; } = null;

        /// <summary>
        /// 导出单个
        /// </summary>
        public bool IsExportSingle { get; set; } = false;

        /// <summary>
        /// 画面分辨率
        /// </summary>
        public Size Resolution { get; set; } = new(100, 100);

        /// <summary>
        /// 渲染视窗, 接管对象生命周期
        /// </summary>
        public SFML.Graphics.View View { get => view; set { view.Dispose(); view = value; } }
        private SFML.Graphics.View view = new();

        /// <summary>
        /// 是否仅渲染选中
        /// </summary>
        public bool RenderSelectedOnly { get; set; } = false;

        /// <summary>
        /// 背景颜色
        /// </summary>
        public SFML.Graphics.Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                var bcPma = value;
                var a = bcPma.A / 255f;
                bcPma.R = (byte)(bcPma.R * a);
                bcPma.G = (byte)(bcPma.G * a);
                bcPma.B = (byte)(bcPma.B * a);
                BackgroundColorPma = bcPma;
            }
        }
        private SFML.Graphics.Color backgroundColor = SFML.Graphics.Color.Transparent;

        /// <summary>
        /// 预乘后的背景颜色
        /// </summary>
        public SFML.Graphics.Color BackgroundColorPma { get; private set; } = SFML.Graphics.Color.Transparent;

        /// <summary>
        /// 获取供渲染的 SFML.Graphics.RenderTexture
        /// </summary>
        private SFML.Graphics.RenderTexture GetRenderTexture()
        {
            var tex = new SFML.Graphics.RenderTexture((uint)Resolution.Width, (uint)Resolution.Height);
            tex.Clear(SFML.Graphics.Color.Transparent);
            tex.SetView(View);
            return tex;
        }

        /// <summary>
        /// 获取单个模型的单帧画面
        /// </summary>
        protected SFMLImageVideoFrame GetFrame(SpineObject spine) => GetFrame([spine]);

        /// <summary>
        /// 获取模型列表的单帧画面
        /// </summary>
        protected SFMLImageVideoFrame GetFrame(SpineObject[] spinesToRender)
        {
            // RenderTexture 必须临时创建, 随用随取, 防止出现跨线程的情况
            using var texPma = GetRenderTexture();

            // 先将预乘结果准确绘制出来, 注意背景色也应当是预乘的
            texPma.Clear(BackgroundColorPma);
            foreach (var spine in spinesToRender) texPma.Draw(spine);
            texPma.Display();

            // 背景色透明度不为 1 时需要处理反预乘, 否则直接就是结果
            if (BackgroundColor.A < 255)
            {
                // 从预乘结果构造渲染对象, 并正确设置变换
                using var view = texPma.GetView();
                using var img = texPma.Texture.CopyToImage();
                using var texSprite = new SFML.Graphics.Texture(img);
                using var sp = new SFML.Graphics.Sprite(texSprite)
                {
                    Origin = new(texPma.Size.X / 2f, texPma.Size.Y / 2f),
                    Position = new(view.Center.X, view.Center.Y),
                    Scale = new(view.Size.X / texPma.Size.X, view.Size.Y / texPma.Size.Y),
                    Rotation = view.Rotation
                };

                // 混合模式用直接覆盖的方式, 保证得到的图像区域是反预乘的颜色和透明度, 同时使用反预乘着色器
                var st = SFML.Graphics.RenderStates.Default;
                st.BlendMode = SFMLBlendMode.SourceOnly;
                st.Shader = SFMLShader.InversePma;

                // 在最终结果上二次渲染非预乘画面
                using var tex = GetRenderTexture();

                // 将非预乘结果覆盖式绘制在目标对象上, 注意背景色应该用非预乘的
                tex.Clear(BackgroundColor);
                tex.Draw(sp, st);
                tex.Display();
                return new(tex.Texture.CopyToImage());
            }
            else
            {
                return new(texPma.Texture.CopyToImage());
            }
        }

        /// <summary>
        /// 每个模型在同一个画面进行导出
        /// </summary>
        protected abstract void ExportSingle(SpineObject[] spinesToRender, BackgroundWorker? worker = null);

        /// <summary>
        /// 每个模型独立导出
        /// </summary>
        protected abstract void ExportIndividual(SpineObject[] spinesToRender, BackgroundWorker? worker = null);

        /// <summary>
        /// 检查参数是否合法并规范化参数值, 否则返回用户错误原因
        /// </summary>
        public virtual string? Validate()
        {
            if (!string.IsNullOrWhiteSpace(OutputDir) && File.Exists(OutputDir))
                return "输出文件夹无效";
            if (!string.IsNullOrWhiteSpace(OutputDir) && !Directory.Exists(OutputDir))
                return $"文件夹 {OutputDir} 不存在";
            if (IsExportSingle && string.IsNullOrWhiteSpace(OutputDir))
                return "导出单个时必须提供输出文件夹";

            OutputDir = string.IsNullOrWhiteSpace(OutputDir) ? null : Path.GetFullPath(OutputDir);
            return null;
        }

        /// <summary>
        /// 执行导出
        /// </summary>
        /// <param name="spines">要进行导出的 Spine 列表</param>
        /// <param name="worker">用来执行该函数的 worker</param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void Export(SpineObject[] spines, BackgroundWorker? worker = null)
        {
            if (Validate() is string err) 
                throw new ArgumentException(err);

            var spinesToRender = spines.Where(sp => !RenderSelectedOnly || sp.IsSelected).Reverse().ToArray();

            if (IsExportSingle) ExportSingle(spinesToRender, worker);
            else ExportIndividual(spinesToRender, worker);

            logger.LogCurrentProcessMemoryUsage();
        }
    }

    /// <summary>
    /// 用于在 PropertyGrid 上提供用户操作接口的包装类
    /// </summary>
    public class ExporterProperty(Exporter exporter)
    {
        [Browsable(false)]
        public virtual Exporter Exporter { get; } = exporter;

        /// <summary>
        /// 输出文件夹
        /// </summary>
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("[0] 导出"), DisplayName("输出文件夹"), Description("逐个导出时可以留空，将逐个导出到模型自身所在目录")]
        public string? OutputDir { get => Exporter.OutputDir; set => Exporter.OutputDir = value; }

        /// <summary>
        /// 导出单个
        /// </summary>
        [Category("[0] 导出"), DisplayName("导出单个"), Description("是否将模型在同一个画面上导出单个文件，否则逐个导出模型")]
        public bool IsExportSingle { get => Exporter.IsExportSingle; set => Exporter.IsExportSingle = value; }

        /// <summary>
        /// 画面分辨率
        /// </summary>
        [TypeConverter(typeof(SizeConverter))]
        [Category("[0] 导出"), DisplayName("分辨率"), Description("画面的宽高像素大小，请在预览画面参数面板进行调整")]
        public Size Resolution { get => Exporter.Resolution; }

        /// <summary>
        /// 渲染视窗
        /// </summary>
        [Category("[0] 导出"), DisplayName("视图"), Description("画面的视图参数，请在预览画面参数面板进行调整")]
        public SFML.Graphics.View View { get => Exporter.View; }

        /// <summary>
        /// 是否仅渲染选中
        /// </summary>
        [Category("[0] 导出"), DisplayName("仅渲染选中"), Description("是否仅导出选中的模型，请在预览画面参数面板进行调整")]
        public bool RenderSelectedOnly { get => Exporter.RenderSelectedOnly; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        [Editor(typeof(SFMLColorEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(SFMLColorConverter))]
        [Category("[0] 导出"), DisplayName("背景颜色"), Description("要使用的背景色, 格式为 #RRGGBBAA")]
        public SFML.Graphics.Color BackgroundColor { get => Exporter.BackgroundColor; set => Exporter.BackgroundColor = value; }
    }
}
