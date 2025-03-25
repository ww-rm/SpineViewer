using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        /// 导出参数
        /// </summary>
        public required ExportArgs ExportArgs { get; init; }

        /// <summary>
        /// 根据参数获取渲染目标
        /// </summary>
        protected SFML.Graphics.RenderTexture GetRenderTexture()
        {
            var tex = new SFML.Graphics.RenderTexture((uint)ExportArgs.Resolution.Width, (uint)ExportArgs.Resolution.Height);
            tex.Clear(SFML.Graphics.Color.Transparent);
            tex.SetView(ExportArgs.View);
            return tex;
        }

        /// <summary>
        /// 得到需要渲染的模型数组，并按渲染顺序排列
        /// </summary>
        protected Spine.Spine[] GetSpinesToRender(IEnumerable<Spine.Spine> spines)
        {
            return spines.Where(sp => !ExportArgs.RenderSelectedOnly || sp.IsSelected).Reverse().ToArray();
        }

        /// <summary>
        /// 执行导出
        /// </summary>
        /// <param name="spines">要进行导出的 Spine 列表</param>
        /// <param name="worker">用来执行该函数的 worker</param>
        public abstract void Export(IEnumerable<Spine.Spine> spines, BackgroundWorker? worker = null);
    }

    /// <summary>
    /// 单帧画面导出器
    /// </summary>
    public class FrameExporter : Exporter
    {
        public override void Export(IEnumerable<Spine.Spine> spines, BackgroundWorker? worker = null)
        {
            var args = (FrameExportArgs)ExportArgs;
            using var tex = GetRenderTexture();
            var spinesToRender = GetSpinesToRender(spines);
            var timestamp = DateTime.Now;

            int total = spinesToRender.Length;
            int success = 0;
            int error = 0;

            worker?.ReportProgress(0, $"已处理 0/{total}");
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    Program.Logger.Info("Export cancelled");
                    break;
                }

                var spine = spinesToRender[i];
                tex.Draw(spine);

                if (args.ExportSingle)
                {
                    // 导出单个则直接算成功, 在最后一次将整体导出
                    success++;
                    if (i >= total - 1)
                    {
                        tex.Display();

                        // 导出单个时必定提供输出文件夹
                        var filename = $"frame_{timestamp:yyMMddHHmmss}{args.FileSuffix}";
                        var savePath = Path.Combine(args.OutputDir, filename); 

                        try
                        {
                            using (var img = new Bitmap(tex.Texture.CopyToBitmap()))
                            {
                                img.SetResolution(args.DPI.Width, args.DPI.Height);
                                img.Save(savePath, args.ImageFormat);
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.Logger.Error(ex.ToString());
                            Program.Logger.Error("Failed to save single frame");
                        }
                    }
                }
                else
                {
                    // 逐个导出则立即渲染, 并且保存完之后需要清除画面
                    tex.Display();

                    // 逐个导出时如果提供了输出文件夹, 则全部导出到输出文件夹, 否则输出到各自的文件夹
                    var filename = $"{spine.Name}_{timestamp:yyMMddHHmmss}{args.FileSuffix}";
                    var savePath = args.OutputDir is null ? Path.Combine(spine.AssetsDir, filename) : Path.Combine(args.OutputDir, filename);
                    try
                    {
                        using (var img = new Bitmap(tex.Texture.CopyToBitmap()))
                        {
                            img.SetResolution(args.DPI.Width, args.DPI.Height);
                            img.Save(savePath, args.ImageFormat);
                        }
                        success++;
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error(ex.ToString());
                        Program.Logger.Error("Failed to save frame {}", spine.SkelPath);
                        error++;
                    }

                    tex.Clear(SFML.Graphics.Color.Transparent);
                }

                worker?.ReportProgress((int)((i + 1) * 100.0) / total, $"已处理 {i + 1}/{total}");
            }

            // 输出逐个导出的统计信息
            if (!args.ExportSingle)
            {
                if (error > 0)
                    Program.Logger.Warn("Frames save {} successfully, {} failed", success, error);
                else
                    Program.Logger.Info("{} frames saved successfully", success);
            }

            Program.LogCurrentMemoryUsage();
        }
    }

    /// <summary>
    /// 视频导出基类
    /// </summary>
    public abstract class VideoExporter : Exporter
    {
        /// <summary>
        /// 生成单个模型的帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(SFML.Graphics.RenderTexture tex, Spine.Spine spine, BackgroundWorker? worker = null)
        {
            var args = (VideoExportArgs)ExportArgs;
            float delta = 1f / args.FPS;
            int total = 1 + (int)(args.Duration * args.FPS); // 至少导出 1 帧

            spine.CurrentAnimation = spine.CurrentAnimation;
            worker?.ReportProgress(0, $"{spine.Name} 已处理 0/{total} 帧");
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    Program.Logger.Info("Export cancelled");
                    break;
                }

                tex.Clear(SFML.Graphics.Color.Transparent);
                tex.Draw(spine);
                spine.Update(delta);
                tex.Display();
                worker?.ReportProgress((int)((i + 1) * 100.0) / total, $"{spine.Name} 已处理 {i + 1}/{total} 帧");
                yield return tex.Texture.CopyToFrame();
            }
        }

        /// <summary>
        /// 生成多个模型的帧序列
        /// </summary>
        protected IEnumerable<SFMLImageVideoFrame> GetFrames(SFML.Graphics.RenderTexture tex, Spine.Spine[] spines, BackgroundWorker? worker = null)
        {
            var args = (VideoExportArgs)ExportArgs;
            float delta = 1f / args.FPS;
            int total = 1 + (int)(args.Duration * args.FPS); // 至少导出 1 帧

            foreach (var spine in spines) spine.CurrentAnimation = spine.CurrentAnimation;
            worker?.ReportProgress(0, $"已处理 0/{total} 帧");
            for (int i = 0; i < total; i++)
            {
                if (worker?.CancellationPending == true)
                {
                    Program.Logger.Info("Export cancelled");
                    break;
                }

                tex.Clear(SFML.Graphics.Color.Transparent);
                foreach (var spine in spines)
                {
                    tex.Draw(spine);
                    spine.Update(delta);
                }
                tex.Display();
                worker?.ReportProgress((int)((i + 1) * 100.0) / total, $"已处理 {i + 1}/{total} 帧");
                yield return tex.Texture.CopyToFrame();
            }
        }
    }

    /// <summary>
    /// 帧序列导出器
    /// </summary>
    public class FrameSequenceExporter : VideoExporter
    {
        public override void Export(IEnumerable<Spine.Spine> spines, BackgroundWorker? worker = null)
        {
            var args = (FrameSequenceExportArgs)ExportArgs;
            using var tex = GetRenderTexture();
            var spinesToRender = GetSpinesToRender(spines);
            var timestamp = DateTime.Now;

            if (args.ExportSingle)
            {
                int frameIdx = 0;
                foreach (var frame in GetFrames(tex, spinesToRender, worker))
                {
                    // 导出单个时必定提供输出文件夹
                    var filename = $"frames_{timestamp:yyMMddHHmmss}_{args.FPS:f0}_{frameIdx:d6}{args.FileSuffix}";
                    var savePath = Path.Combine(args.OutputDir, filename);

                    try
                    {
                        frame.SaveToFile(savePath);
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error(ex.ToString());
                        Program.Logger.Error("Failed to save frame {}", savePath);
                    }
                    finally
                    {
                        frame.Dispose();
                    }

                    frameIdx++;
                }
            }
            else
            {
                foreach (var spine in spinesToRender)
                {
                    if (worker?.CancellationPending == true) break; // 取消的日志在 GetFrames 里输出

                    // 如果提供了输出文件夹, 则全部导出到输出文件夹, 否则导出到各自的文件夹下
                    var subDir = $"{spine.Name}_{timestamp:yyMMddHHmmss}_{args.FPS:f0}";
                    var saveDir = args.OutputDir is null ? Path.Combine(spine.AssetsDir, subDir) : Path.Combine(args.OutputDir, subDir);
                    Directory.CreateDirectory(saveDir);

                    int frameIdx = 0;
                    foreach (var frame in GetFrames(tex, spine, worker))
                    {
                        var filename = $"{spine.Name}_{timestamp:yyMMddHHmmss}_{args.FPS:f0}_{frameIdx:d6}{args.FileSuffix}";
                        var savePath = Path.Combine(saveDir, filename);

                        try
                        {
                            frame.SaveToFile(savePath);
                        }
                        catch (Exception ex)
                        {
                            Program.Logger.Error(ex.ToString());
                            Program.Logger.Error("Failed to save frame {} {}", savePath, spine.SkelPath);
                        }
                        finally
                        {
                            frame.Dispose();
                        }
                        frameIdx++;
                    }
                }
            }

            Program.LogCurrentMemoryUsage();
        }
    }
}
