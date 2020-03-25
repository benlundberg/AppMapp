using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AppMapp
{
    public class BoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool val)
            {
                return val ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility val)
            {
                return val == Visibility.Visible ? true : false;
            }

            return false;
        }
    }
}
