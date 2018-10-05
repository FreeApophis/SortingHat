using System.Collections.Generic;

namespace SortingHat.API.AutoTag
{
    public interface IAutoTag
    {
        IEnumerable<string> PossibleAutoTags { get; }
        string HandleTag(string autoTag, string fileName);
    }
}
