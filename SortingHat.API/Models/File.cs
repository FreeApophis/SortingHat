using System;

namespace SortingHat.API.Models;

/// <summary>
/// The content of a file, which hat knows by its <see cref="Hash"/>. The same content can live at several paths, see <see cref="FileAt"/>.
/// </summary>
/// <param name="Hash">The hash of the content.</param>
/// <param name="Size">The size in bytes.</param>
/// <param name="CreatedAt">When the content was created (UTC); the database keeps the oldest creation time it saw.</param>
public sealed record File(Hash Hash, long Size, DateTime CreatedAt);
