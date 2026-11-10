using System.Collections.Generic;

namespace SortingHat.Desktop.Core;

/// <summary>Everything the main window shows. It only changes through <see cref="MainUpdate.Update"/>.</summary>
/// <param name="Messages">What the user should know, such as warnings at startup and what plugins write, oldest first.</param>
internal sealed record MainState(IReadOnlyList<string> Messages)
{
    public static MainState Initial { get; } = new([]);

    /// <summary>What the window does when it opens.</summary>
    public static IReadOnlyList<Effect> Start { get; } = [new Effect.LoadStartupWarnings()];
}
