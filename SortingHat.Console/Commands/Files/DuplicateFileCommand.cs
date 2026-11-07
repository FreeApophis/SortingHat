using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class DuplicateFileCommand(IFile fileStore, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "duplicate";
    public IReadOnlyList<string> Aliases => ["d"];
    public string Description => "Find duplicate files in your dms";
    public string Details => "Duplicates are files with the same content, stored under several paths.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        foreach (var file in fileStore.GetDuplicates())
        {
            consoleWriter.WriteLine("--- Duplicate ---");
            consoleWriter.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
            consoleWriter.WriteLine($"File Size: {file.Size}");
            consoleWriter.WriteLine($"File Tags: {string.Join(", ", fileStore.GetTags(file).Select(t => t.FullName))}");

            var table = new ConsoleTable(5);
            foreach (var path in fileStore.GetPaths(file))
            {
                var fileInfo = new FileInfo(path);

                if (fileInfo.Exists)
                {
                    table.Append(fileInfo.CreationTimeUtc, fileInfo.Length.HumanSize(), fileInfo.Name, string.Empty, fileInfo.DirectoryName);
                }
                else
                {
                    table.Append(string.Empty, string.Empty, fileInfo.Name, "*", fileInfo.DirectoryName);
                }
            }

            table.WriteTo(consoleWriter);
            consoleWriter.WriteLine();
        }

        consoleWriter.WriteLine("Files with an asterisk (*) are in the database, but at the given path they seem to be deleted.");

        return Task.FromResult(true);
    }
}
