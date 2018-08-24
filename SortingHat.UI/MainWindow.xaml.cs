using SortingHat.API;
using SortingHat.API.Models;
using SortingHat.API.Parser;
using SortingHat.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SortingHat.API.Parser.OperatorType;
using SortingHat.API.Parser.Token;

namespace SortingHat.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private SQLiteDB _db;

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

        private bool _popupVisible;
        public bool PopupVisible
        {
            get { return _popupVisible; }
            set
            {
                _popupVisible = value;
                NotifyPropertyChanged(nameof(PopupVisible));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            DatabaseSettings databaseSettings = new DatabaseSettings { DBName = "hat", DBPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
            //_db = new SQLiteDB(databaseSettings);

            //InitializeTokenizer();
            LoadTags();
            LoadDrives();
        }

        private string RunSearch(string search)
        {
            SearchBackground = BackgroundColor(search);
            PossibleNextArguments(search);

            Files = _db.File.Search(search);

            return search;
        }

        class IntellisenseItem
        {
            public string Title { get; set; }
        }

        private void PossibleNextArguments(string search)
        {
            var operatorType = new TextOperatorType();
            IntelliSense.Items.Clear();

            var parser = new QueryParser(search);
            parser.Parse();

            foreach (var token in parser.NextToken())
            {
                switch (token)
                {
                    case AndToken t:
                        IntelliSense.Items.Add(new IntellisenseItem { Title = operatorType.And });
                        break;
                    case OrToken t:
                        IntelliSense.Items.Add(new IntellisenseItem { Title = operatorType.Or });
                        break;
                    case NotToken t:
                        IntelliSense.Items.Add(new IntellisenseItem { Title = operatorType.Not });
                        break;
                    case OpenParenthesisToken t:
                        IntelliSense.Items.Add(new IntellisenseItem { Title = "(" });
                        break;
                    case ClosedParenthesisToken t:
                        IntelliSense.Items.Add(new IntellisenseItem { Title = ")" });
                        break;
                    case TagToken t:
                        SelectPossibleTokens(t);
                        break;
                    default:
                        IntelliSense.Items.Add(new IntellisenseItem { Title = "???" });
                        break;
                }

                PopupVisible = true;
            }


        }

        private void SelectPossibleTokens(TagToken tagToken)
        {
            var partial = (tagToken == null ? string.Empty : tagToken.Value) ?? string.Empty;
            foreach (var possibleTag in _db.Tag.GetTags().Select(t => t.FullName).Where(f => f.StartsWith(partial)))
            {
                IntelliSense.Items.Add(new IntellisenseItem { Title = possibleTag });
                if (IntelliSense.Items.Count > 8)
                {
                    break;
                }

            }
        }

        private string BackgroundColor(string search)
        {
            var parser = new QueryParser(search);
            var ir = parser.Parse();

            if (parser.IllegalExpression)
            {
                return "#ffcccc";
            }

            //var visitor = new SearchQueryVisitor(_db);
            //ir.Accept(visitor);

            //if (visitor.UnknownTag)
            //{
            //    return "#ffffcc";
            //}
            return "#ccffcc";
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
                    var tagItem = new TagItem(tag);
                    TagHierarchy.Items.Add(tagItem);
                    BuildTagTree(tagItem.Items, tag.Children);
                }
            };
        }

        private void BuildTagTree(ObservableCollection<TagItem> items, List<Tag> children)
        {
            foreach (var tag in children)
            {
                var tagItem = new TagItem(tag);
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

        private void OnItemMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var treeViewItem = sender as TreeViewItem;
            if (treeViewItem != null && treeViewItem.IsSelected)
            {
                var tagItem = treeViewItem.DataContext as TagItem;
                SearchBox.Text = tagItem.Tag.FullName;
                e.Handled = true;
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
