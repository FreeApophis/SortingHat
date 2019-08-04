using System.Collections.Generic;
using apophis.FileSystem;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files
{
    [UsedImplicitly]
    internal sealed class CopyFilesCommand : ICommand
    {
        private readonly FileOperations<ICopyFile> _copyFileOperation;

        public CopyFilesCommand(FileOperations<ICopyFile> copyFileOperation)
        {
            _copyFileOperation = copyFileOperation;
        }

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            return _copyFileOperation.ExportFiles(arguments, options, LongCommand);
        }

        public string LongCommand => "copy-files";
        public Option<string> ShortCommand => Option.Some("cp");
        public string ShortHelp => "This command copies all files which match the search query to a specified folder location.";
        public CommandGrouping CommandGrouping => CommandGrouping.File;
    }
}
