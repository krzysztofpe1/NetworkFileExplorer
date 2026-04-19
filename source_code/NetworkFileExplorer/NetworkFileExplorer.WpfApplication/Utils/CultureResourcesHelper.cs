using NetworkFileExplorer.WpfApplication.Resources.CultureStrings;
using System.Globalization;
using System.Windows.Data;

namespace NetworkFileExplorer.WpfApplication.Utils;

public class CultureResourcesHelper
{
    private const string ResourcesFileName = "Strings";

    public Strings GetStringsInstance()
    {
        return new Strings();
    }

    private static ObjectDataProvider _provider;
    public static ObjectDataProvider ResourceProvider
    {
        get
        {
            if (_provider == null)
                _provider =
                (ObjectDataProvider)System.Windows.Application.Current.FindResource(ResourcesFileName);
            return _provider;
        }
    }
    public static void ChangeCulture(CultureInfo culture)
    {
        ResourceProvider.Refresh();
    }
}
