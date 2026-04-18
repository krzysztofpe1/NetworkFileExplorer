using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

class DirectoryInfoViewModel : FileSystemInfoViewModel
{

    private string? _originalPath;

    public ObservableCollection<FileSystemInfoViewModel> Items { get; private set; } = new();
    public Exception? Exception { get; private set; }

    private FileSystemWatcher? _watcher;

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
                DirectoryInfoViewModel dirVM = new() { Model = dirInfo, Caption = dirInfo.Name, LastWriteTime = dirInfo.LastWriteTime };
                dirVM.Open(dirInfo.FullName);
                Items.Add(dirVM);
            }
            foreach (string fileName in Directory.GetFiles(path))
            {
                FileInfo fileInfo = new(fileName);
                FileInfoViewModel fileVM = new() { Model = fileInfo, Caption = fileInfo.Name, LastWriteTime = fileInfo.LastWriteTime };
                Items.Add(fileVM);
            }
            return true;
        }
        catch (Exception ex)
        {
            Exception = ex;
            return false;
        }
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
