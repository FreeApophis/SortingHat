using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class RepairCommand(IFile fileStore, IExistsFile existsFile, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private static readonly OptionDefinition DryRun = new() { Name = "dry-run", ShortName = "n", Description = "Shows the missing paths, without changing the database." };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "repair";
    public string Description => "Removes the paths of files that no longer exist from the database.";

    public string Details
        => """
           The files keep their tags and names: when the same content turns up at a new place,
           'tag-files <path>' links it to them again.
           """;

    public IReadOnlyList<OptionDefinition> Options => [DryRun];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var missingPaths = fileStore.GetPaths().Where(path => !existsFile.Exists(path)).Distinct().ToList();

        if (missingPaths.Count == 0)
        {
            consoleWriter.WriteLine("Every path in the database exists.");
        }
        else if (input.Has(DryRun))
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

        return Task.FromResult(true);
    }
}
