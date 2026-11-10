using System.Collections.Generic;

namespace SortingHat.Desktop.Core;

/// <summary>Something that happened: the user did something, or an <see cref="Effect"/> has a result.</summary>
internal abstract record Message
{
    private Message()
    {
    }

    /// <summary>The warnings of the start, such as plugins that were not loaded.</summary>
    public sealed record StartupWarningsLoaded(IReadOnlyList<string> Warnings) : Message;

    /// <summary>A line for the user, from a plugin or about an effect that failed.</summary>
    public sealed record Wrote(string Line) : Message;
}
