using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter
{
    /// <summary>
    /// 导出器基类
    /// </summary>
    public abstract class Exporter(ExportArgs exportArgs) : ImplementationResolver<Exporter, ExportImplementationAttribute, ExportType>
    {
        /// <summary>
        /// 仅源像素混合模式
        /// </summary>
        private static readonly SFML.Graphics.BlendMode SrcOnlyBlendMode = new(SFML.Graphics.BlendMode.Factor.One, SFML.Graphics.BlendMode.Factor.Zero);

        /// <summary>
        /// 创建指定类型导出器
        /// </summary>
        /// <param name="exportType">导出类型</param>
        /// <param name="exportArgs">与 <paramref name="exportType"/> 匹配的导出参数</param>
        /// <returns>与 <paramref name="exportType"/> 匹配的导出器</returns>
        public static Exporter New(ExportType exportType, ExportArgs exportArgs) => New(exportType, [exportArgs]);

        /// <summary>
        /// 日志器
        /// </summary>
        protected Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 导出参数
        /// </summary>
        public ExportArgs ExportArgs { get; } = exportArgs;

        /// <summary>
        /// 可用于文件名的时间戳字符串
        /// </summary>
        protected readonly string timestamp = DateTime.Now.ToString("yyMMddHHmmss");

        /// <summary>
        /// 获取供渲染的 SFML.Graphics.RenderTexture
        /// </summary>
        private SFML.Graphics.RenderTexture GetRenderTexture()
        {
            var tex = new SFML.Graphics.RenderTexture((uint)ExportArgs.Resolution.Width, (uint)ExportArgs.Resolution.Height);
            tex.Clear(SFML.Graphics.Color.Transparent);
            tex.SetView(ExportArgs.View);
            return tex;
        }

        /// <summary>
        /// 获取单个模型的单帧画面
        /// </summary>
        protected SFMLImageVideoFrame GetFrame(Spine.Spine spine) => GetFrame([spine]);

        /// <summary>
        /// 获取模型列表的单帧画面
        /// </summary>
        protected SFMLImageVideoFrame GetFrame(Spine.Spine[] spinesToRender)
        {
            // RenderTexture 必须临时创建, 随用随取, 防止出现跨线程的情况
            using var texPma = GetRenderTexture();

            // 先将预乘结果准确绘制出来, 注意背景色也应当是预乘的
            texPma.Clear(ExportArgs.BackgroundColorPma);
            foreach (var spine in spinesToRender) texPma.Draw(spine);
            texPma.Display();

            // 背景色透明度不为 1 时需要处理反预乘, 否则直接就是结果
            if (ExportArgs.BackgroundColor.A < 255)
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
                st.BlendMode = SrcOnlyBlendMode; // 用源的颜色和透明度直接覆盖
                st.Shader = Shader.InversePma;

                // 在最终结果上二次渲染非预乘画面
                using var tex = GetRenderTexture();

                // 将非预乘结果覆盖式绘制在目标对象上, 注意背景色应该用非预乘的
                tex.Clear(ExportArgs.BackgroundColor);
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
        protected abstract void ExportSingle(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null);

        /// <summary>
        /// 每个模型独立导出
        /// </summary>
        protected abstract void ExportIndividual(Spine.Spine[] spinesToRender, BackgroundWorker? worker = null);

        /// <summary>
        /// 执行导出
        /// </summary>
        /// <param name="spines">要进行导出的 Spine 列表</param>
        /// <param name="worker">用来执行该函数的 worker</param>
        public virtual void Export(Spine.Spine[] spines, BackgroundWorker? worker = null)
        {
            var spinesToRender = spines.Where(sp => !ExportArgs.RenderSelectedOnly || sp.IsSelected).Reverse().ToArray();

            if (ExportArgs.ExportSingle) ExportSingle(spinesToRender, worker);
            else ExportIndividual(spinesToRender, worker);

            logger.LogCurrentProcessMemoryUsage();
        }
    }
}
