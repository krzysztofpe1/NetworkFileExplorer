using System.DirectoryServices;

namespace NetworkFileExplorer.WpfApplication.DataModels;

public class SortOptions
{
    public required SortDirection Direction { get; set; }
    public required SortType OrderBy { get; set; }
}

public enum SortType
{
    Name,
    Extension,
    Size,
    LastModifiedDate
}
