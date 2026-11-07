namespace SortingHat.API.Plugin;

/// <summary>Why something in the plugins folder was not loaded; hat warns about it and runs without it.</summary>
/// <param name="Source">The file or folder in the plugins folder.</param>
/// <param name="Reason">Why it was not loaded.</param>
public sealed record PluginProblem(string Source, string Reason);
