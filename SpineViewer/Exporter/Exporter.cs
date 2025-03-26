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
    public abstract class Exporter
    {
        /// <summary>
        /// 实现类缓存
        /// </summary>
        private static readonly Dictionary<ExportType, Type> ImplementationTypes = [];

        static Exporter()
        {
            var impTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(Exporter).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in impTypes)
            {
                var attr = type.GetCustomAttribute<ExportImplementationAttribute>();
                if (attr is not null)
                {
                    if (ImplementationTypes.ContainsKey(attr.ExportType))
                        throw new InvalidOperationException($"Multiple implementations found: {attr.ExportType}");
                    ImplementationTypes[attr.ExportType] = type;
                }
            }
            Program.Logger.Debug("Find exporter implementations: [{}]", string.Join(", ", ImplementationTypes.Keys));
        }

        /// <summary>
        /// 创建指定类型导出参数
        /// </summary>
        public static Exporter New(ExportType exportType, ExportArgs exportArgs)
        {
            if (!ImplementationTypes.TryGetValue(exportType, out var type))
            {
                throw new NotImplementedException($"Not implemented type: {exportType}");
            }
            return (Exporter)Activator.CreateInstance(type, exportArgs);
        }

        /// <summary>
        /// 导出参数
        /// </summary>
        public ExportArgs ExportArgs { get; }

        /// <summary>
        /// 可用于文件名的时间戳字符串
        /// </summary>
        protected readonly string timestamp;

        public Exporter(ExportArgs exportArgs)
        {
            ExportArgs = exportArgs;
            timestamp = DateTime.Now.ToString("yyMMddHHmmss");
        }

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
            tex.Clear(SFML.Graphics.Color.Transparent);
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
            tex.Clear(SFML.Graphics.Color.Transparent);
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

            Program.LogCurrentMemoryUsage();
        }
    }
}
