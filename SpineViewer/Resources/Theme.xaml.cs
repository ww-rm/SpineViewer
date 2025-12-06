using SpineViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpineViewer.Resources
{
    public partial class Theme : ResourceDictionary
    {
        private void ListView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var list = (ListView)sender;
            if (((DependencyObject)e.OriginalSource)?.GetParent<ListViewItem>(true) is null)
                list.SelectedItems.Clear();
        }

        private void ListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var list = (ListBox)sender;
            if (((DependencyObject)e.OriginalSource)?.GetParent<ListBoxItem>(true) is null)
                list.SelectedItems.Clear();
        }
    }
}
