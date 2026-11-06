using System.Collections.Generic;

namespace SortingHat.API.Models;

/// <summary>
/// The files matching a search, and the tags of the search which do not exist.
/// </summary>
public sealed record SearchResult(IReadOnlyList<File> Files, IReadOnlyList<string> UnknownTags);
