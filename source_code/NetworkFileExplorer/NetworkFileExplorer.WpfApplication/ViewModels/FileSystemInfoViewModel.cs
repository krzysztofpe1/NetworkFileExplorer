using GalaSoft.MvvmLight;
using System.IO;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

public class FileSystemInfoViewModel : ViewModelBase
{
    public required ViewModelBase Owner { get; init; }
    public FileExplorerViewModel? OwnerExplorer
    {
        get
        {
            ViewModelBase owner = Owner;

            while(owner is DirectoryInfoViewModel directoryInforVM)
                owner = directoryInforVM.Owner;

            return owner is FileExplorerViewModel fileExplorerVM ? fileExplorerVM : null;
        }
    }

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

    public bool IsSelected
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            RaisePropertyChanged();
        }
    } = false;

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
