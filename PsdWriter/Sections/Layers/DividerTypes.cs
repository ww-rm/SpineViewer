using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdWriter.Sections.Layers
{
    internal static class DividerTypes
    {
        public const uint Other = 0;
        public const uint OpenFolder = 1;
        public const uint ClosedFolder = 2;
        public const uint BoundingSectionDivider = 3;
    }
}
