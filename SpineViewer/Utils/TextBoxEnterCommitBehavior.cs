using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpineViewer.Utils
{
    /// <summary>
    /// TextBox 使用 Enter 提交内容附加行为
    /// </summary>
    public static class TextBoxEnterCommitBehavior
    {
        public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(TextBoxEnterCommitBehavior),
            new PropertyMetadata(false, OnChanged)
        );

        public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb)
                return;

            if ((bool)e.NewValue)
                tb.KeyDown += OnKeyDown;
            else
                tb.KeyDown -= OnKeyDown;
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (sender is not TextBox tb)
                return;

            tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            e.Handled = true;
        }
    }
}
