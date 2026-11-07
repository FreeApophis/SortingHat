using System;
using System.Linq;
using Funcky.Monads;

namespace SortingHat.API.Models;

/// <summary>
/// The name of one level of a <see cref="Tag"/>, such as <c>2018</c> in <c>:tax:2018</c>. It contains only letters, digits, '-', '_', '+' and '.',
/// the characters a search can contain as well: tagging and searching share this one rule.
/// Names are equal in any case, like file names on Windows: <c>Tax</c> and <c>TAX</c> are the same name, and <see cref="Value"/> keeps the spelling.
/// </summary>
public sealed record TagName
{
    private const char Separator = ':';

    private const char Replacement = '-';

    private TagName(string value)
        => Value = value;

    public string Value { get; }

    /// <summary>Returns the name, or why <paramref name="name"/> cannot be one.</summary>
    public static Either<string, TagName> Parse(string name)
        => Problem(name).Match(
            none: () => Either<string, TagName>.Right(new TagName(name)),
            some: reason => Either<string, TagName>.Left($"'{name}' is not a tag name: {reason}."));

    /// <summary>
    /// The name for a <paramref name="value"/> read from a file, such as an EXIF entry: every run of characters a name cannot contain becomes one '-',
    /// so 'NIKON CORPORATION' becomes 'NIKON-CORPORATION'. None when nothing of it can be a name.
    /// </summary>
    public static Option<TagName> FromValue(string value)
        => string.Join(Replacement, string.Concat(value.Select(character => IsAllowed(character) ? character : ' ')).Split(' ', StringSplitOptions.RemoveEmptyEntries)).Trim(Replacement) switch
        {
            "" => Option<TagName>.None,
            var name => new TagName(name),
        };

    public bool Equals(TagName? other)
        => other is not null && StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);

    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString()
        => Value;

    /// <summary>Whether a tag name can contain the <paramref name="character"/>. The query lexer reads tags with the same rule.</summary>
    internal static bool IsAllowed(char character)
        => char.IsLetterOrDigit(character) || character is '-' or '_' or '+' or '.';

    /// <summary>Why <paramref name="name"/> cannot be the name of a level, none when it can.</summary>
    internal static Option<string> Problem(string name)
        => name switch
        {
            "" => "a level is empty",
            _ when name.Contains(Separator) => "a name cannot contain a colon, colons separate the levels",
            _ when !name.All(IsAllowed) => "a level can only contain letters, digits, '-', '_', '+' and '.'",
            _ => Option<string>.None,
        };
}
