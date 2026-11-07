using System.Collections.Generic;
using Funcky.Monads;

namespace SortingHat.API.AutoTag;

/// <summary>The auto-tags of hat and its plugins.</summary>
public interface IAutoTagHandler
{
    IReadOnlyList<IAutoTag> AutoTags { get; }

    /// <summary>Parses a tag mask against every registered auto-tag.</summary>
    Either<string, TagMask> ParseMask(string mask);
}
