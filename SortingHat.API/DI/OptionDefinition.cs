using Funcky.Monads;

namespace SortingHat.API.DI;

/// <summary>
/// A flag of an <see cref="IDeclaredCommand"/>, given as <c>--name</c> or <c>-s</c> for its short name.
/// </summary>
public sealed record OptionDefinition
{
    /// <summary>The long name without dashes, for example <c>recursive</c> for <c>--recursive</c>.</summary>
    public required string Name { get; init; }

    /// <summary>The short name without the dash, for example <c>r</c> for <c>-r</c>.</summary>
    public Option<string> ShortName { get; init; }

    public string Description { get; init; } = string.Empty;
}
