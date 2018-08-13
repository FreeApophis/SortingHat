using SortingHat.API.Models;
using System.Collections.ObjectModel;

namespace SortingHat.UI
{
    public class TagItem
    {
        public TagItem(Tag tag)
        {
            Items = new ObservableCollection<TagItem>();
            Tag = tag;
        }

        public Tag Tag { get; set; }

        public ObservableCollection<TagItem> Items { get; set; }
    }
}
