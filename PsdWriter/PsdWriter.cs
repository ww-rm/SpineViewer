using PsdWriter.Sections;
using PsdWriter.Sections.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdWriter
{
    public class PsdWriter
    {
        private readonly FileHeaderSection _fileHeaderSection;
        private readonly ColorModeDataSection _colorModeDataSection;
        private readonly ImageResourcesSection _imageResourcesSection;
        private readonly LayerAndMaskSection _layerAndMaskSection;
        private readonly ImageDataSection _imageDataSection;

        private readonly Stack<string> _groupNames = [];

        public PsdWriter(uint width, uint height)
        {
            Width = width; 
            Height = height;
            _fileHeaderSection = new(width, height);
            _colorModeDataSection = new();
            _imageResourcesSection = new();
            _layerAndMaskSection = new();
            _imageDataSection = new(width, height);
        }

        public uint Width { get; }
        public uint Height { get; }

        public void AddRgbaLayer(byte[] pixels, string name = null, bool preMultipliedAlpha = false, string blendMode = BlendModes.Normal)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = Guid.NewGuid().ToString()[..8];

            var layer = new RgbaLayer(name, Width, Height) { BlendMode = blendMode };
            layer.SetRgbaImageData(pixels, preMultipliedAlpha);
            _layerAndMaskSection.Layers.Add(layer);
        }

        public void BeginGroup(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = Guid.NewGuid().ToString()[..8];

            var layer = new DividerLayer("</Layer group>", DividerTypes.BoundingSectionDivider);
            _layerAndMaskSection.Layers.Add(layer);
            _groupNames.Push(name);
        }

        public string EndGroup(bool openFolder = false)
        {
            if (_groupNames.Count <= 0)
                throw new IndexOutOfRangeException("No groups");

            var name = _groupNames.Pop();
            var layer = new DividerLayer(name, openFolder ? DividerTypes.OpenFolder : DividerTypes.ClosedFolder);
            _layerAndMaskSection.Layers.Add(layer);
            return name;
        }

        public void WriteTo(Stream stream)
        {
            while (_groupNames.Count > 0)
                EndGroup();

            _fileHeaderSection.WriteTo(stream);
            _colorModeDataSection.WriteTo(stream);
            _imageResourcesSection.WriteTo(stream);
            _layerAndMaskSection.WriteTo(stream);
            _imageDataSection.WriteTo(stream);
        }

        public void WriteTo(string path)
        {
            while (_groupNames.Count > 0)
                EndGroup();

            using var stream = File.OpenWrite(path);
            WriteTo(stream);
        }
    }
}
