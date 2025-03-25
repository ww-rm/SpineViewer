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
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
        {
            // 支持标准值列表
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
        {
            // 排他模式，只有下拉列表中的值可选
            return true;
        }

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
        {
            return new StandardValuesCollection(supportedFileSuffix);
        }
    }
}
