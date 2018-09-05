using System.Collections.Generic;

namespace SortingHat.API.Tagging
{
    public interface IAutoTag
    {
        IEnumerable<string> PossibleAutoTags { get; }
        string HandleTag(string autoTag, string fileName);
    }
}
