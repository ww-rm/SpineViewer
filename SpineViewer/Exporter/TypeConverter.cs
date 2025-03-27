using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Exporter
{
    public class SFMLImageFileSuffixConverter : StringConverter
    {
        private readonly string[] supportedFileSuffix = [".png", ".jpg", ".tga", ".bmp"];

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            return new StandardValuesCollection(supportedFileSuffix);
        }
    }
}
