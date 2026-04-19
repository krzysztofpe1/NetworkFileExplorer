using GalaSoft.MvvmLight;
using NetworkFileExplorer.WpfApplication.DataModels;
using System.DirectoryServices;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

internal class SortOptionsViewModel : ViewModelBase
{
    public SortType OrderBy
    {
        get => Model.OrderBy;
        set
        {
            if (Model.OrderBy == value)
                return;

            Model.OrderBy = value;
            RaisePropertyChanged();
        }
    }

    public SortDirection Direction
    {
        get => Model.Direction;
        set
        {
            if (Model.Direction == value)
                return;

            Model.Direction = value;
            RaisePropertyChanged();
        }
    }

    public SortOptions Model
    {
        get;
        private set
        {
            if(field == value)
                return;
            field = value;
            RaisePropertyChanged();
        }
    }

    public SortOptionsViewModel(SortOptions? model)
    {
        Model = model ?? new SortOptions()
        {
            Direction = SortDirection.Ascending,
            OrderBy = SortType.Name
        };
        RaisePropertyChanged(nameof(Direction));
        RaisePropertyChanged(nameof(OrderBy));
    }
}
