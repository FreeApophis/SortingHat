using System;
using System.Collections.Generic;
using System.Threading;
using Autofac.Features.OwnedInstances;
using JetBrains.Annotations;
using Serilog;
using SortingHat.API.DI;
using SortingHat.App;
using SortingHat.App.Files;
using SortingHat.App.Tags;
using SortingHat.Desktop.Core;

namespace SortingHat.Desktop.Shell;

/// <summary>
/// Runs an <see cref="Effect"/> and returns what happened as messages. Every effect gets its own lifetime scope
/// (<see cref="Owned{T}"/>), like one run of hat, so it gets its own database connection.
/// </summary>
[UsedImplicitly]
internal sealed class EffectRunner(
    ILogger logger,
    SearchPause searchPause,
    IFileOpener fileOpener,
    Func<Owned<StartupReport>> startupReport,
    Func<Owned<ITag>> tagStore,
    Func<Owned<IFile>> fileStore,
    Func<Owned<FileReports>> fileReports,
    Func<Owned<TagEdits>> tagEdits)
{
    /// <summary>An effect that fails is a bug or a broken database: the user gets one line, the log the exception.</summary>
    public IReadOnlyList<Message> Run(Effect effect)
    {
        try
        {
            return effect switch
            {
                Effect.LoadStartupWarnings => Use(startupReport, report => new Message.StartupWarningsLoaded(report.Warnings())),
                Effect.LoadTags => Use(tagStore, tags => new Message.TagsLoaded(tags.FileCounts())),
                Effect.Pause pause => Pause(pause),
                Effect.Search search => Use(fileStore, files => new Message.SearchFinished(search.Generation, files.Search(search.Query))),
                Effect.LoadDetails load => Use(fileReports, reports => new Message.DetailsLoaded(load.File, reports.Details(load.File.File))),
                Effect.Open open => Open(open.Path),
                Effect.RenameTag rename => Use(tagEdits, edits => new Message.TagEdited(edits.Rename(rename.Tag, rename.NewName).Select(renamed => $"Renamed '{rename.Tag}' to '{renamed}'."))),
                Effect.MoveTag move => Use(tagEdits, edits => new Message.TagEdited(edits.Move([move.Tag], move.Destination).Select(_ => $"Moved '{move.Tag}' to {move.Destination.Match(none: "the top level", some: tag => $"'{tag}'")}."))),
                Effect.RemoveTag remove => Use(tagEdits, edits => new Message.TagEdited(edits.Remove(remove.Tag).Select(_ => $"Removed '{remove.Tag}'."))),
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, "Every effect is handled."),
            };
        }
        catch (Exception exception)
        {
            logger.Error(exception, "{Effect} failed", effect);

            return [new Message.Wrote($"{effect.GetType().Name} failed: {exception.Message}")];
        }
    }

    private static IReadOnlyList<Message> Use<T>(Func<Owned<T>> resolve, Func<T, Message> run)
        where T : notnull
    {
        using var owned = resolve();

        return [run(owned.Value)];
    }

    private IReadOnlyList<Message> Pause(Effect.Pause pause)
    {
        Thread.Sleep(searchPause.Duration);

        return [new Message.Paused(pause.Generation)];
    }

    private IReadOnlyList<Message> Open(string path)
        => fileOpener.Open(path).Match(
            none: () => [],
            some: problem => (IReadOnlyList<Message>)[new Message.Wrote(problem)]);
}
