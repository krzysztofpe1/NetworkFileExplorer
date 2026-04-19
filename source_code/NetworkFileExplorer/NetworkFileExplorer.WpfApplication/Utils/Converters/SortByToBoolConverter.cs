using NetworkFileExplorer.WpfApplication.DataModels;
using System.Globalization;
using System.Windows.Data;

namespace NetworkFileExplorer.WpfApplication.Utils.Converters;

public class SortByToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SortType sortType && parameter is string paramString && Enum.TryParse<SortType>(paramString, out var result))
            return sortType == result;

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string paramString && Enum.TryParse<SortType>(paramString, out var result))
            return result;

        return Binding.DoNothing;
    }
}
