using System.Collections.Generic;
using System.Linq;
using Apophis.CLI;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.CLI.Options;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class FindFilesCommand(IFile fileStore, IConsoleWriter consoleWriter) : ICommand
{
    public string LongCommand => "find-files";
    public Option<string> ShortCommand => Option.Some("ff");
    public string ShortHelp => "Finds all files matching the search query";
    public CommandGrouping CommandGrouping => CommandGrouping.File;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        var search = string.Join(" ", arguments);
        consoleWriter.WriteLine($"Find Files: {search}");

        var result = fileStore.Search(search);
        var files = result.Files;

        foreach (var unknownTag in result.UnknownTags)
        {
            consoleWriter.WriteLine($"Unknown tag: {unknownTag}");
        }

        if (files.Any())
        {
            var table = FileTable();
            foreach (var file in files)
            {
                if (options.HasOption<OpenOption>())
                {
                    FileHelper.OpenWithAssociatedProgram(file.Path);
                }

                var hash = file.Hash.Result;
                table.Append(hash.ShortHash().GetOrElse(hash), file.CreatedAt, file.Size.HumanSize(), file.Path);
            }

            table.WriteTo(consoleWriter);
        }
        else
        {
            consoleWriter.WriteLine("No files found for your search query...");
        }

        return true;
    }

    private ConsoleTable FileTable()
    {
        var table = new ConsoleTable(4);

        return table;
    }
}
