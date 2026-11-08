using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Events;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class RepairCommand(IFile fileStore, IExistsFile existsFile, IFileScanner fileScanner, IConsoleWriter consoleWriter, FileProgress progress, IFileEvents<FileChanged> fileChanged) : IDeclaredCommand
{
    private static readonly OptionDefinition DryRun = new() { Name = "dry-run", ShortName = "n", Description = "Shows what would be repaired, without changing the database." };

    private static readonly OptionDefinition Rehash = new() { Name = "rehash", Description = "Also reads every existing file and hashes it again, to find changes that kept the size and the timestamp." };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "repair";
    public string Description => "Removes the paths of files that no longer exist from the database.";

    public string Details
        => """
           The files keep their tags and names: when the same content turns up at a new place,
           'tag-files <path>' links it to them again.
           With --rehash, a file whose content has changed keeps its path, and its tags are carried over to the new content.
           """;

    public IReadOnlyList<OptionDefinition> Options => [DryRun, Rehash];

    public async Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var paths = fileStore.GetPaths().Distinct().ToList();
        var missingPaths = paths.Where(path => !existsFile.Exists(path)).ToList();

        RemoveMissingPaths(missingPaths, input.Has(DryRun));

        if (input.Has(Rehash))
        {
            await RehashPaths([.. paths.Except(missingPaths)], input.Has(DryRun), cancellationToken);
        }

        return true;
    }

    private void RemoveMissingPaths(List<string> missingPaths, bool dryRun)
    {
        if (missingPaths.Count == 0)
        {
            consoleWriter.WriteLine("Every path in the database exists.");
        }
        else if (dryRun)
        {
            missingPaths.ForEach(path => consoleWriter.WriteLine($"would remove: {path}"));
            consoleWriter.WriteLine("Dry run: nothing was removed.");
        }
        else
        {
            fileStore.RemovePaths(missingPaths);
            missingPaths.ForEach(path => consoleWriter.WriteLine($"removed: {path}"));
            consoleWriter.WriteLine($"{missingPaths.Count} missing paths removed, the files keep their tags.");
        }
    }

    /// <summary>Storing a file hashed from disk moves its path to the new content and carries the tags over.</summary>
    private async Task RehashPaths(IReadOnlyList<string> paths, bool dryRun, CancellationToken cancellationToken)
    {
        var changed = 0;

        await progress.ForEach("Hashing", paths, async path =>
        {
            var state = PathState.Decide(fileStore.Find(path), await fileScanner.Scan(path));

            if (state is PathState.Changed)
            {
                changed++;
                consoleWriter.WriteLine(dryRun
                    ? $"'{path}' has changed, its tags would be carried over to the new content."
                    : $"'{path}' has changed, its tags were carried over to the new content.");
            }

            if (!dryRun)
            {
                fileStore.Store(state.Current);
            }

            if (!dryRun && state is PathState.Changed changedPath)
            {
                (await fileChanged.Publish(new FileChanged(changedPath.Stored, changedPath.Current), cancellationToken)).ForEach(consoleWriter.WriteLine);
            }
        });

        consoleWriter.WriteLine(dryRun
            ? $"Dry run: {paths.Count} files hashed again, {changed} changed, no hash was updated."
            : $"{paths.Count} files hashed again, {changed} changed.");
    }
}
