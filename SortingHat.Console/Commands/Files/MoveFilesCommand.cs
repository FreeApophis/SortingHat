using System.Collections.Generic;
using apophis.FileSystem;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files
{
    [UsedImplicitly]
    internal class MoveFilesCommand : ICommand
    {
        private readonly FileOperations<IMoveFile> _moveFileOperation;

        public MoveFilesCommand(FileOperations<IMoveFile> moveFileOperation)
        {
            _moveFileOperation = moveFileOperation;
        }

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            return _moveFileOperation.ExportFiles(arguments, options, LongCommand);
        }

        public string LongCommand => "move-files";
        public Option<string> ShortCommand => Option.Some("mv");
        public string ShortHelp => "This moves all files which match the search query to a specified folder location";
        public CommandGrouping CommandGrouping => CommandGrouping.File;

    }
}
