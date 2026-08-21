using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Utils
{
    public class Secrets
    {
        /// <summary>
        /// <see cref="DataProtectionScope.CurrentUser"/>
        /// </summary>
        public static Secrets User { get; } = new(DataProtectionScope.CurrentUser);

        /// <summary>
        /// <see cref="DataProtectionScope.LocalMachine"/>
        /// </summary>
        public static Secrets Local { get; } = new(DataProtectionScope.LocalMachine);

        private static readonly UTF8Encoding _utf8 = new(false, true);

        private readonly DataProtectionScope _scope;

        private Secrets(DataProtectionScope scope)
        {
            _scope = scope;
        }

        /// <summary>
        /// 加密数据
        /// </summary>
        public string Encrypt(string data)
        {
            ArgumentException.ThrowIfNullOrEmpty(data, nameof(data));
            byte[] plainData = _utf8.GetBytes(data);
            byte[] encryptedData = ProtectedData.Protect(plainData, null, _scope);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        public string Decrypt(string data)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(data, nameof(data));
            byte[] encryptedData = Convert.FromBase64String(data);
            byte[] plainData = ProtectedData.Unprotect(encryptedData, null, _scope);
            return _utf8.GetString(plainData);
        }
    }
}
