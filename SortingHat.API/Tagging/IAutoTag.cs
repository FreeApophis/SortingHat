using System.Collections.Generic;

namespace SortingHat.API.Tagging
{
    public interface IAutoTag
    {
        IEnumerable<string> HandledTags { get; }
    }
}
