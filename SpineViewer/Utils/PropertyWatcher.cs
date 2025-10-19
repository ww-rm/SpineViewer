using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Utils
{
    public static class PropertyWatcher
    {
        public static IDisposable Watch(DependencyObject target, DependencyProperty property, Action callback)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(property, target.GetType());
            if (dpd == null) return null;

            EventHandler handler = (s, e) => callback();
            dpd.AddValueChanged(target, handler);

            return new Unsubscriber(() => dpd.RemoveValueChanged(target, handler));
        }

        private class Unsubscriber : IDisposable
        {
            private readonly Action _dispose;
            public Unsubscriber(Action dispose) => _dispose = dispose;
            public void Dispose() => _dispose();
        }
    }
}
