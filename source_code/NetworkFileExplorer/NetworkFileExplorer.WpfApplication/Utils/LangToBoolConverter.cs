using System.Globalization;
using System.Windows.Data;

namespace NetworkFileExplorer.WpfApplication.Utils;

internal class LangToBoolConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((string)value == (string)parameter)
            return true;
        return false;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value == true)
            return (string)parameter;
        throw new ArgumentException(nameof(value));
    }
}
