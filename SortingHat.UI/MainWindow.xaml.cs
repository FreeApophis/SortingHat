using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Parser;
using SortingHat.API.Parser.OperatorType;

namespace SortingHat.UI;

public partial class MainWindow : MetroWindow, INotifyPropertyChanged
{
    private readonly IFile _file;

    private readonly ITag _tag;
    private readonly ILogger<MainWindow> _logger;
    private readonly Parser _parser;
    private string _searchString = string.Empty;

    private IEnumerable<API.Models.FileAt> _files;

    private bool _popupVisible;

    public MainWindow(IFile file, ITag tag, ILogger<MainWindow> logger, Parser parser)
    {
        logger.LogTrace("Main Window created");

        _file = file;
        _tag = tag;
        _logger = logger;
        _parser = parser;
        _files = [];

        InitializeComponent();

        DataContext = this;

        LoadDrives();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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

            // var parser = new QueryParser(_searchString);
            // var ir = parser.Parse();

            // if (parser.IllegalExpression)
            // {
            //    return "#ffcccc";
            // }

            // var visitor = _newSearchQueryVisitor();
            // ir.Accept(visitor);

            // if (visitor.UnknownTag)
            // {
            //    return "#ffffcc";
            // }
            // return "#ccffcc";
        }
    }

    public IEnumerable<API.Models.FileAt> Files
    {
        get
        {
            return _files;
        }

        set
        {
            _files = value;
            NotifyPropertyChanged(nameof(Files));
        }
    }

    public bool PopupVisible
    {
        get
        {
            return _popupVisible;
        }

        set
        {
            _popupVisible = value;
            NotifyPropertyChanged(nameof(PopupVisible));
        }
    }

    private void NotifyPropertyChanged(string propName)
    {
        _logger.LogTrace($"Property '{propName}' changed");

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    private string RunSearch(string search)
    {
        NotifyPropertyChanged(nameof(SearchBackground));

        PossibleNextArguments(search);

        Files = _file.Search(search).Match(left: _ => [], right: result => result.Files);

        return search;
    }

    /// <summary>What can come next: after a valid query an operator, after an incomplete one what the parser expected.</summary>
    private void PossibleNextArguments(string search)
    {
        var operatorType = new TextOperatorType();
        IntelliSense.Items.Clear();

        var next = _parser.Parse(search).Match(
            left: error => error.IsIncomplete ? error.Expected : [],
            right: _ => [QueryPart.And, QueryPart.Or]);

        foreach (var part in next)
        {
            switch (part)
            {
                case QueryPart.And:
                    IntelliSense.Items.Add(new IntellisenseItem { Title = operatorType.And });
                    break;
                case QueryPart.Or:
                    IntelliSense.Items.Add(new IntellisenseItem { Title = operatorType.Or });
                    break;
                case QueryPart.Not:
                    IntelliSense.Items.Add(new IntellisenseItem { Title = operatorType.Not });
                    break;
                case QueryPart.OpenParenthesis:
                    IntelliSense.Items.Add(new IntellisenseItem { Title = "(" });
                    break;
                case QueryPart.ClosedParenthesis:
                    IntelliSense.Items.Add(new IntellisenseItem { Title = ")" });
                    break;
                case QueryPart.Tag:
                    SelectPossibleTags();
                    break;
            }

            PopupVisible = true;
        }
    }

    private void SelectPossibleTags()
    {
        foreach (var possibleTag in _tag.GetTags().Select(t => t.FullName).Take(9))
        {
            IntelliSense.Items.Add(new IntellisenseItem { Title = possibleTag });
        }
    }

    private void LoadDrives()
    {
        foreach (var driveInfo in DriveInfo.GetDrives())
        {
            FolderBrowser.Items.Add(CreateTreeItem(driveInfo));
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
                    _ => null,
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
                }
                catch { }
            }
        }
    }

    private TreeViewItem CreateTreeItem(object o)
    {
        var item = new TreeViewItem
        {
            Header = o.ToString(),
            Tag = o,
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

    private class IntellisenseItem
    {
        public string Title { get; set; } = string.Empty;
    }
}
