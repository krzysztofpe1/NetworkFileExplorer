using NetworkFileExplorer.WpfApplication.DataModels;
using NetworkFileExplorer.WpfApplication.Resources.CultureStrings;
using NetworkFileExplorer.WpfApplication.ViewModels;
using System.Windows;

namespace NetworkFileExplorer.WpfApplication;

public partial class SortDialog : Window
{

    private SortOptionsViewModel _sortOptionsViewModel;

    public SortOptions SortOptions => _sortOptionsViewModel.Model;

    public SortDialog(SortOptions? sortOptions)
    {
        InitializeComponent();
        Title = Strings.SortDialogTitle;
        _sortOptionsViewModel = new SortOptionsViewModel(sortOptions);
        DataContext = _sortOptionsViewModel;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
