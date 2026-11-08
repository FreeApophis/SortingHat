using System.Collections.Generic;
using SortingHat.API.Models;

namespace SortingHat.API.Events;

/// <summary>The <paramref name="File"/> got the <paramref name="Tags"/> in one run of tag-files; they are stored already.</summary>
/// <param name="File">The file at the path it was tagged at.</param>
/// <param name="Tags">Every tag the file got in this run, at least one.</param>
public sealed record FileTagged(FileAt File, IReadOnlyList<Tag> Tags);
