using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.Models;

/// <summary>
/// A hierarchical tag such as <c>:tax:2018</c>: it starts with a colon, and colons separate its levels. A tag is a value: it knows its levels, nothing else.
/// </summary>
public sealed record Tag
{
    private const char Separator = ':';

    private readonly ImmutableArray<TagName> _levels;

    private Tag(ImmutableArray<TagName> levels)
        => _levels = levels;

    /// <summary>The levels from the root, at least one.</summary>
    public IReadOnlyList<TagName> Levels
        => _levels;

    /// <summary>The name of the last level: <c>2018</c> in <c>:tax:2018</c>.</summary>
    public TagName Name
        => _levels[^1];

    /// <summary>The tag one level up: <c>:tax</c> for <c>:tax:2018</c>, none for a tag with one level.</summary>
    public Option<Tag> Parent
        => _levels.Length > 1
            ? Option.Some(new Tag(_levels.RemoveAt(_levels.Length - 1)))
            : Option<Tag>.None;

    /// <summary>The tag as it is written: <c>:tax:2018</c>.</summary>
    public string FullName
        => string.Concat(_levels.Select(level => $"{Separator}{level}"));

    /// <summary>Returns the tag written in <paramref name="text"/>, or why it is none.</summary>
    public static Either<string, Tag> Parse(string text)
        => Problem(text).Match(
            none: () => Either<string, Tag>.Right(new Tag([.. LevelTexts(text).WhereSelect(level => TagName.Parse(level).RightOrNone())])),
            some: reason => Either<string, Tag>.Left($"'{text}' is not a tag: {reason}."));

    /// <summary>The tag with only one level.</summary>
    public static Tag Root(TagName name)
        => new([name]);

    /// <summary>The tag one level down: <c>:tax:2018</c> for <c>:tax</c> and <c>2018</c>.</summary>
    public Tag Child(TagName name)
        => new(_levels.Add(name));

    public bool Equals(Tag? other)
        => other is not null && _levels.SequenceEqual(other._levels);

    public override int GetHashCode()
        => FullName.GetHashCode(StringComparison.Ordinal);

    public override string ToString()
        => FullName;

    private static Option<string> Problem(string text)
        => text.StartsWith(Separator)
            ? LevelTexts(text).WhereSelect(TagName.Problem).FirstOrNone()
            : "a tag starts with a colon, for example ':tax:2018'";

    private static string[] LevelTexts(string text)
        => text[1..].Split(Separator);
}
