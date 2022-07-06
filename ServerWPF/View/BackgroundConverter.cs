using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ServerWPF.View
{
    public class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo
         culture)
        {
            bool isBanned = (bool)value;

            if (isBanned)
            {
                return (SolidColorBrush)new BrushConverter().ConvertFrom("#FFE4BBFF");

            }
            else
            {
                return (SolidColorBrush)new BrushConverter().ConvertFrom("#FFDDF4FF");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
