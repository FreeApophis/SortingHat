using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.API.DI;

namespace SortingHat.Plugin.ExtractRelevantText
{
    internal class ScanCommand : ICommand
    {
        private readonly FolderScanner _folderScanner;

        public ScanCommand(FolderScanner folderScanner)
        {
            _folderScanner = folderScanner;
        }

        public CommandGrouping CommandGrouping => CommandGrouping.File;

        public string LongCommand => "scan";

        public Option<string> ShortCommand => Option.Some("s");

        public string ShortHelp => "scans a folder and its content";

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            return _folderScanner.Scan(arguments);
        }
    }
}