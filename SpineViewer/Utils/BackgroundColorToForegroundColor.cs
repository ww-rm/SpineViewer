using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SpineViewer.Utils
{
    public class BackgroundToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = Colors.White;
            if (value is SolidColorBrush brush)
            {
                color = brush.Color;
            }
            else if (value is Color c)
            {
                color = c;
            }

            if (color.A < 128)
                return Brushes.Black;

            // 计算亮度 (使用标准加权公式)
            double brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            return brightness < 0.5 ? Brushes.White : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
