using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SpineViewer.Services
{
    public static class DownloadService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 超时时间, 用于小文件下载的话, 超时间不需要太长
        /// </summary>
        private static readonly TimeSpan _requestTimeout = TimeSpan.FromMinutes(5);
        private static readonly ProductInfoHeaderValue _userAgent = new(App.AppName, App.Version);

        private static WebProxy? _webProxy;

        private static HttpClient? _systemClient;
        private static HttpClient? _customClient;

        /// <summary>
        /// 下载文件至本地
        /// </summary>
        public static async Task<bool> DownloadAsync(string url, string path, CancellationToken ct = default)
        {
            try
            {
                var client = GetClient();

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);

                response.EnsureSuccessStatusCode();

                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                var tempPath = path + ".download";

                try
                {
                    await using var input = await response.Content.ReadAsStreamAsync(ct);
                    await using (var output = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await input.CopyToAsync(output, ct);
                        await output.FlushAsync(ct);
                    }
                    File.Move(tempPath, path, true);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }

                return true;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Error("Failed to download {0} to {1}, {2}", url, path, ex.Message);

                return false;
            }
        }

        /// <summary>
        /// 获取 HTTP 客户端, 不可持久保存, 随用随取
        /// </summary>
        private static HttpClient GetClient()
        {
            switch (App.ProxyUri)
            {
                case Uri proxyUri:
                    // 代理发生变化就要重新创建客户端
                    if (_webProxy?.Address != proxyUri)
                    {
                        _customClient?.Dispose();
                        _customClient = null;
                        _webProxy = new(proxyUri);
                    }
                    return _customClient ??= GetCustomClient();
                default:
                    return _systemClient ??= GetSystemClient();
            }
        }

        private static HttpClient GetSystemClient()
        {
            var client = new HttpClient { Timeout = _requestTimeout };
            client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
            return client;
        }

        private static HttpClient GetCustomClient()
        {
            var handler = new HttpClientHandler()
            {
                Proxy = _webProxy,
                UseProxy = true
            };

            var client = new HttpClient(handler) { Timeout = _requestTimeout };
            client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
            return client;
        }
    }
}