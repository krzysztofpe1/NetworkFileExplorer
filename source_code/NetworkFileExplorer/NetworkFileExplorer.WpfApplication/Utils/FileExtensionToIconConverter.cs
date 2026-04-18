using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace NetworkFileExplorer.WpfApplication.Utils;

public class FileExtensionToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return ResourceHelper.GetImage("default.png");

        string? fileName = value.ToString();
        string? extension = Path.GetExtension(fileName)?.ToLower();

        switch (extension)
        {
            case ".js":
                return ResourceHelper.GetImage("file_js_closed.png");
            default:
                return ResourceHelper.GetImage("file_generic_closed.png");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}