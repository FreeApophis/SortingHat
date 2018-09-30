using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IAutoTag
    {
        IEnumerable<string> PossibleAutoTags { get; }
        string HandleTag(string autoTag, string fileName);
    }
}
