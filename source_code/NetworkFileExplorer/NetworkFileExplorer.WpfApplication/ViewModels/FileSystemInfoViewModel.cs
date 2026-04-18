using GalaSoft.MvvmLight;
using System.IO;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

class FileSystemInfoViewModel : ViewModelBase
{
    public DateTime? LastWriteTime
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            RaisePropertyChanged();
        }
    }

    public string? Caption
    {
        get;
        set
        {
            if(value == field)
                return;
            field = value;
            RaisePropertyChanged();
        }
    }

    public FileSystemInfo? Model
    {
        get;
        set
        {
            if(value == field)
                return;
            field = value;
            RaisePropertyChanged();
        }
    }
}
