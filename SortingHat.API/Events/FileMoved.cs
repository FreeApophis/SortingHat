using SortingHat.API.Models;

namespace SortingHat.API.Events;

/// <summary>move-files moved the file from its path to <paramref name="Destination"/>, on disk and in the database.</summary>
/// <param name="File">The file at the path it was moved from.</param>
/// <param name="Destination">The path it lives at now.</param>
public sealed record FileMoved(FileAt File, string Destination);
