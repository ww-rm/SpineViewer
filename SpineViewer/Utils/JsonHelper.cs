using Microsoft.Win32;
using NLog;
using SpineViewer.Models;
using SpineViewer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpineViewer.Utils
{
    public static class JsonHelper
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 保存 Json 文件的格式参数
        /// </summary>
        public static JsonSerializerOptions JsonOptions => _jsonOptions;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// 从文件反序列对象, 不会抛出异常
        /// </summary>
        public static bool Deserialize<T>(string path, out T obj)
        {
            if (!File.Exists(path))
            {
                _logger.Error("Json file {0} not found", path);
                MessagePopupService.Error($"Json file {path} not found");
            }
            else
            {
                try
                {
                    var json = File.ReadAllText(path, Encoding.UTF8);
                    var model = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                    if (model is T m)
                    {
                        obj = m;
                        return true;
                    }
                    _logger.Error("Null data in file {0}", path);
                    MessagePopupService.Error($"Null data in file {path}");
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to read json file {0}, {1}", path, ex.Message);
                    _logger.Trace(ex.ToString());
                    MessagePopupService.Error($"Failed to read json file {path}, {ex.ToString()}");
                }
            }
            obj = default;
            return false;
        }

        /// <summary>
        /// 保存至文件, 不会抛出异常
        /// </summary>
        public static bool Serialize<T>(T obj, string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var json = JsonSerializer.Serialize(obj, _jsonOptions);
                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save json file {0}, {1}", path, ex.Message);
                _logger.Trace(ex.ToString());
                MessagePopupService.Error($"Failed to save json file {path}, {ex.ToString()}");
                return false;
            }
            return true;
        }
    }
}
