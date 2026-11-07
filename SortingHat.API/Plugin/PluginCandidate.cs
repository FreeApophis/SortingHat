namespace SortingHat.API.Plugin;

/// <summary>A plugin folder with its plugin assembly, ready to load.</summary>
internal sealed record PluginCandidate(string Name, string AssemblyPath);
