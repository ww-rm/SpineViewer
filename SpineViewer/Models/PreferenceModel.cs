using CommunityToolkit.Mvvm.ComponentModel;
using Spine.SpineWrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpineViewer.Models
{
    /// <summary>
    /// 首选项参数模型, 用于对话框修改以及本地保存
    /// </summary>
    public partial class PreferenceModel : ObservableObject
    {
        #region 纹理加载首选项

        [ObservableProperty]
        private bool _forcePremul;

        [ObservableProperty]
        private bool _forceNearest;

        [ObservableProperty]
        private bool _forceMipmap;

        #endregion

        #region 模型加载首选项

        [ObservableProperty]
        private bool _usePma;

        [ObservableProperty]
        private bool _debugTexture = true;

        [ObservableProperty]
        private bool _debugBounds;

        [ObservableProperty]
        private bool _debugBones;

        [ObservableProperty]
        private bool _debugRegions;

        [ObservableProperty]
        private bool _debugMeshHulls;

        [ObservableProperty]
        private bool _debugMeshes;

        [ObservableProperty]
        private bool _debugBoundingBoxes;

        [ObservableProperty]
        private bool _debugPaths;

        [ObservableProperty]
        private bool _debugPoints;

        [ObservableProperty]
        private bool _debugClippings;

        #endregion

        #region 序列化与反序列

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
        public static PreferenceModel Deserialize(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Preference file not found", path);
            var json = File.ReadAllText(path, Encoding.UTF8);
            var model = JsonSerializer.Deserialize<PreferenceModel>(json, _jsonOptions);
            return model ?? throw new JsonException($"null data in file '{path}'");
        }

        /// <summary>
        /// 保存至文件, 可能抛出异常
        /// </summary>
        public void Serialize(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var json = JsonSerializer.Serialize(this, _jsonOptions);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        #endregion
    }
}
