using Funcky.Monads;

namespace SortingHat.API.Models;

/// <summary>The name of one level of a <see cref="Tag"/>, such as <c>2018</c> in <c>:tax:2018</c>.</summary>
public sealed record TagName
{
    private const char Separator = ':';

    private TagName(string value)
        => Value = value;

    public string Value { get; }

    /// <summary>Returns the name, or why <paramref name="name"/> cannot be one.</summary>
    public static Either<string, TagName> Parse(string name)
        => Problem(name).Match(
            none: () => Either<string, TagName>.Right(new TagName(name)),
            some: reason => Either<string, TagName>.Left($"'{name}' is not a tag name: {reason}."));

    public override string ToString()
        => Value;

    /// <summary>Why <paramref name="name"/> cannot be the name of a level, none when it can.</summary>
    internal static Option<string> Problem(string name)
        => name switch
        {
            "" => "a level is empty",
            _ when name.Contains(Separator) => "a name cannot contain a colon, colons separate the levels",
            _ when name.Trim() != name => "a level cannot start or end with whitespace",
            _ => Option<string>.None,
        };
}
