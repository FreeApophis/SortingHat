using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SortingHat.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            IDatabase db = new SQLiteDB(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "hat");

            var tags = API.Models.Tag.List(db);

            foreach (var tag in tags)
            {
                if (tag.Parent == null)
                {
                    var tagItem = new TagItem() { Title = tag.FullName };
                    TagHierarchy.Items.Add(tagItem);
                    BuildTagTree(tagItem.Items, tag.Children);
                }
            };
        }

        private void BuildTagTree(ObservableCollection<TagItem> items, List<Tag> children)
        {
            foreach (var tag in children)
            {
                var tagItem = new TagItem() { Title = tag.Name };
                items.Add(tagItem);
                BuildTagTree(tagItem.Items, tag.Children);
            }
        }
    }
}
