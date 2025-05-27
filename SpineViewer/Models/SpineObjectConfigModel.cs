using Spine.SpineWrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SpineViewer.Models
{
    public class SpineObjectConfigModel
    {
        /// <summary>
        /// 保存 Json 文件的格式参数
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// 从文件反序列对象, 可能抛出异常
        /// </summary>
        public static SpineObjectConfigModel Deserialize(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Config file not found", path);
            var json = File.ReadAllText(path, Encoding.UTF8);
            var model = JsonSerializer.Deserialize<SpineObjectConfigModel>(json, _jsonOptions);
            return model ?? throw new JsonException($"null data in file '{path}'");
        }

        /// <summary>
        /// 保存预设至文件, 概率抛出异常
        /// </summary>
        public void Serialize(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var json = JsonSerializer.Serialize(this, _jsonOptions);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        public bool UsePma { get; set; }

        public string Physics { get; set; } = ISkeleton.Physics.Update.ToString();

        public float Scale { get; set; } = 1f;

        public bool FlipX { get; set; }

        public bool FlipY { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public List<string> LoadedSkins { get; set; } = [];

        public Dictionary<string, string?> SlotAttachment { get; set; } = [];

        public List<string?> Animations { get; set; } = [];

        public bool DebugTexture { get; set; } = true;

        public bool DebugBounds { get; set; } = true;

        public bool DebugBones { get; set; }

        public bool DebugRegions { get; set; }

        public bool DebugMeshHulls { get; set; }

        public bool DebugMeshes { get; set; }

        public bool DebugBoundingBoxes { get; set; }

        public bool DebugPaths { get; set; }

        public bool DebugPoints { get; set; }

        public bool DebugClippings { get; set; }
    }
}
