using FFMpegCore;
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpineViewer.Utils
{
    public static class JsonHelper
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static JsonHelper()
        {
            _jsonOptions.Converters.Add(new ColorJsonConverter());
        }

        /// <summary>
        /// 保存 Json 文件的格式参数
        /// </summary>
        public static JsonSerializerOptions JsonOptions => _jsonOptions;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            IndentSize = 4,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// 从文件反序列对象, 不会抛出异常
        /// </summary>
        public static bool Deserialize<T>(string path, out T obj, bool quietForNotExist = false)
        {
            if (!File.Exists(path))
            {
                if (!quietForNotExist)
                {
                    _logger.Error("Json file {0} not found", path);
                }
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
                }
                catch (Exception ex)
                {
                    _logger.Trace(ex.ToString());
                    _logger.Error("Failed to read json file {0}, {1}", path, ex.Message);
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
                _logger.Trace(ex.ToString());
                _logger.Error("Failed to save json file {0}, {1}", path, ex.Message);
                return false;
            }
            return true;
        }

        public static string Serialize<T>(T obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.Trace(ex.ToString());
                _logger.Error("Failed to serialize json object {0}", ex.Message);
                return string.Empty;
            }
        }
    }

    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 解析 JSON 对象
            var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
            var r = jsonObject.GetProperty("R").GetByte();
            var g = jsonObject.GetProperty("G").GetByte();
            var b = jsonObject.GetProperty("B").GetByte();
            var a = jsonObject.GetProperty("A").GetByte();
            return Color.FromArgb(a, r, g, b);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("R", value.R);
            writer.WriteNumber("G", value.G);
            writer.WriteNumber("B", value.B);
            writer.WriteNumber("A", value.A);
            writer.WriteEndObject();
        }
    }
}
