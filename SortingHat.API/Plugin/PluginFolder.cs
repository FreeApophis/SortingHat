using System.Collections.Generic;

namespace SortingHat.API.Plugin;

/// <summary>A folder in the plugins folder, with the files in it.</summary>
internal sealed record PluginFolder(string Path, IReadOnlyList<string> Files);
