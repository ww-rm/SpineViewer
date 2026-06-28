using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpineViewer.Behaviors
{
    public static class ClearSelectionOnBlankBehavior
    {
        public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(ClearSelectionOnBlankBehavior),
            new PropertyMetadata(false, OnEnableChanged)
        );

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);

        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox list)
                return;

            if ((bool)e.NewValue)
                list.MouseLeftButtonDown += OnMouseLeftButtonDown;
            else
                list.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var list = (ListBox)sender;

            if (e.OriginalSource is not DependencyObject source)
                return;

            // 点击到了一个 ListBoxItem，不处理
            if (source.GetParent<ListBoxItem>(true) != null)
                return;

            list.UnselectAll();
        }
    }
}
