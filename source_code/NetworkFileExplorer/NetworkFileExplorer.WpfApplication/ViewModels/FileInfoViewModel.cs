using NetworkFileExplorer.WpfApplication.Utils;
using System.IO;

namespace NetworkFileExplorer.WpfApplication.ViewModels;

public class FileInfoViewModel : FileSystemInfoViewModel
{
    public RelayCommand OpenFileCommand => OwnerExplorer?.OpenFileCommand ?? new(_ => { }, _ => false);

    public string GetFileContent()
    {
        if(Model is FileInfo fileInfo)
        {
            try
            {
                return File.ReadAllText(fileInfo.FullName);
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }
        return "Invalid file.";
    }
}
