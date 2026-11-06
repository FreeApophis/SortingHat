using System.Collections.Generic;
using Apophis.FileSystem;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class MoveFilesCommand(FileOperations<IMoveFile> moveFileOperation) : ICommand
{
    public string LongCommand => "move-files";
    public Option<string> ShortCommand => Option.Some("mv");
    public string ShortHelp => "This moves all files which match the search query to a specified folder location";
    public CommandGrouping CommandGrouping => CommandGrouping.File;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        return moveFileOperation.ExportFiles(arguments, options, LongCommand);
    }
}
