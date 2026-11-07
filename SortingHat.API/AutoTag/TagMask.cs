using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.API.AutoTag;

/// <summary>
/// A tag whose levels can contain variables, such as <c>:Photos:{Camera.Make}:{CreatedAt.Year}</c>. It is parsed once against the known
/// <see cref="IAutoTag"/>s, so a typo is reported before any file is touched, and then filled per file with <see cref="Apply"/>.
/// </summary>
public sealed class TagMask
{
    private const char Separator = ':';

    private readonly string _text;

    private readonly ImmutableArray<ImmutableArray<Segment>> _levels;

    private TagMask(string text, ImmutableArray<ImmutableArray<Segment>> levels)
        => (_text, _levels) = (text, levels);

    /// <summary>Returns the mask written in <paramref name="text"/>, or why it is none, such as an unknown variable.</summary>
    public static Either<string, TagMask> Parse(string text, IReadOnlyList<IAutoTag> autoTags)
        => (text.StartsWith(Separator)
                ? text[1..].Split(Separator).Aggregate(
                    Either<string, ImmutableArray<ImmutableArray<Segment>>>.Right([]),
                    (levels, level) => from parsed in levels
                                       from segments in Level(level, autoTags)
                                       select parsed.Add(segments))
                : Either<string, ImmutableArray<ImmutableArray<Segment>>>.Left("a tag starts with a colon, for example ':tax:2018'"))
            .Match(
                left: reason => Either<string, TagMask>.Left($"'{text}' is not a tag mask: {reason}."),
                right: levels => Either<string, TagMask>.Right(new TagMask(text, levels)));

    /// <summary>
    /// The tag for the <paramref name="file"/>, or why the mask gives none for it: a variable without a value, a value that is no tag name, or a failing auto-tag.
    /// The reason does not name the file, so the reasons of many files can be counted together.
    /// </summary>
    public Either<string, Tag> Apply(AutoTagFile file)
        => _levels.Skip(1).Aggregate(
            Name(_levels[0], file).Select(Tag.Root),
            (tag, level) => from parent in tag
                            from name in Name(level, file)
                            select parent.Child(name));

    public override string ToString()
        => _text;

    private static Either<string, ImmutableArray<Segment>> Level(string level, IReadOnlyList<IAutoTag> autoTags)
        => level.IndexOfAny(['{', '}']) < 0
            ? TagName.Problem(level).Match(
                none: () => Either<string, ImmutableArray<Segment>>.Right([new Literal(level)]),
                some: Either<string, ImmutableArray<Segment>>.Left)
            : Segments(level, autoTags, []);

    private static Either<string, ImmutableArray<Segment>> Segments(string rest, IReadOnlyList<IAutoTag> autoTags, ImmutableArray<Segment> parsed)
        => rest.IndexOfAny(['{', '}']) switch
        {
            _ when rest is "" => parsed,
            < 0 => parsed.Add(new Literal(rest)),
            var start when rest[start] == '}' => Either<string, ImmutableArray<Segment>>.Left("a '}' has no '{' before it"),
            0 => rest.IndexOf('}') switch
            {
                < 0 => Either<string, ImmutableArray<Segment>>.Left("a '{' is not closed with '}'"),
                var end => ParseVariable(rest[1..end], autoTags).SelectMany(variable => Segments(rest[(end + 1)..], autoTags, parsed.Add(variable))),
            },
            var start => Segments(rest[start..], autoTags, parsed.Add(new Literal(rest[..start]))),
        };

    /// <summary>The variable is the auto-tag with the longest name the text starts with; what follows its name is its argument.</summary>
    private static Either<string, Segment> ParseVariable(string text, IReadOnlyList<IAutoTag> autoTags)
        => autoTags
            .Where(autoTag => text == autoTag.Name || text.StartsWith($"{autoTag.Name}.", StringComparison.Ordinal))
            .GroupBy(autoTag => autoTag.Name.Length)
            .OrderByDescending(candidates => candidates.Key)
            .FirstOrNone()
            .Match(
                none: () => Either<string, Segment>.Left(text is "" ? "the variable {} is empty" : $"the variable {{{text}}} is unknown, the help lists the variables"),
                some: candidates => candidates.ToList() switch
                {
                    [var autoTag] => autoTag.Parameter
                        .Parse(text.Length == autoTag.Name.Length ? Option<string>.None : text[(autoTag.Name.Length + 1)..])
                        .Match(
                            left: reason => Either<string, Segment>.Left($"the variable {{{text}}} {reason}"),
                            right: argument => Either<string, Segment>.Right(new Variable(autoTag, argument, text))),
                    _ => Either<string, Segment>.Left($"the variable {{{text}}} is registered by more than one plugin"),
                });

    private static Either<string, TagName> Name(ImmutableArray<Segment> level, AutoTagFile file)
        => level
            .Aggregate(Either<string, string>.Right(string.Empty), (text, segment) => from before in text from value in segment.Value(file) select before + value)
            .SelectMany(name => TagName.Parse(name));

    private abstract record Segment
    {
        public abstract Either<string, string> Value(AutoTagFile file);
    }

    private sealed record Literal(string Text) : Segment
    {
        public override Either<string, string> Value(AutoTagFile file)
            => Text;
    }

    private sealed record Variable(IAutoTag AutoTag, AutoTagArgument Argument, string Text) : Segment
    {
        /// <summary>Values read from files are often padded, such as EXIF strings; a value must not add levels.</summary>
        public override Either<string, string> Value(AutoTagFile file)
            => Read(file).SelectMany(value => value.Trim() switch
            {
                "" => Either<string, string>.Left($"no value for {{{Text}}}"),
                var trimmed => TagName.Problem(trimmed).Match(
                    none: () => Either<string, string>.Right(trimmed),
                    some: problem => Either<string, string>.Left($"{{{Text}}} gives '{trimmed}', which is no tag name: {problem}")),
            });

        /// <summary>An auto-tag comes from a plugin: whatever it throws skips the tag for this file, the other files and tags go on.</summary>
        private Either<string, string> Read(AutoTagFile file)
        {
            try
            {
                return AutoTag.Value(file, Argument).Match(
                    none: () => Either<string, string>.Left($"no value for {{{Text}}}"),
                    some: Either<string, string>.Right);
            }
            catch (Exception exception)
            {
                return Either<string, string>.Left($"{{{Text}}} failed: {exception.Message}");
            }
        }
    }
}
