using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SpineViewer.Utils
{
    public static class VisualTreeExtension
    {
        /// <summary>
        /// 向上查找指定类型的对象 (含源节点)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源节点</param>
        /// <param name="includeSource">是否包含源节点</param>
        public static T? GetParent<T>(this DependencyObject source, bool includeSource = false) where T : DependencyObject
        {
            if (!includeSource)
                source = VisualTreeHelper.GetParent(source);
            while (source is not null && source is not T)
                source = VisualTreeHelper.GetParent(source);
            return source as T;
        }
    }
}
