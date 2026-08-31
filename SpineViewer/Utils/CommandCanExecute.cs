using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Utils
{
    public static class CommandCanExecute
    {
        public static bool NotNull([NotNullWhen(true)] object? v)
            => v is not null;

        public static bool MoreThanZero([NotNullWhen(true)] IList? v)
            => NotNull(v) && v.Count > 0;

        public static bool OnlyOne([NotNullWhen(true)] IList? v)
            => NotNull(v) && v.Count == 1;
    }
}
