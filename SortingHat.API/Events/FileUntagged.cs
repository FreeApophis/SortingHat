using System.Collections.Generic;
using SortingHat.API.Models;

namespace SortingHat.API.Events;

/// <summary>untag-files removed the <paramref name="Tags"/> from the <paramref name="File"/>: it has none of them any more.</summary>
/// <param name="File">The file at the path it was untagged at.</param>
/// <param name="Tags">The tags untag-files was asked to remove, at least one, whether the file had them or not.</param>
public sealed record FileUntagged(FileAt File, IReadOnlyList<Tag> Tags);
