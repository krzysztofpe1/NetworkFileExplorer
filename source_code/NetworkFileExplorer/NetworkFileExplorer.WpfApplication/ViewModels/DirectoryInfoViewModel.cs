using NetworkFileExplorer.WpfApplication.DataModels;
using NetworkFileExplorer.WpfApplication.Resources.CultureStrings;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Windows;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

public class DirectoryInfoViewModel : FileSystemInfoViewModel
{
    // Zadanie 4.1: distinct managed thread ids used while sorting and the highest id seen.
    public static List<int> ThreadIds = new();
    public static int MaxThreadId { get; private set; }
    private static readonly object _threadStatsLock = new();

    // Zadanie 4.2 / 4.3: controls how the parallel sort tasks are created.
    //   TaskCreationOptions.None         -> the thread pool decides (≈ number of CPU cores).
    //   TaskCreationOptions.LongRunning  -> one dedicated thread per task (oversubscription, 4.2).
    //   TaskCreationOptions.PreferFairness -> tasks tend to start in the order they were created (4.3).
    public static TaskCreationOptions SortTaskCreationOptions { get; set; }
        = TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;

    private string? _originalPath;

    public DispatchedObservableCollection<FileSystemInfoViewModel> Items { get; private set; } = new();
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

    public DirectoryInfoViewModel()
    {
        Items.CollectionChanged += Items_CollectionChanged;
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems?.Cast<FileSystemInfoViewModel>() ?? [])
                    item.PropertyChanged += Item_PropertyChanged;
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems?.Cast<FileSystemInfoViewModel>() ?? [])
                    item.PropertyChanged -= Item_PropertyChanged;
                break;
        }
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StatusMessage) && sender is FileSystemInfoViewModel viewModel)
            StatusMessage = viewModel.StatusMessage;
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
                StatusMessage = $"{Strings.Loading} {dirName}";
                DirectoryInfo dirInfo = new(dirName);
                DirectoryInfoViewModel dirVM = new() { Model = dirInfo, Caption = dirInfo.Name, LastWriteTime = dirInfo.LastWriteTime, Owner = this };
                dirVM.Open(dirInfo.FullName);
                Items.Add(dirVM);
            }
            foreach (string fileName in Directory.GetFiles(path))
            {
                StatusMessage = $"{Strings.Loading} {fileName}";
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
    /// Resets the thread statistics gathered for Zadanie 4 (call before starting a new sort).
    /// </summary>
    public static void ResetThreadStatistics()
    {
        lock (_threadStatsLock)
        {
            ThreadIds.Clear();
            MaxThreadId = 0;
        }
    }

    /// <summary>
    /// Records the current managed thread id, keeping the set of distinct ids and the maximum id (Zadanie 4.1).
    /// </summary>
    private static void TrackCurrentThread()
    {
        int currentThreadId = Environment.CurrentManagedThreadId;
        lock (_threadStatsLock)
        {
            if (!ThreadIds.Contains(currentThreadId))
                ThreadIds.Add(currentThreadId);
            if (currentThreadId > MaxThreadId)
                MaxThreadId = currentThreadId;
        }
    }

    /// <summary>
    /// Sorts the Items and sets the provided SortOptions as default sorting options.
    /// Sorting runs recursively, spawning one parallel task per sub-directory (Zadanie 3),
    /// and can be cancelled via the supplied <paramref name="cancellationToken"/> (Zadanie 5).
    /// </summary>
    /// <param name="sortOptions"></param>
    /// <param name="cancellationToken"></param>
    public void Sort(SortOptions sortOptions, CancellationToken cancellationToken = default)
    {
        // Cooperative cancellation (Zadanie 5): bail out quietly instead of throwing an exception.
        if (cancellationToken.IsCancellationRequested)
            return;

        TrackCurrentThread();
        Debug.WriteLine("Sorting on thread ID: " + Environment.CurrentManagedThreadId);

        SortOptions = sortOptions;

        var directories = Items.Where(i => i.Model is DirectoryInfo).Cast<DirectoryInfoViewModel>().ToList();
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

        List<Task> sortingTasks = new();
        foreach (var dir in directories)
        {
            // Stop scheduling new work once cancellation has been requested.
            if (cancellationToken.IsCancellationRequested)
                break;

            // Zadanie 4.4: report (through StatusMessage) the directory we are creating a sort task for.
            StatusMessage = $"{Strings.Sorting} {dir.Caption}";

            // The token is NOT passed to StartNew: each task always runs and bails out internally,
            // so Task.WaitAll completes normally (no Canceled tasks, no AggregateException to catch).
            sortingTasks.Add(Task.Factory.StartNew(() =>
            {
                Debug.WriteLine($"Sorting directory: {dir.Caption} on thread ID: {Environment.CurrentManagedThreadId}");
                dir.Sort(sortOptions, cancellationToken);
            }, CancellationToken.None, SortTaskCreationOptions, TaskScheduler.Default));
        }

        Task.WaitAll(sortingTasks.ToArray());

        // If we were cancelled, leave the items as they are - don't reorder a half-sorted tree.
        if (cancellationToken.IsCancellationRequested)
            return;

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

    private void Root_PropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(StatusMessage) && sender is FileSystemInfoViewModel viewModel)
            StatusMessage = viewModel.StatusMessage;
    }
}
