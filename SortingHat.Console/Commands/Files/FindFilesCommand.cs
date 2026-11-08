using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;
using Spectre.Console;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class FindFilesCommand(IFile fileStore, IConsoleWriter consoleWriter, IAnsiConsole console) : IDeclaredCommand
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

        return Task.FromResult(fileStore.Search(search).Match(left: Report, right: result => Show(result, input.Has(Open))));
    }

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }

    private bool Show(SearchResult result, bool open)
    {
        var files = result.Files.ToList();

        foreach (var unknownTag in result.UnknownTags)
        {
            consoleWriter.WriteLine($"Unknown tag: {unknownTag}");
        }

        if (files.Any())
        {
            var table = LightTable.Create(LightTable.Column("Hash"), LightTable.Column("Created"), LightTable.Column("Size").RightAligned(), LightTable.Column("Path"));
            foreach (var file in files)
            {
                if (open)
                {
                    FileHelper.OpenWithAssociatedProgram(file.Path);
                }

                table.AddCells(file.File.Hash.Short, $"{file.File.CreatedAt}", file.File.Size.HumanSize(), file.Path);
            }

            console.Write(table);
        }
        else
        {
            consoleWriter.WriteLine("No files found for your search query...");
        }

        return true;
    }
}
