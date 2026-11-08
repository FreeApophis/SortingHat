using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.Plugin.MoveOnTag;

/// <summary>The move rules, at most one per tag, in the order they were added.</summary>
internal sealed record MoveRules(ImmutableList<MoveRule> Rules)
{
    public static MoveRules Empty { get; } = new([]);

    /// <summary>Adds the <paramref name="rule"/>; a rule for the same tag is replaced.</summary>
    public MoveRules With(MoveRule rule)
        => new(Rules.RemoveAll(existing => existing.Tag == rule.Tag).Add(rule));

    public Either<string, MoveRules> Without(Tag tag)
        => Rules.Any(rule => rule.Tag == tag)
            ? new MoveRules(Rules.RemoveAll(rule => rule.Tag == tag))
            : Either<string, MoveRules>.Left($"There is no move rule for '{tag.FullName}'.");

    /// <summary>
    /// The rule for a file with the <paramref name="tags"/>: a rule matches its tag and the children of its tag, and the most specific rule wins.
    /// None when no rule matches; why the file stays when equally specific rules match.
    /// </summary>
    public Option<Either<string, MoveRule>> For(IReadOnlyList<Tag> tags)
        => Rules
            .Where(rule => tags.Any(rule.Tag.IsSelfOrAncestorOf))
            .GroupBy(rule => rule.Tag.Levels.Count)
            .OrderByDescending(group => group.Key)
            .FirstOrNone()
            .Select(group => group.ToList() switch
            {
                [var rule] => Either<string, MoveRule>.Right(rule),
                var rules => Either<string, MoveRule>.Left($"the rules for {string.Join(" and ", rules.Select(rule => rule.Tag.FullName))} all match"),
            });
}
