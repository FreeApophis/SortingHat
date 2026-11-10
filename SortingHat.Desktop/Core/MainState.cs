using System.Collections.Generic;

namespace SortingHat.Desktop.Core;

/// <summary>Everything the main window shows. It only changes through <see cref="MainUpdate.Update"/>.</summary>
/// <param name="Search">The query, its results and the selected file.</param>
/// <param name="Tags">The tag tree and the change the user is making to it.</param>
/// <param name="Messages">What the user should know, such as warnings at startup and what plugins write, oldest first.</param>
internal sealed record MainState(SearchState Search, TagsState Tags, IReadOnlyList<string> Messages)
{
    public static MainState Initial { get; } = new(SearchState.Empty, TagsState.Empty, []);

    /// <summary>What the window does when it opens.</summary>
    public static IReadOnlyList<Effect> Start { get; } = [new Effect.LoadStartupWarnings(), new Effect.LoadTags()];
}
