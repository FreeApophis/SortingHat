using Funcky.Monads;

namespace SortingHat.API.Models;

/// <summary>A <see cref="Models.File"/> at one of the paths where it lives.</summary>
/// <param name="File">The content.</param>
/// <param name="Path">Where the content lives.</param>
/// <param name="Fingerprint">The size and last write time of the file at <paramref name="Path"/> when it was hashed, when known.</param>
public sealed record FileAt(File File, string Path, Option<PathFingerprint> Fingerprint);
