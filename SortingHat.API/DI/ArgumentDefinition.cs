namespace SortingHat.API.DI;

/// <summary>
/// A positional argument of an <see cref="IDeclaredCommand"/>. Its values are strings: the command parses them into its own types.
/// </summary>
public sealed record ArgumentDefinition
{
    /// <summary>The name in the help, for example <c>query</c> for <c>&lt;query&gt;</c>.</summary>
    public required string Name { get; init; }

    public string Description { get; init; } = string.Empty;

    public Arity Arity { get; init; } = Arity.ExactlyOne;
}
