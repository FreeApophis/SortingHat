using SortingHat.API.Models;
using System.Collections.Generic;
using System.IO;

namespace SortingHat.API.AutoTag
{
    public interface IAutoTagHandler
    {
        IEnumerable<IAutoTag> AutoTags { get; }

        Tag TagFromMask(string tagMask, FileInfo file);
    }
}
