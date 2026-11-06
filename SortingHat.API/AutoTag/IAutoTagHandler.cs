using System.Collections.Generic;
using System.IO;
using SortingHat.API.Models;

namespace SortingHat.API.AutoTag;

public interface IAutoTagHandler
{
    IEnumerable<IAutoTag> AutoTags { get; }

    Tag? TagFromMask(string tagMask, FileInfo file);
}
