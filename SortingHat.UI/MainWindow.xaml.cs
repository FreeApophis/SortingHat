﻿using SortingHat.API.Models;
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
using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Parser.OperatorType;
using SortingHat.API.Parser.Token;
using File = SortingHat.API.Models.File;

namespace SortingHat.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IFile _file;
        private readonly ITag _tag;
        private readonly ILogger<MainWindow> _logger;
        private readonly Func<SearchQueryVisitor> _newSearchQueryVisitor;
        private readonly Parser _parser;

        private string _searchString = "";
        public string SearchString
        {
            get => _searchString;
            set => _searchString = RunSearch(value);
        }

        public string SearchBackground
        {
            get
            {
                return "#ffffff";
                //var parser = new QueryParser(_searchString);
                //var ir = parser.Parse();

                //if (parser.IllegalExpression)
                //{
                //    return "#ffcccc";
                //}

                //var visitor = _newSearchQueryVisitor();
                //ir.Accept(visitor);

                //if (visitor.UnknownTag)
                //{
                //    return "#ffffcc";
                //}
                //return "#ccffcc";
            }
        }

        IEnumerable<API.Models.File> _files;
        public IEnumerable<API.Models.File> Files
        {
            get => _files;
            set
            {
                _files = value;
                NotifyPropertyChanged(nameof(Files));

            }
        }

        private void NotifyPropertyChanged(string propName)
        {
            _logger.LogTrace($"Property '{propName}' changed");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private bool _popupVisible;
        public bool PopupVisible
        {
            get => _popupVisible;
            set
            {
                _popupVisible = value;
                NotifyPropertyChanged(nameof(PopupVisible));
            }
        }

        public MainWindow(IFile file, ITag tag, ILogger<MainWindow> logger, Func<SearchQueryVisitor> newSearchQueryVisitor, Parser parser)
        {
            logger.LogTrace("Main Window created");

            _file = file;
            _tag = tag;
            _logger = logger;
            _newSearchQueryVisitor = newSearchQueryVisitor;
            _parser = parser;
            _files = Enumerable.Empty<File>();

            InitializeComponent();

            DataContext = this;

            LoadTags();
            LoadDrives();
        }

        private string RunSearch(string search)
        {
            NotifyPropertyChanged(nameof(SearchBackground));

            PossibleNextArguments(search);

            Files = _file.Search(search);

            return search;
        }

        class IntellisenseItem
        {
            public string Title { get; set; } = string.Empty;
        }

        private void PossibleNextArguments(string search)
        {
            var operatorType = new TextOperatorType();
            IntelliSense.Items.Clear();

            _parser.Parse(search);

            foreach (var token in _parser.NextToken())
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
            foreach (var possibleTag in _tag.GetTags().Select(t => t.FullName).Where(f => f.StartsWith(partial)))
            {
                IntelliSense.Items.Add(new IntellisenseItem { Title = possibleTag });
                if (IntelliSense.Items.Count > 8)
                {
                    break;
                }

            }
        }

        private void LoadDrives()
        {
            foreach (var driveInfo in DriveInfo.GetDrives())
            {
                FolderBrowser.Items.Add(CreateTreeItem(driveInfo));
            }
        }

        private void LoadTags()
        {
            // API.Models.Tag.List(_db.ProjectDatabase);

            //foreach (var tag in tags)
            //{
            //    if (tag.Parent is null)
            //    {
            //        var tagItem = new TagItem(tag);
            //        TagHierarchy.Items.Add(tagItem);
            //        BuildTagTree(tagItem.Items, tag.Children);
            //    }
            //};
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
            if (e.Source is TreeViewItem item)
            {
                if (item.Items.Count == 1 && item.Items[0] is string)
                {
                    item.Items.Clear();

                    var expandedDir = item.Tag switch
                    {
                        DriveInfo driveInfo => driveInfo.RootDirectory,
                        DirectoryInfo directoryInfo => directoryInfo,
                        _ => null
                    };

                    try
                    {
                        if (expandedDir is { })
                        {
                            foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                            {
                                item.Items.Add(CreateTreeItem(subDir));
                            }
                        }
                    } catch { }
                }
            }
        }

        private TreeViewItem CreateTreeItem(object o)
        {
            var item = new TreeViewItem
            {
                Header = o.ToString(),
                Tag = o
            };
            item.Items.Add("Loading...");
            return item;
        }

        private void OnItemMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && treeViewItem.IsSelected && treeViewItem.DataContext is TagItem tagItem)
            {
                SearchBox.Text = tagItem.Tag.FullName;
                e.Handled = true;
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {

        }

        private void TestOnClick(object sender, RoutedEventArgs e)
        {
        }

        private void OnMenuSettings(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }

        private void OnListView(object sender, RoutedEventArgs e)
        {
            FileList.ItemTemplate = (DataTemplate)FindResource("ListTemplate");
            FileList.Style = (Style)FindResource("ListStyle");
        }

        private void OnTileView(object sender, RoutedEventArgs e)
        {
            FileList.ItemTemplate = (DataTemplate)FindResource("TileTemplate");
            FileList.Style = (Style)FindResource("TiledStyle");
        }
    }
}
