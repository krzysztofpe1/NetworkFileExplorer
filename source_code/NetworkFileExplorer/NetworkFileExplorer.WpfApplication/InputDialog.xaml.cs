using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace NetworkFileExplorer.WpfApplication
{
    public partial class InputDialog : Window
    {
        private string path;
        TreeViewItem item;
        MainWindow mainWindow;
        public InputDialog(TreeViewItem item, MainWindow mainWindow)
        {
            this.item = item;
            this.mainWindow = mainWindow;
            path = (string)item.Tag;
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool checkFileName(string name)
        {
            if (Regex.IsMatch(name, "^[a-zA-Z0-9_~-]{1,8}\\.(txt|php|html)$")) return true;
            return false;
        }
    }
}
