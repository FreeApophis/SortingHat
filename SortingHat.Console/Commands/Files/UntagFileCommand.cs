using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Events;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class UntagFileCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, IFileScanner fileScanner, IFile fileStore, FileProgress progress, IFileEvents<FileUntagged> fileUntagged) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "untag-files";
    public string Description => "Removes tags from files.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [FileOptions.TagsAndFiles];
    public IReadOnlyList<OptionDefinition> Options => [FileOptions.Recursive];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var arguments = input.Values(FileOptions.TagsAndFiles);
        var tags = arguments.Where(a => a.IsTag).WhereSelect(tag => Tag.Parse(tag).RightOrNone()).ToList();

        return filePathExtractor.FromFilePatterns(arguments.Where(a => a.IsFile), input.Has(FileOptions.Recursive)).Match(
            left: problems => Task.FromResult(Report(problems)),
            right: paths => Untag(paths, tags, cancellationToken));
    }

    private bool Report(IEnumerable<string> problems)
    {
        problems.ForEach(consoleWriter.WriteLine);
        consoleWriter.WriteLine("Nothing was untagged.");

        return false;
    }

    private async Task<bool> Untag(IReadOnlyList<string> paths, IReadOnlyList<Tag> tags, CancellationToken cancellationToken)
    {
        await progress.ForEach("Untagging", paths, async path => await Untag((await fileScanner.Examine(path)).Current, tags, cancellationToken));

        return true;
    }

    /// <summary>The handlers hear of the file once, with all the tags, after they are removed.</summary>
    private async Task Untag(FileAt file, IReadOnlyList<Tag> tags, CancellationToken cancellationToken)
    {
        tags.ForEach(tag => fileStore.Untag(file.File, tag));

        if (tags.Count > 0)
        {
            (await fileUntagged.Publish(new FileUntagged(file, tags), cancellationToken)).ForEach(consoleWriter.WriteLine);
        }
    }
}
