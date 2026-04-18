using Microsoft.Win32;
using NetworkFileExplorer.WpfApplication.Utils;
using NetworkFileExplorer.WpfApplication.ViewModels;
using System.Windows;

namespace NetworkFileExplorer.WpfApplication;

public partial class MainWindow : Window
{

    private FileExplorerViewModel _fileExplorerViewModel;

    public MainWindow()
    {
        InitializeComponent();
        _fileExplorerViewModel = new();
        DataContext = _fileExplorerViewModel;
        _fileExplorerViewModel.PropertyChanged += _fileExplorerViewModel_PropertyChanged;
    }

    private void _fileExplorerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(FileExplorerViewModel.Lang))
        {
            CultureResourcesHelper.ChangeCulture(new System.Globalization.CultureInfo(_fileExplorerViewModel.Lang));
        }
    }

    private void OpenDirectoryMI_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFolderDialog() { Title = "Select directory to open", Multiselect = false };
        if (ofd.ShowDialog() != true)
            return;

        var path = ofd.FolderName;
        _fileExplorerViewModel.OpenRoot(path);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}