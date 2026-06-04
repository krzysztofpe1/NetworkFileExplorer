using GalaSoft.MvvmLight;
using Microsoft.Win32;
using NetworkFileExplorer.WpfApplication.Resources.CultureStrings;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

public class FileExplorerViewModel : ViewModelBase
{

    private static readonly string[] SupportedFileExtensions = { ".txt", ".ini", ".log", ".js" };

    public Utils.RelayCommand OpenRootFolderCommand { get; private set; }

    public Utils.RelayCommand SortRootFolderCommand { get; private set; }

    public Utils.RelayCommand CancelSortCommand { get; private set; }

    public Utils.RelayCommand OpenFileCommand { get; private set; }

    public DirectoryInfoViewModel? Root { get; set; }

    public event EventHandler<FileInfoViewModel>? OnOpenFileRequest;

    // Zadanie 5: token source used to cancel the currently running sort operation.
    private CancellationTokenSource? _sortCancellationTokenSource;

    // True while a sort operation is in progress - drives the Cancel button visibility (Zadanie 5).
    public bool IsSorting
    {
        get;
        private set
        {
            if (field == value)
                return;
            field = value;
            RaisePropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

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

    public string StatusMessage
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            RaisePropertyChanged(nameof(StatusMessage));
        }
    }

    public FileExplorerViewModel()
    {
        RaisePropertyChanged(nameof(Lang));
        OpenRootFolderCommand = new Utils.RelayCommand(OpenRootFolderExecuteAsync);
        SortRootFolderCommand = new Utils.RelayCommand(SortRootFolderExecuteAsync, _ => Root != null && !IsSorting);
        CancelSortCommand = new Utils.RelayCommand(CancelSortExecute, _ => IsSorting);
        OpenFileCommand = new Utils.RelayCommand(OpenFileCommandExecute, OpenFileCommandCanExecute);
        StatusMessage = String.Empty;
    }

    private void Root_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(StatusMessage) && sender is FileSystemInfoViewModel fileSysInfoVM)
            this.StatusMessage = fileSysInfoVM.StatusMessage;
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

    private async void OpenRootFolderExecuteAsync(object? parameter)
    {
        var ofd = new OpenFolderDialog() { Title = Strings.SelectDirectoryToOpen };
        if (ofd.ShowDialog() != true)
            return;

        await Task.Factory.StartNew(() =>
        {
            OpenRoot(ofd.FolderName);
            StatusMessage = Strings.Ready;
        });
    }

    private async void SortRootFolderExecuteAsync(object? parameter)
    {
        if (Root == null || IsSorting)
            return;

        var sortOptions = Root.SortOptions;
        var sortDialog = new SortDialog(sortOptions);
        if (sortDialog.ShowDialog() != true)
            return;

        sortOptions = sortDialog.SortOptions;

        // Zadanie 5: create a fresh cancellation token for this sort operation.
        _sortCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _sortCancellationTokenSource.Token;

        DirectoryInfoViewModel.ResetThreadStatistics();
        IsSorting = true;

        try
        {
            // Run the (blocking, recursive) sort on a background task so the GUI thread stays
            // responsive and the Cancel button can be pressed (Zadanie 2 + 5).
            // Cancellation is cooperative: Sort bails out on its own, no exception is thrown.
            await Task.Factory.StartNew(
                () => Root.Sort(sortOptions, cancellationToken));

            StatusMessage = cancellationToken.IsCancellationRequested
                ? Strings.SortingCancelled
                : Strings.Ready;
        }
        finally
        {
            IsSorting = false;
            _sortCancellationTokenSource.Dispose();
            _sortCancellationTokenSource = null;

            // Zadanie 4.1: report how many threads were actually used and the highest thread id.
            Debug.WriteLine("Finished sorting.");
            Debug.WriteLine($"Number of threads used while sorting: {DirectoryInfoViewModel.ThreadIds.Count}");
            Debug.WriteLine($"Max managed thread id: {DirectoryInfoViewModel.MaxThreadId}");
            foreach (var threadId in DirectoryInfoViewModel.ThreadIds)
                Debug.WriteLine("Thread ID: " + threadId);
        }
    }

    private void CancelSortExecute(object? parameter)
    {
        _sortCancellationTokenSource?.Cancel();
    }

    public void OpenRoot(string path)
    {
        Root = new DirectoryInfoViewModel() { Owner = this };
        Root.PropertyChanged += Root_PropertyChanged;
        Root.Open(path);
        RaisePropertyChanged(nameof(Root));
    }
}
