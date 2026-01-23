using PSDWriter;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSDWriter.Sections
{
    internal class ColorModeDataSection
    {
        public void WriteTo(Stream stream)
        {
            stream.WriteZeros(4);
        }
    }
}
