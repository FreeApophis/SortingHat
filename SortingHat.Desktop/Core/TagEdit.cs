using SortingHat.API.Models;

namespace SortingHat.Desktop.Core;

/// <summary>A change to the tag tree the user started; it waits for the user to confirm it.</summary>
/// <param name="Tag">The tag to change.</param>
internal closed record TagEdit(Tag Tag)
{
    /// <summary>What the edit bar asks.</summary>
    public abstract string Prompt { get; }

    /// <summary>The label of the button that confirms.</summary>
    public abstract string Action { get; }

    /// <summary>Gives the tag's last level the name in <paramref name="Text"/>.</summary>
    public sealed record Renaming(Tag Tag, string Text) : TagEdit(Tag)
    {
        public override string Prompt => $"Rename '{Tag}' to";

        public override string Action => "Rename";
    }

    /// <summary>Moves the tag with its children and files under the tag in <paramref name="Text"/>, or to the top level for ':'.</summary>
    public sealed record Moving(Tag Tag, string Text) : TagEdit(Tag)
    {
        public override string Prompt => $"Move '{Tag}' under, ':' for the top level";

        public override string Action => "Move";
    }

    /// <summary>Removes the tag; its files stay.</summary>
    public sealed record Removing(Tag Tag) : TagEdit(Tag)
    {
        public override string Prompt => $"Remove '{Tag}'? Its files stay, they only lose the tag.";

        public override string Action => "Remove";
    }
}
