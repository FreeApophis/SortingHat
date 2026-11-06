using System.Collections.ObjectModel;
using SortingHat.API.Models;

namespace SortingHat.UI;

public class TagItem(Tag tag)
{
    public Tag Tag { get; set; } = tag;

    public ObservableCollection<TagItem> Items { get; set; } = [];
}
