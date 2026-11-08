using SortingHat.API.Models;

namespace SortingHat.API.Events;

/// <summary>hat stored a file it did not know: new content, or known content at a new path.</summary>
/// <param name="File">The file at the path it was stored at.</param>
public sealed record FileAdded(FileAt File);
