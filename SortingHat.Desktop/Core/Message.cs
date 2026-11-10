using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.API.Models;
using SortingHat.App.Files;

namespace SortingHat.Desktop.Core;

/// <summary>Something that happened: the user did something, or an <see cref="Effect"/> has a result.</summary>
internal abstract record Message
{
    private Message()
    {
    }

    /// <summary>The warnings of the start, such as plugins that were not loaded.</summary>
    public sealed record StartupWarningsLoaded(IReadOnlyList<string> Warnings) : Message;

    /// <summary>A line for the user, from a plugin or about an effect that failed.</summary>
    public sealed record Wrote(string Line) : Message;

    /// <summary>Every tag of the project with the number of files that have exactly this tag.</summary>
    public sealed record TagsLoaded(IReadOnlyDictionary<Tag, long> FileCounts) : Message;

    /// <summary>The user chose a tag in the tree: its files are searched.</summary>
    public sealed record TagChosen(Tag Tag) : Message;

    /// <summary>The user started to rename, move or remove a tag.</summary>
    public sealed record TagEditStarted(TagEdit Edit) : Message;

    /// <summary>The user typed the new name or the destination.</summary>
    public sealed record TagEditTextChanged(string Text) : Message;

    public sealed record TagEditConfirmed : Message;

    public sealed record TagEditCancelled : Message;

    /// <summary>What the change did, or why the database refused it.</summary>
    public sealed record TagEdited(Either<string, string> Result) : Message;

    /// <summary>The user changed the query; it is searched once the user pauses.</summary>
    public sealed record QueryChanged(string Query) : Message;

    /// <summary>The user wants the results now (Enter).</summary>
    public sealed record SearchRequested : Message;

    /// <summary>The pause after the query of <paramref name="Generation"/> is over.</summary>
    public sealed record Paused(int Generation) : Message;

    /// <summary>The result of the search of <paramref name="Generation"/>, or why it failed.</summary>
    public sealed record SearchFinished(int Generation, Either<string, SearchResult> Result) : Message;

    /// <summary>The user selected a result, or none.</summary>
    public sealed record ResultSelected(Option<FileAt> File) : Message;

    public sealed record DetailsLoaded(FileAt File, FileDetails Details) : Message;

    /// <summary>The user wants to open the file with the program the system has for it.</summary>
    public sealed record OpenRequested(FileAt File) : Message;
}
