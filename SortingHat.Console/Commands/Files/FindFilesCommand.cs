using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class FindFilesCommand(IFile fileStore, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Query = new()
    {
        Name = "query",
        Description = "Tags combined with and, or, not and parentheses, for example ':tax:2018 and not :bank'. The words are joined, quotes are optional.",
        Arity = Arity.OneOrMore,
    };

    private static readonly OptionDefinition Open = new() { Name = "open", Description = "Opens the file with the associated program." };

    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "find-files";
    public IReadOnlyList<string> Aliases => ["ff"];
    public string Description => "Finds all files matching the search query";
    public string Details => "A tag also finds the files tagged with its children: ':tax' finds ':tax:2018'.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Query];
    public IReadOnlyList<OptionDefinition> Options => [Open];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var search = string.Join(" ", input.Values(Query));
        consoleWriter.WriteLine($"Find Files: {search}");

        var result = fileStore.Search(search);
        var files = result.Files.ToList();

        foreach (var unknownTag in result.UnknownTags)
        {
            consoleWriter.WriteLine($"Unknown tag: {unknownTag}");
        }

        if (files.Any())
        {
            var table = new ConsoleTable(4);
            foreach (var file in files)
            {
                if (input.Has(Open))
                {
                    FileHelper.OpenWithAssociatedProgram(file.Path);
                }

                table.Append(file.File.Hash.Short, file.File.CreatedAt, file.File.Size.HumanSize(), file.Path);
            }

            table.WriteTo(consoleWriter);
        }
        else
        {
            consoleWriter.WriteLine("No files found for your search query...");
        }

        return Task.FromResult(true);
    }
}
