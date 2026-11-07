using System;
using System.Threading.Tasks;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

/// <summary>The arguments of copy-files and move-files, which both run a search and put the files into a folder.</summary>
internal static class FileOperationArguments
{
    public const string Details = "When several files have the same content, hat asks which one to take.";

    public static readonly ArgumentDefinition Search = new() { Name = "search", Description = "A tag or a search query in quotes, for example ':tax:2018' or \":tax and not :bank\"." };

    public static readonly ArgumentDefinition Destination = new() { Name = "destination", Description = "The folder for the files." };

    public static Task<bool> Run(CommandInput input, Func<string, string, Task<bool>> exportFiles)
        => (from search in input.Value(Search)
            from destination in input.Value(Destination)
            select exportFiles(search, destination))
            .GetOrElse(Task.FromResult(false));
}
