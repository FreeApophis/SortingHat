using System.Collections.ObjectModel;
using SortingHat.API.Models;

namespace SortingHat.UI;

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
