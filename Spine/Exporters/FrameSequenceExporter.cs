using NLog;
using SFML.System;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spine.Exporters
{
    /// <summary>
    /// 帧序列导出器, 导出 png 帧序列
    /// </summary>
    public class FrameSequenceExporter : VideoExporter
    {
        public FrameSequenceExporter(uint width = 100, uint height = 100) : base(width, height) { }
        public FrameSequenceExporter(Vector2u resolution) : base(resolution) { }

        /// <summary>
        /// PNG 有损压缩质量, 取值范围 0-100, 值越高质量越好文件越大
        /// 仅在系统可用 pngquant 时生效, 否则会回退到普通 PNG 导出
        /// </summary>
        public int PngQuality { get => _pngQuality; set => _pngQuality = Math.Clamp(value, 0, 100); }
        protected int _pngQuality = 85;

        /// <summary>
        /// pngquant 可执行文件路径。
        /// 为空时会优先尝试应用目录下的 bundled 文件, 再尝试从 PATH 查找。
        /// </summary>
        public string PngQuantPath
        {
            get => _pngQuantPath;
            set => _pngQuantPath = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
        protected string _pngQuantPath = string.Empty;

        /// <summary>
        /// 后台并行编码线程数
        /// </summary>
        public int EncodeWorkerCount { get => _encodeWorkerCount; set => _encodeWorkerCount = Math.Clamp(value, 1, 32); }
        protected int _encodeWorkerCount = Math.Clamp(Environment.ProcessorCount / 2, 1, 8);

        private int _pngQuantUnavailable = 0;

        private readonly record struct FrameEncodeJob(int Width, int Height, byte[] Pixels, string SavePath);
        private readonly record struct FrameQuantJob(string SavePath);

        public override void Export(string output, CancellationToken ct, params SpineObject[] spines)
        {
            Directory.CreateDirectory(output);

            int frameCount = GetFrameCount();
            int frameIdx = 0;
            var workerCount = Math.Clamp(_encodeWorkerCount, 1, Math.Max(1, Environment.ProcessorCount));
            var enablePngQuant = _pngQuality < 100;
            using var frameQueue = new BlockingCollection<FrameEncodeJob>(workerCount * 2);
            using var quantQueue = enablePngQuant ? new BlockingCollection<FrameQuantJob>(workerCount * 4) : null;
            var workers = new Task[workerCount];
            for (int i = 0; i < workerCount; i++)
                workers[i] = Task.Run(() => EncodeWorkerTask(frameQueue, quantQueue, ct), ct);
            var quantWorker = quantQueue is null
                ? null
                : Task.Run(() => QuantWorkerTask(quantQueue, ct), ct);

            _progressReporter?.Invoke(frameCount, 0, $"[0/{frameCount}] {output}");
            try
            {
                foreach (var frame in GetFrames(spines))
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.Info("Export cancelled");
                        frame.Dispose();
                        break;
                    }

                    var savePath = Path.Combine(output, $"frame_{_fps}_{frameIdx:d6}.png");
                    _progressReporter?.Invoke(frameCount, frameIdx + 1, $"[{frameIdx + 1}/{frameCount}] {savePath}");

                    try
                    {
                        // 复制像素数据后立刻释放 SFML 帧, 避免渲染和编码互相阻塞
                        var pixels = frame.Image.Pixels.ToArray();
                        frameQueue.Add(new(frame.Width, frame.Height, pixels, savePath), ct);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Info("Export cancelled");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex.ToString());
                        _logger.Error("Failed to queue frame {0}, {1}", savePath, ex.Message);
                    }
                    finally
                    {
                        frame.Dispose();
                    }

                    frameIdx++;
                }
            }
            finally
            {
                frameQueue.CompleteAdding();
                try
                {
                    Task.WaitAll(workers);
                }
                catch (AggregateException ex)
                {
                    foreach (var inner in ex.InnerExceptions.Where(e => e is not OperationCanceledException))
                    {
                        _logger.Debug(inner.ToString());
                        _logger.Error("Failed to export frame sequence worker, {0}", inner.Message);
                    }
                }

                if (quantQueue is not null)
                {
                    quantQueue.CompleteAdding();
                    try
                    {
                        quantWorker?.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var inner in ex.InnerExceptions.Where(e => e is not OperationCanceledException))
                        {
                            _logger.Debug(inner.ToString());
                            _logger.Error("Failed to optimize frame sequence, {0}", inner.Message);
                        }
                    }
                }
            }
        }

        private void EncodeWorkerTask(BlockingCollection<FrameEncodeJob> frameQueue, BlockingCollection<FrameQuantJob>? quantQueue, CancellationToken ct)
        {
            foreach (var job in frameQueue.GetConsumingEnumerable())
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    var info = new SKImageInfo(job.Width, job.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
                    using var skImage = SKImage.FromPixelCopy(info, job.Pixels);
                    using var pixmap = skImage.PeekPixels();
                    if (pixmap is null)
                    {
                        _logger.Error("Failed to encode frame {0}, unable to read pixmap", job.SavePath);
                        continue;
                    }

                    var options = new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, 9);
                    using (var stream = new SKFileWStream(job.SavePath))
                    {
                        if (!pixmap.Encode(stream, options))
                        {
                            _logger.Error("Failed to encode frame {0}, encoder returned false", job.SavePath);
                            continue;
                        }
                    }

                    if (quantQueue is not null && !ct.IsCancellationRequested)
                    {
                        try
                        {
                            quantQueue.Add(new(job.SavePath), ct);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.Info("Export cancelled");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Error("Failed to save frame {0}, {1}", job.SavePath, ex.Message);
                }
            }
        }

        private void QuantWorkerTask(BlockingCollection<FrameQuantJob> quantQueue, CancellationToken ct)
        {
            var minQuality = Math.Max(0, _pngQuality - 10);
            foreach (var job in quantQueue.GetConsumingEnumerable())
            {
                if (ct.IsCancellationRequested) break;
                TryRunPngQuant(job.SavePath, minQuality, _pngQuality);
            }
        }

        private bool TryRunPngQuant(string framePath, int minQuality, int maxQuality)
        {
            if (_pngQuantUnavailable != 0) return false;
            var tempPath = $"{framePath}.pngquant.tmp";
            var pngQuantPath = ResolvePngQuantPath();
            if (string.IsNullOrWhiteSpace(pngQuantPath))
            {
                if (Interlocked.Exchange(ref _pngQuantUnavailable, 1) == 0)
                    _logger.Warn("pngquant is not available, fallback to normal PNG output");
                return false;
            }

            var psi = new ProcessStartInfo
            {
                FileName = pngQuantPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("--force");
            psi.ArgumentList.Add("--skip-if-larger");
            psi.ArgumentList.Add("--strip");
            psi.ArgumentList.Add("--speed");
            psi.ArgumentList.Add("1");
            psi.ArgumentList.Add("--output");
            psi.ArgumentList.Add(tempPath);
            psi.ArgumentList.Add("--quality");
            psi.ArgumentList.Add($"{minQuality}-{maxQuality}");
            psi.ArgumentList.Add("--");
            psi.ArgumentList.Add(framePath);

            try
            {
                using var process = Process.Start(psi);
                if (process is null)
                {
                    _logger.Warn("Failed to start pngquant process, fallback to normal PNG output");
                    return false;
                }

                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    // 使用临时文件输出并带重试替换, 避免 Windows 下文件短暂占用导致失败
                    if (File.Exists(tempPath))
                    {
                        if (!TryMoveWithRetry(tempPath, framePath))
                        {
                            _logger.Warn("pngquant replacement failed for {0}, fallback to original PNG", framePath);
                            return false;
                        }
                    }
                    return true;
                }
                if (process.ExitCode is 98 or 99)
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                    return true; // skipped by pngquant rule
                }

                var errText = process.StandardError.ReadToEnd().Trim();
                _logger.Warn("pngquant failed with exit code {0}, fallback to normal PNG output", process.ExitCode);
                if (!string.IsNullOrWhiteSpace(errText))
                    _logger.Warn("pngquant stderr: {0}", errText);
                return false;
            }
            catch (Exception ex) when (ex is Win32Exception || ex is FileNotFoundException)
            {
                if (Interlocked.Exchange(ref _pngQuantUnavailable, 1) == 0)
                    _logger.Warn("pngquant is not available, fallback to normal PNG output");
                _logger.Debug(ex.ToString());
                return false;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                _logger.Warn("pngquant replacement failed for {0}, fallback to original PNG", framePath);
                _logger.Debug(ex.ToString());
                return false;
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        private string? ResolvePngQuantPath()
        {
            // 显式指定时直接使用 (可以是绝对路径或者 PATH 中的命令名)
            if (!string.IsNullOrWhiteSpace(_pngQuantPath))
                return _pngQuantPath;

            var baseDir = AppContext.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDir, "pngquant.exe"),
                Path.Combine(baseDir, "pngquant"),
                Path.Combine(baseDir, "Tools", "pngquant.exe"),
                Path.Combine(baseDir, "Tools", "pngquant"),
            };
            foreach (var path in candidates)
            {
                if (File.Exists(path)) return path;
            }

            return OperatingSystem.IsWindows() ? "pngquant.exe" : "pngquant";
        }

        private static bool TryMoveWithRetry(string sourcePath, string destinationPath, int maxRetry = 5)
        {
            for (int attempt = 0; attempt < maxRetry; attempt++)
            {
                try
                {
                    File.Move(sourcePath, destinationPath, true);
                    return true;
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    if (attempt >= maxRetry - 1) return false;
                    Thread.Sleep(25 * (attempt + 1));
                }
            }

            return false;
        }
    }
}
