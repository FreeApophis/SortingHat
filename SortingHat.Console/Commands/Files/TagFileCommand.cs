using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Serilog;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Events;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class TagFileCommand(ILogger logger, IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, IFileScanner fileScanner, IFile fileStore, IAutoTagHandler autoTagHandler, FileProgress progress, IFileEvents<FileTagged> fileTagged) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "tag-files";
    public IReadOnlyList<string> Aliases => ["tag"];
    public string Description => "Tags files and adds them to the database.";

    public string Details
        => """
           Without tags, the files are only added to the database.
           A tag can contain variables, which are replaced per file, for example ':Files:{FileType.Category}'.
           'hat help' lists the variables of the loaded plugins, 'hat file-info --variables <file>' shows their values for a file.
           A file without a value for a variable does not get that tag.
           """;

    public IReadOnlyList<ArgumentDefinition> Arguments => [FileOptions.TagsAndFiles];
    public IReadOnlyList<OptionDefinition> Options => [FileOptions.Recursive];

    public async Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var arguments = input.Values(FileOptions.TagsAndFiles);
        var masks = arguments.Where(argument => argument.IsTag).Select(autoTagHandler.ParseMask).ToList();
        var paths = filePathExtractor.FromFilePatterns(arguments.Where(argument => argument.IsFile), input.Has(FileOptions.Recursive));
        IReadOnlyList<string> problems = [.. masks.WhereSelect(mask => mask.LeftOrNone()), .. paths.LeftOrNone().GetOrElse([])];

        if (problems.Count > 0)
        {
            problems.ForEach(consoleWriter.WriteLine);
            consoleWriter.WriteLine("Nothing was tagged.");

            return false;
        }

        var skipped = new List<(TagMask Mask, string Reason)>();
        IReadOnlyList<TagMask> validMasks = [.. masks.WhereSelect(mask => mask.RightOrNone())];

        await progress.ForEach("Tagging", paths.RightOrNone().GetOrElse([]), async path => skipped.AddRange(await TagFile(path, validMasks, cancellationToken)));

        Summary(skipped).ForEach(consoleWriter.WriteLine);

        return true;
    }

    /// <summary>One line per mask and reason, not per file: tagging a folder of documents with a camera variable skips every document.</summary>
    private static IEnumerable<string> Summary(IEnumerable<(TagMask Mask, string Reason)> skipped)
        => skipped
            .GroupBy(skip => (Mask: skip.Mask.ToString(), skip.Reason))
            .Select(group => $"'{group.Key.Mask}': {Files(group.Count())} not tagged, {group.Key.Reason}.");

    private static string Files(int count)
        => count == 1 ? "1 file" : $"{count} files";

    /// <summary>The handlers of plugins hear of the file once, with all its new tags, after they are stored.</summary>
    /// <returns>The masks that gave no tag for this file, and why.</returns>
    private async Task<IReadOnlyList<(TagMask Mask, string Reason)>> TagFile(string path, IReadOnlyList<TagMask> masks, CancellationToken cancellationToken)
    {
        var state = await fileScanner.Examine(path);

        if (state is PathState.Changed)
        {
            consoleWriter.WriteLine($"'{path}' has changed, its tags were carried over to the new content.");
        }

        if (masks.Count == 0)
        {
            fileStore.Store(state.Current);

            return [];
        }

        var file = new AutoTagFile(new FileInfo(path));
        var tags = masks.Select(mask => (Mask: mask, Tag: mask.Apply(file))).ToList();

        IReadOnlyList<Tag> applied = [.. tags.WhereSelect(tag => tag.Tag.RightOrNone())];
        applied.ForEach(tag => Tag(state.Current, tag));

        if (applied.Count > 0)
        {
            (await fileTagged.Publish(new FileTagged(state.Current, applied), cancellationToken)).ForEach(consoleWriter.WriteLine);
        }

        return [.. tags.WhereSelect(tag => tag.Tag.LeftOrNone().Select(reason => (tag.Mask, reason)))];
    }

    private void Tag(FileAt file, Tag tag)
    {
        logger.Information("{Path} tagged with {Tag}", file.Path, tag.FullName);

        consoleWriter.WriteLine($"{file.Path} tagged with {tag.FullName}");
        fileStore.Tag(file, tag);
    }
}
