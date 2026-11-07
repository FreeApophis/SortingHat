using System.Collections.Generic;
using System.IO;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.API.AutoTag;

public interface IAutoTagHandler
{
    IEnumerable<IAutoTag> AutoTags { get; }

    Option<Tag> TagFromMask(string tagMask, FileInfo file);
}
