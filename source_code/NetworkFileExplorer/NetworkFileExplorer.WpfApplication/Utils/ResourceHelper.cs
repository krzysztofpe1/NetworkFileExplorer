using System.Windows.Media.Imaging;

namespace NetworkFileExplorer.WpfApplication.Utils;

internal static class ResourceHelper
{
    public static BitmapImage GetImage(string fileName)
    {
        return new BitmapImage(
            new Uri($"pack://application:,,,/Resources/Images/{fileName}", UriKind.Absolute));
    }
}
