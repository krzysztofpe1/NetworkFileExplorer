using GalaSoft.MvvmLight;
using NetworkFileExplorer.WpfApplication.DataModels;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.IO;
using System.Windows;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

public class DirectoryInfoViewModel : FileSystemInfoViewModel
{

    private string? _originalPath;

    public ObservableCollection<FileSystemInfoViewModel> Items { get; private set; } = new();
    public Exception? Exception { get; private set; }
    public SortOptions? SortOptions { get; private set; }

    private FileSystemWatcher? _watcher;

    public bool IsExpanded
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

    public bool Open(string path)
    {
        try
        {
            _originalPath = path;

            Items.Clear();
            _watcher?.Dispose();

            _watcher = new FileSystemWatcher(path);
            _watcher.Created += OnFileSystemChanged;
            _watcher.Renamed += OnFileSystemChanged;
            _watcher.Changed += OnFileSystemChanged;
            _watcher.Deleted += OnFileSystemChanged;
            _watcher.Error += OnFileSystemWatcherError;
            _watcher.EnableRaisingEvents = true;

            foreach (string dirName in Directory.GetDirectories(path))
            {
                DirectoryInfo dirInfo = new(dirName);
                DirectoryInfoViewModel dirVM = new() { Model = dirInfo, Caption = dirInfo.Name, LastWriteTime = dirInfo.LastWriteTime, Owner = this };
                dirVM.Open(dirInfo.FullName);
                Items.Add(dirVM);
            }
            foreach (string fileName in Directory.GetFiles(path))
            {
                FileInfo fileInfo = new(fileName);
                FileInfoViewModel fileVM = new() { Model = fileInfo, Caption = fileInfo.Name, LastWriteTime = fileInfo.LastWriteTime, Owner = this };
                Items.Add(fileVM);
            }

            if (SortOptions != null)
                Sort(SortOptions);

            return true;
        }
        catch (Exception ex)
        {
            Exception = ex;
            return false;
        }
    }

    /// <summary>
    /// Sorts the Items and sets the provided SortOptions as default sorting options.
    /// </summary>
    /// <param name="sortOptions"></param>
    public void Sort(SortOptions sortOptions)
    {
        SortOptions = sortOptions;

        var directories = Items.Where(i => i.Model is DirectoryInfo).ToList();
        var files = Items.Where(i => i.Model is FileInfo).ToList();

        Func<FileSystemInfoViewModel, object?> keySelector = sortOptions.OrderBy switch
        {
            SortType.Name => i => i.Caption,
            SortType.Extension => i => i.Model is FileInfo f ? f.Extension : i.Caption,
            // By size in context of directories means, that it should take into account number of items in the directory, and for files it should take into account file size
            SortType.Size => i => i.Model is FileInfo f ? f.Length : (i is DirectoryInfoViewModel d ? d.Items.Count() : i.Caption),
            SortType.LastModifiedDate => i => i.LastWriteTime,
            _ => i => i.Caption
        };

        IOrderedEnumerable<FileSystemInfoViewModel> sortedDirs = sortOptions.Direction == SortDirection.Ascending ? directories.OrderBy(keySelector) : directories.OrderByDescending(keySelector);
        IOrderedEnumerable<FileSystemInfoViewModel> sortedFiles = sortOptions.Direction == SortDirection.Ascending ? files.OrderBy(keySelector) : files.OrderByDescending(keySelector);

        //foreach (var dir in directories)
        //    dir.Sort(sortOptions);

        Items.Clear();

        foreach (var dir in sortedDirs)
            Items.Add(dir);

        foreach (var file in sortedFiles)
            Items.Add(file);

        RaisePropertyChanged();
    }

    private void OnFileSystemWatcherError(object sender, ErrorEventArgs e)
    {
        Exception = e.GetException();
    }

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => OnFileSystemChanged(e));
    }

    private void OnFileSystemChanged(FileSystemEventArgs e)
    {
        if (_originalPath == null)
            return;
        Open(_originalPath);
    }
}
