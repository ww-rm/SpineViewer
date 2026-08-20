using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Utils
{
    /// <summary>
    /// 隐私信息管理工具类
    /// </summary>
    public class Secrets
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly UTF8Encoding _utf8 = new(false, true);

        /// <summary>
        /// 使用本机用户上下文加密数据
        /// </summary>
        public static string Encrypt(string data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            byte[] plainData = _utf8.GetBytes(data);
            byte[] encryptedData = ProtectedData.Protect(plainData, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// 使用本机用户上下文解密数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string? Decrypt(string data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            try
            {
                byte[] encryptedData = Convert.FromBase64String(data);
                byte[] plainData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                return _utf8.GetString(plainData);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                _logger.Warn("Failed to decrypt data, {0}", ex.Message);
            }
            return null;
        }
    }
}
