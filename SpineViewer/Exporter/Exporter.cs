using NLog;
using SpineViewer.Spine;
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
        protected SFMLImageVideoFrame GetFrame(Spine.Spine spine)
        {
            // tex 必须临时创建, 随用随取, 防止出现跨线程的情况
            using var tex = GetRenderTexture();
            tex.Clear(ExportArgs.BackgroundColor);
            tex.Draw(spine);
            tex.Display();
            return new(tex.Texture.CopyToImage());
        }

        /// <summary>
        /// 获取模型列表的单帧画面
        /// </summary>
        protected SFMLImageVideoFrame GetFrame(Spine.Spine[] spinesToRender)
        {
            // tex 必须临时创建, 随用随取, 防止出现跨线程的情况
            using var tex = GetRenderTexture();
            tex.Clear(ExportArgs.BackgroundColor);
            foreach (var spine in spinesToRender) tex.Draw(spine);
            tex.Display();
            return new(tex.Texture.CopyToImage());
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
