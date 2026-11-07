using System;
using System.Threading.Tasks;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

/// <summary>The arguments and options of copy-files and move-files, which both run a search and put the files into a folder.</summary>
internal static class FileOperationArguments
{
    public const string Details
        = "Nothing is overwritten: a file whose name already exists in the destination is skipped and reported, "
        + "like a file that no longer exists on disk. The other files still go, the exit code says whether all of them did. "
        + "When several copies of the same content exist, hat asks which one to take. "
        + "With --dry-run, hat shows all of this without changing any file or the database.";

    public static readonly ArgumentDefinition Search = new() { Name = "search", Description = "A tag or a search query in quotes, for example ':tax:2018' or \":tax and not :bank\"." };

    public static readonly ArgumentDefinition Destination = new() { Name = "destination", Description = "The folder for the files." };

    public static readonly OptionDefinition DryRun = new() { Name = "dry-run", ShortName = "n", Description = "Shows what would happen, without changing any file or the database." };

    public static Task<bool> Run(CommandInput input, Func<string, string, bool, Task<bool>> exportFiles)
        => (from search in input.Value(Search)
            from destination in input.Value(Destination)
            select exportFiles(search, destination, input.Has(DryRun)))
            .GetOrElse(Task.FromResult(false));
}
