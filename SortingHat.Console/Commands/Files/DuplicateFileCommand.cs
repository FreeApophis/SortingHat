using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using Spectre.Console;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class DuplicateFileCommand(IFile fileStore, IConsoleWriter consoleWriter, IAnsiConsole console) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "duplicate";
    public IReadOnlyList<string> Aliases => ["d"];
    public string Description => "Lists the files whose content is stored under several paths.";
    public string Details => "Duplicates are files with the same content, stored under several paths.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var duplicates = fileStore.GetDuplicates().ToList();

        if (duplicates is [])
        {
            consoleWriter.WriteLine("There are no duplicates.");
        }

        foreach (var file in duplicates)
        {
            consoleWriter.WriteLine("--- Duplicate ---");
            consoleWriter.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
            consoleWriter.WriteLine($"File Size: {file.Size}");
            consoleWriter.WriteLine($"File Tags: {string.Join(", ", fileStore.GetTags(file).Select(t => t.FullName))}");

            var table = LightTable.Create(LightTable.Column("Created"), LightTable.Column("Size").RightAligned(), LightTable.Column("Folder"), LightTable.Column("Name"));
            fileStore.GetPaths(file).Select(path => new FileInfo(path)).ForEach(info => table.AddCells(Row(info)));

            console.Write(table);
            consoleWriter.WriteLine();
        }

        return Task.FromResult(true);
    }

    /// <summary>A path the database knows, but where the file is gone, is 'missing'.</summary>
    private static IEnumerable<string> Row(FileInfo info)
        =>
        [
            info.Exists ? $"{info.CreationTimeUtc}" : "missing",
            info.Exists ? info.Length.HumanSize() : string.Empty,
            Option.FromNullable(info.DirectoryName).GetOrElse(string.Empty),
            info.Name,
        ];
}
