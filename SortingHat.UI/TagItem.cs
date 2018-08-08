using System.Collections.ObjectModel;

namespace SortingHat.UI
{
    public class TagItem
    {
        public TagItem()
        {
            Items = new ObservableCollection<TagItem>();
        }

        public string Title { get; set; }

        public ObservableCollection<TagItem> Items { get; set; }
    }
}
