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

        public bool DebugBounds { get; set; }

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
