using System.Collections.Generic;

namespace SortingHat.Plugin.FileType;

internal sealed record FileType(string Category, string Name, IReadOnlyList<string> Extensions);
