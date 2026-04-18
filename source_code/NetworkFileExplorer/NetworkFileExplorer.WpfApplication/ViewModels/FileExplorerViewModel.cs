using GalaSoft.MvvmLight;
using System.Globalization;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

class FileExplorerViewModel : ViewModelBase
{
    public DirectoryInfoViewModel? Root { get; set; }
    public string Lang
    {
        get { return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; }
        set
        {
            if (value != null)
                if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != value)
                {
                    CultureInfo.CurrentUICulture = new CultureInfo(value);
                    RaisePropertyChanged();
                }
        }
    }

    public FileExplorerViewModel()
    {
        RaisePropertyChanged(nameof(Lang));
    }

    public void OpenRoot(string path)
    {
        Root = new DirectoryInfoViewModel();
        Root.Open(path);
        RaisePropertyChanged(nameof(Root));
    }
}
