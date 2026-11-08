using SortingHat.API.Models;

namespace SortingHat.API.Events;

/// <summary>The content at a known path has changed and hat stored the new content; the tags of the old content were carried over.</summary>
/// <param name="Stored">What hat knew for the path before.</param>
/// <param name="Current">The new content at the path.</param>
public sealed record FileChanged(FileAt Stored, FileAt Current);
