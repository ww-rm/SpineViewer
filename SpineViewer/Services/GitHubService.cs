using NLog;
using Octokit;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Services
{
    /// <summary>
    /// 提供 GitHub API 等功能
    /// </summary>
    public static class GitHubService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(30);
        private static readonly ProductHeaderValue _productHeaderValue = new(App.AppName, App.Version);

        private static Credentials _credentials = Credentials.Anonymous;
        private static WebProxy? _webProxy;

        private static GitHubClient? _systemClient;
        private static GitHubClient? _customClient;

        /// <summary>
        /// 设置客户端的访问令牌, 使用空值清除访问令牌
        /// </summary>
        public static string? Token
        {
            get => _credentials.Password;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (_credentials.AuthenticationType == AuthenticationType.Anonymous)
                        return;

                    _systemClient = null;
                    _customClient = null;
                    _credentials = Credentials.Anonymous;
                }
                else
                {
                    if (_credentials.Password == value)
                        return;

                    _systemClient = null;
                    _customClient = null;
                    _credentials = new(value);
                }
            }
        }

        /// <summary>
        /// 获取客户端对象, 不可持久保存, 随用随取
        /// </summary>
        public static GitHubClient GetClient()
        {
            switch (App.ProxyUri)
            {
                case Uri proxyUri:
                    // 代理发生变化就要重新创建连接
                    if (_webProxy?.Address != proxyUri)
                    {
                        _customClient = null;
                        _webProxy = new(proxyUri);
                    }
                    return _customClient ??= GetCustomClient();
                default:
                    return _systemClient ??= GetSystemClient();
            }
        }

        private static GitHubClient GetSystemClient()
        {
            var client = new GitHubClient(_productHeaderValue) { Credentials = _credentials };
            client.SetRequestTimeout(_requestTimeout);
            return client;
        }

        private static GitHubClient GetCustomClient()
        {
            var httpClient = new HttpClientAdapter(GetProxyMessageHandler);
            var connection = new Connection(_productHeaderValue, httpClient);
            var client = new GitHubClient(connection) { Credentials = _credentials };
            client.SetRequestTimeout(_requestTimeout);
            return client;
        }

        private static HttpMessageHandler GetProxyMessageHandler() => HttpMessageHandlerFactory.CreateDefault(_webProxy);
    }
}
