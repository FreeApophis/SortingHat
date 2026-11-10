using System;

namespace SortingHat.Desktop.Shell;

/// <summary>How long the user pauses typing before the query is searched.</summary>
internal sealed record SearchPause(TimeSpan Duration)
{
    public static SearchPause Default { get; } = new(TimeSpan.FromMilliseconds(300));
}
