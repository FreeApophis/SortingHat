namespace SortingHat.API.Plugin;

/// <summary>The folder hat loads its plugins from, normally 'plugins' next to hat. Every plugin is a folder in it: plugins/&lt;name&gt;/&lt;name&gt;.dll with its own dependencies.</summary>
/// <param name="Path">The full path of the folder.</param>
internal sealed record PluginDirectory(string Path);
