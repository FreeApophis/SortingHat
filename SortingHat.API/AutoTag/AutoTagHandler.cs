using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class AutoTagHandler(IEnumerable<IAutoTag> autoTags) : IAutoTagHandler
{
    public IReadOnlyList<IAutoTag> AutoTags { get; } = [.. autoTags];

    public Either<string, TagMask> ParseMask(string mask)
        => TagMask.Parse(mask, AutoTags);
}
