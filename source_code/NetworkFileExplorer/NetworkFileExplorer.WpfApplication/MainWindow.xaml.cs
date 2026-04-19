using NetworkFileExplorer.WpfApplication.Utils;
using NetworkFileExplorer.WpfApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;

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
        _fileExplorerViewModel.OnOpenFileRequest += _fileExplorerViewModel_OnOpenFileRequest;
    }

    private void _fileExplorerViewModel_OnOpenFileRequest(object? sender, FileInfoViewModel e)
    {
        string content = e.GetFileContent();
        var textBlock = new TextBlock() { Text = content };
        contentControl.Content = textBlock;
    }

    private void _fileExplorerViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(FileExplorerViewModel.Lang))
        {
            CultureResourcesHelper.ChangeCulture(new System.Globalization.CultureInfo(_fileExplorerViewModel.Lang));
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}