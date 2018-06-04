using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace digiCamControl.LightBox.Wpf
{
    public class StringToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as string;
            if (val != null)
            {
                PackIconKind icon;
                if (Enum.TryParse(val, out icon))
                    return icon;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
