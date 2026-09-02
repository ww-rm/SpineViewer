using NLog;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Extensions
{
    public static class OctokitExtension
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void LogRateLimit(this GitHubClient client)
        {
            if (client.GetLastApiInfo()?.RateLimit is RateLimit r)
            {
                _logger.Info("GitHub API RateLimit: {0}/{1}, Reset at: {2}", r.Remaining, r.Limit, r.Reset.LocalDateTime);
            }
            else
            {
                _logger.Info("GitHub API has not been used yet.");
            }
        }
    }
}
