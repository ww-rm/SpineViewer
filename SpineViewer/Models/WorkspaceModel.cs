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
    public class WorkspaceModel
    {
        public string? ExploringDirectory { get; set; }
        public RendererWorkspaceConfigModel RendererConfig { get; set; } = new();
        public List<SpineObjectWorkspaceConfigModel> LoadedSpineObjects { get; set; } = [];
    }

    public class RendererWorkspaceConfigModel
    {
        public uint ResolutionX { get; set; } = 100;

        public uint ResolutionY { get; set; } = 100;

        public float CenterX { get; set; }

        public float CenterY { get; set; }

        public float Zoom { get; set; } = 1f;

        public float Rotation { get; set; }

        public bool FlipX { get; set; }

        public bool FlipY { get; set; } = true;

        public uint MaxFps { get; set; } = 30;

        public bool ShowAxis { get; set; } = true;

        public Color BackgroundColor { get; set; }

        // TODO: 背景图片
        //public string? BackgroundImagePath { get; set; }

        //public ? BackgroundImageDisplayMode { get; set; }
    }

    public class SpineObjectWorkspaceConfigModel
    {
        public string SkelPath { get; set; } = "";
        public string AtlasPath { get; set; } = "";
        public bool IsShown { get; set; } = true;
        public SpineObjectConfigModel ObjectConfig { get; set; } = new();
    }

}
