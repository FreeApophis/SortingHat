namespace SortingHat.CLI.Commands.Files;

/// <summary>A file that copy-files or move-files puts from <paramref name="Source"/> to <paramref name="Target"/>.</summary>
internal sealed record ExportTarget(string Source, string Target);
