using PSDWriter.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDWriter
{
    public class PSDWriter
    {
        private readonly PSDFileHeaderSection _fileHeaderSection;
        private readonly PSDColorModeDataSection _colorModeDataSection;
        private readonly PSDImageResourcesSection _imageResourcesSection;
        private readonly PSDLayerAndMaskSection _layerAndMaskSection;
        private readonly PSDImageDataSection _imageDataSection;

        public PSDWriter(uint width, uint height)
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

        public void AddRgbaLayer(byte[] pixels, string name = null, bool preMultipliedAlpha = false)
        {
            if (string.IsNullOrEmpty(name))
                name = Guid.NewGuid().ToString()[..8];

            var layer = new PSDLayer(name, Width, Height);
            layer.SetRgbaImageData(pixels);
            _layerAndMaskSection.Layers.Add(layer);
        }

        public void WriteTo(Stream stream)
        {
            _fileHeaderSection.WriteTo(stream);
            _colorModeDataSection.WriteTo(stream);
            _imageResourcesSection.WriteTo(stream);
            _layerAndMaskSection.WriteTo(stream);
            _imageDataSection.WriteTo(stream);
        }

        public void WriteTo(string path)
        {
            using var stream = File.OpenWrite(path);
            WriteTo(stream);
        }
    }
}
