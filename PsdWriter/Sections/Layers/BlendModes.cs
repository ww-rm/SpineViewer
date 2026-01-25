using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdWriter.Sections.Layers
{
    public static class BlendModes
    {
        public const string PassThrough = "pass";
        public const string Normal = "norm";
        public const string Dissolve = "diss";

        public const string Darken = "dark";
        public const string Multiply = "mul ";
        public const string ColorBurn = "idiv";
        public const string LinearBurn = "lbrn";
        public const string DarkerColor = "dkCl";

        public const string Lighten = "lite";
        public const string Screen = "scrn";
        public const string ColorDodge = "div ";
        public const string LinearDodge = "lddg";
        public const string LighterColor = "lgCl";

        public const string Overlay = "over";
        public const string SoftLight = "sLit";
        public const string HardLight = "hLit";
        public const string VividLight = "vLit";
        public const string LinearLight = "lLit";
        public const string PinLight = "pLit";
        public const string HardMix = "hMix";

        public const string Difference = "diff";
        public const string Exclusion = "smud";
        public const string Subtract = "fsub";
        public const string Divide = "fdiv";

        public const string Hue = "hue ";
        public const string Saturation = "sat ";
        public const string Color = "colr";
        public const string Luminosity = "lum ";
    }
}
