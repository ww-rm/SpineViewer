using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdWriter.Sections.Layers
{
    internal class DividerLayer : Layer
    {
        public static class DividerTypes
        {
            public const uint Other = 0;
            public const uint OpenFolder = 1;
            public const uint ClosedFolder = 2;
            public const uint BoundingSectionDivider = 3;
        }

        public DividerLayer(string name, uint dividerType) : base(name, 0, 0)
        {
            using (var ms = new MemoryStream())
            {
                // 16 bytes (Section divider setting)
                ms.Write(Encoding.ASCII.GetBytes("8BIMlsct"));
                ms.WriteU32BE(4);
                ms.WriteU32BE(dividerType);
                _additionalInfo.Add(ms.ToArray());
            }

            _channelDataA = [0, 0];
            _channelDataR = [0, 0];
            _channelDataG = [0, 0];
            _channelDataB = [0, 0];
        }
    }
}
