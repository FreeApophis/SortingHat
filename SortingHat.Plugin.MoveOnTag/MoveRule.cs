using SortingHat.API.Models;

namespace SortingHat.Plugin.MoveOnTag;

/// <summary>A file that gets the <paramref name="Tag"/>, or one of its children, moves into the <paramref name="Folder"/>.</summary>
/// <param name="Tag">The tag of the rule.</param>
/// <param name="Folder">The full path of the folder.</param>
internal sealed record MoveRule(Tag Tag, string Folder)
{
    public override string ToString()
        => $"{Tag.FullName} -> {Folder}";
}
