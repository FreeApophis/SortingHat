using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.Desktop.Core;

/// <summary>The pure core of the tag tree: loading it, and renaming, moving and removing tags after the user confirmed.</summary>
internal static class TagUpdate
{
    private const string TopLevel = ":";

    public static MainState Loaded(MainState state, IReadOnlyDictionary<Tag, long> fileCounts)
    {
        var tree = TagTreeNode.Build(fileCounts);
        IReadOnlyList<Tag> all = [.. Flatten(tree)];

        return state with
        {
            Tags = state.Tags with { All = all, Tree = tree },
            Search = state.Search with { Completions = QueryCompletions.For(state.Search.Query, all) },
        };
    }

    public static MainState Started(MainState state, TagEdit edit)
        => state with { Tags = state.Tags with { Edit = edit, Problem = Option<string>.None } };

    public static MainState TextChanged(MainState state, string text)
        => state with
        {
            Tags = state.Tags with
            {
                Edit = state.Tags.Edit.Select(edit => edit switch
                {
                    TagEdit.Renaming renaming => renaming with { Text = text },
                    TagEdit.Moving moving => moving with { Text = text },
                    TagEdit.Removing => edit,
                }),
                Problem = Option<string>.None,
            },
        };

    public static MainState Cancelled(MainState state)
        => state with { Tags = state.Tags with { Edit = Option<TagEdit>.None, Problem = Option<string>.None } };

    /// <summary>What the user typed is parsed here; only a valid change goes to the database.</summary>
    public static Transition Confirmed(MainState state)
        => state.Tags.Edit.Match(
            none: () => new Transition(state, []),
            some: edit => Change(edit).Match(
                left: problem => new Transition(state with { Tags = state.Tags with { Problem = problem } }, []),
                right: effect => new Transition(state, [effect])));

    /// <summary>A done change closes the edit bar and reloads what it changed; a refused one keeps the bar open with the reason.</summary>
    public static Transition Done(MainState state, Either<string, string> result, Func<MainState, Transition> searchAgain)
        => result.Match(
            left: problem => new Transition(state with { Tags = state.Tags with { Problem = problem } }, []),
            right: done => WithTagsReloaded(searchAgain(Cancelled(state) with { Messages = [.. state.Messages, done] })));

    private static Transition WithTagsReloaded(Transition transition)
        => transition with { Effects = [new Effect.LoadTags(), .. transition.Effects] };

    private static Either<string, Effect> Change(TagEdit edit)
        => edit switch
        {
            TagEdit.Renaming renaming => TagName.Parse(renaming.Text.Trim()).Select(name => (Effect)new Effect.RenameTag(renaming.Tag, name)),
            TagEdit.Moving moving => Destination(moving.Text.Trim()).Select(destination => (Effect)new Effect.MoveTag(moving.Tag, destination)),
            TagEdit.Removing removing => new Effect.RemoveTag(removing.Tag),
        };

    private static Either<string, Option<Tag>> Destination(string text)
        => text == TopLevel
            ? Option<Tag>.None
            : Tag.Parse(text).Select(Option.Some);

    private static IEnumerable<Tag> Flatten(IEnumerable<TagTreeNode> nodes)
        => nodes.SelectMany(node => Flatten(node.Children).Prepend(node.Tag));
}
