using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.Desktop.Core;

/// <summary>Something <see cref="MainUpdate.Update"/> wants done outside the pure core, such as reading the database; the shell runs it and reports back with messages.</summary>
internal closed record Effect
{
    public sealed record LoadStartupWarnings : Effect;

    public sealed record LoadTags : Effect;

    /// <summary>Waits for the user to pause typing, then reports <see cref="Message.Paused"/>.</summary>
    public sealed record Pause(int Generation) : Effect;

    public sealed record Search(string Query, int Generation) : Effect;

    public sealed record LoadDetails(FileAt File) : Effect;

    public sealed record Open(string Path) : Effect;

    public sealed record RenameTag(Tag Tag, TagName NewName) : Effect;

    /// <summary>Moves the tag under the <paramref name="Destination"/>, or to the top level.</summary>
    public sealed record MoveTag(Tag Tag, Option<Tag> Destination) : Effect;

    public sealed record RemoveTag(Tag Tag) : Effect;
}
