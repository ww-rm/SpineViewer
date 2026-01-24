using PsdWriter;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdWriter.Sections
{
    internal class ImageResourcesSection
    {
        public void WriteTo(Stream stream)
        {
            stream.WriteZeros(4);
        }
    }
}
