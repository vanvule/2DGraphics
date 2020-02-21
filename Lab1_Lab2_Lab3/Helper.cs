using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Paint
{
    public class PointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Point point = (Point)value;
            //int x = System.Convert.ToInt32(point.X);
            //int y = System.Convert.ToInt32(point.Y);
            int x = (int)point.X;
            int y = (int)point.Y;
            return $"{x}, {y}px";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
