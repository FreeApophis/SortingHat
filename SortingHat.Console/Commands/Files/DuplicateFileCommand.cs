using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class DuplicateFileCommand(IFile fileStore, IConsoleWriter consoleWriter) : ICommand
{
    public string LongCommand => "duplicate";
    public Option<string> ShortCommand => Option.Some("d");
    public string ShortHelp => "Find duplicate files in your dms";
    public CommandGrouping CommandGrouping => CommandGrouping.File;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        foreach (var file in fileStore.GetDuplicates())
        {
            consoleWriter.WriteLine("--- Duplicate ---");
            consoleWriter.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
            consoleWriter.WriteLine($"File Size: {file.Size}");
            consoleWriter.WriteLine($"File Tags: {string.Join(", ", file.GetTags().Result.Select(t => t.FullName))}");

            var table = new ConsoleTable(5);
            foreach (var path in file.GetPaths().Result)
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

        return true;
    }
}
