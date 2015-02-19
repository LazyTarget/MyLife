using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MyLife.App
{
    public class NullOrEmptyToCollapsedValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var str = System.Convert.ToString(value);
            var isEmpty = string.IsNullOrEmpty(str);
            var res = isEmpty ? Visibility.Collapsed : Visibility.Visible;
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
