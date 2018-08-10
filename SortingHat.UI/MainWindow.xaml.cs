using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SortingHat.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private IDatabase _db;

        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set { _searchString = RunSearch(value); }
        }

        private string _searchBackground;
        public string SearchBackground
        {
            get { return _searchBackground; }
            set
            {
                _searchBackground = value;
                NotifyPropertyChanged(nameof(SearchBackground));
            }
        }

        IEnumerable<API.Models.File> _files;
        public IEnumerable<API.Models.File> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                NotifyPropertyChanged(nameof(Files));

            }
        }

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            DatabaseSettings databaseSettings = new DatabaseSettings { DBName = "hat", DBPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
            _db = new SQLiteDB(databaseSettings);

            //InitializeTokenizer();
            LoadTags();
            LoadDrives();
        }

        private string RunSearch(string search)
        {
            try
            {
                Files = _db.File.Search(search);
                SearchBackground = "#ccffcc";
            }
            catch (Exception)
            {
                SearchBackground = "#ffcccc";
            }

            return search;
        }

        private void LoadDrives()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in drives)
                FolderBrowser.Items.Add(CreateTreeItem(driveInfo));
        }

        private void InitializeTokenizer()
        {
            //Tokenizer.TokenMatcher = text =>
            //{
            //    if (text.EndsWith(";"))
            //    {
            //        // Remove the ';'
            //        return text.Substring(0, text.Length - 1).Trim().ToUpper();
            //    }

            //    return null;
            //};
        }

        private void LoadTags()
        {
            var tags = API.Models.Tag.List(_db);

            foreach (var tag in tags)
            {
                if (tag.Parent == null)
                {
                    var tagItem = new TagItem() { Name = tag.Name };
                    TagHierarchy.Items.Add(tagItem);
                    BuildTagTree(tagItem.Items, tag.Children);
                }
            };
        }

        private void BuildTagTree(ObservableCollection<TagItem> items, List<Tag> children)
        {
            foreach (var tag in children)
            {
                var tagItem = new TagItem() { Name = tag.Name };
                items.Add(tagItem);
                BuildTagTree(tagItem.Items, tag.Children);
            }
        }

        private void FolderBrowser_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            if ((item.Items.Count == 1) && (item.Items[0] is string))
            {
                item.Items.Clear();

                DirectoryInfo expandedDir = null;
                if (item.Tag is DriveInfo)
                    expandedDir = (item.Tag as DriveInfo).RootDirectory;
                if (item.Tag is DirectoryInfo)
                    expandedDir = (item.Tag as DirectoryInfo);
                try
                {
                    foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                        item.Items.Add(CreateTreeItem(subDir));
                }
                catch { }
            }
        }

        private TreeViewItem CreateTreeItem(object o)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = o.ToString();
            item.Tag = o;
            item.Items.Add("Loading...");
            return item;
        }

    }
}
