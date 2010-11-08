using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace WinMilk.Gui.Controls
{
    public sealed class BooleanToScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else if (value is bool?)
            {
                var nullable = (bool?)value;
                flag = nullable.GetValueOrDefault();
            }
            if (parameter != null)
            {
                if (bool.Parse((string)parameter))
                {
                    flag = !flag;
                }
            }
            if (flag)
            {
                return ScrollBarVisibility.Auto;
            }
            else
            {
                return ScrollBarVisibility.Disabled;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
