using System.DirectoryServices;
using System.Globalization;
using System.Windows.Data;

namespace NetworkFileExplorer.WpfApplication.Utils.Converters;

public class SortDirectionToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SortDirection sortDirection && parameter is string paramString && Enum.TryParse<SortDirection>(paramString, out var result))
            return sortDirection == result;

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is bool isChecked && isChecked && parameter is string paramString && Enum.TryParse<SortDirection>(paramString, out var result))
            return result;

        return Binding.DoNothing;
    }
}
