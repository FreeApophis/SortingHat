using System.IO;

namespace SortingHat.API.AutoTag
{
    public interface IAutoTag
    {
        string AutoTagKey { get; }
        string Description { get; }
        string HumanReadableAutoTagsKey { get; }
        string FindMatch(string value);
        string HandleTag(FileInfo file, string tagMatch);
    }
}
