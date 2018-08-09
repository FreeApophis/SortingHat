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

            InitializeTokenizer();

            LoadTags(db);
        }

        private void InitializeTokenizer()
        {
            Tokenizer.TokenMatcher = text =>
            {
                if (text.EndsWith(";"))
                {
                    // Remove the ';'
                    return text.Substring(0, text.Length - 1).Trim().ToUpper();
                }

                return null;
            };
        }

        private void LoadTags(IDatabase db)
        {
            var tags = API.Models.Tag.List(db);

            foreach (var tag in tags)
            {
                if (tag.Parent == null)
                {
                    var tagItem = new TagItem() { Name= tag.Name };
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
    }
}
