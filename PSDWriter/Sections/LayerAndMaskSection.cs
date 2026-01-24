using PSDWriter.Sections.Layers;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDWriter.Sections
{
    internal class LayerAndMaskSection
    {
        private readonly List<Layer> _layers = [];

        public List<Layer> Layers { get => _layers; }

        public void WriteTo(Stream stream)
        {
            if (_layers.Count >= 0x7fff)
                throw new ArgumentOutOfRangeException(nameof(_layers), "Too many layers");

            // 4 bytes (Section length)
            stream.WriteI32BE(4 + 2 + _layers.Select(it => it.RecordLength + it.ChannelDataLength).Sum() + 4);

            // 4 bytes (Length of layer info)
            stream.WriteI32BE(2 + _layers.Select(it => it.RecordLength + it.ChannelDataLength).Sum());

            // 2 bytes (Layer count)
            stream.WriteI16BE((short)_layers.Count);

            // 2n bytes (Layer records)
            foreach (var layer in _layers)
                layer.WriteRecordTo(stream);

            // 2n bytes (Layer channel data)
            foreach (var layer in _layers)
                layer.WriteChannelDataTo(stream);

            // 4 bytes (Global layer mask info)
            stream.WriteU32BE(0);

            // 0 bytes (Tagged blocks)
        }
    }
}
