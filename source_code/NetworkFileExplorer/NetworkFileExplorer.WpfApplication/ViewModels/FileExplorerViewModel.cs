using GalaSoft.MvvmLight;
using Microsoft.Win32;
using NetworkFileExplorer.WpfApplication.Resources.CultureStrings;
using System.Globalization;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

public class FileExplorerViewModel : ViewModelBase
{

    private static readonly string[] SupportedFileExtensions = { ".txt", ".ini", ".log", ".js" };

    public Utils.RelayCommand OpenRootFolderCommand { get; private set; }

    public Utils.RelayCommand SortRootFolderCommand { get; private set; }

    public Utils.RelayCommand OpenFileCommand { get; private set; }

    public DirectoryInfoViewModel? Root { get; set; }

    public event EventHandler<FileInfoViewModel>? OnOpenFileRequest;

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
        OpenRootFolderCommand = new Utils.RelayCommand(OpenRootFolderExecute);
        SortRootFolderCommand = new Utils.RelayCommand(SortRootFolderExecute, _ => Root != null);
        OpenFileCommand = new Utils.RelayCommand(OpenFileCommandExecute, OpenFileCommandCanExecute);
    }

    private bool OpenFileCommandCanExecute(object? parameter)
    {
        if(parameter is not FileInfoViewModel fileInfoVM)
            return false;

        return fileInfoVM.Model == null ? false : SupportedFileExtensions.Contains(fileInfoVM.Model.Extension);
    }

    private void OpenFileCommandExecute(object? obj)
    {
        if (obj is not FileInfoViewModel fileInfoVM)
            return;

        OnOpenFileRequest?.Invoke(this, fileInfoVM);
    }

    private void OpenRootFolderExecute(object? parameter)
    {
        var ofd = new OpenFolderDialog() { Title = Strings.SelectDirectoryToOpen };
        if (ofd.ShowDialog() != true)
            return;

        OpenRoot(ofd.FolderName);
    }

    private void SortRootFolderExecute(object? parameter)
    {
        if (Root == null)
            return;

        var sortOptions = Root.SortOptions;
        var sortDialog = new SortDialog(sortOptions);
        if (sortDialog.ShowDialog() != true)
            return;

        sortOptions = sortDialog.SortOptions;

        Root.Sort(sortOptions);
    }

    public void OpenRoot(string path)
    {
        Root = new DirectoryInfoViewModel() { Owner = this };
        Root.Open(path);
        RaisePropertyChanged(nameof(Root));
    }
}
