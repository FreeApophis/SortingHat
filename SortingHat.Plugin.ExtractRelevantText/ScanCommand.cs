using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.API.DI;

namespace SortingHat.Plugin.ExtractRelevantText;

internal class ScanCommand(FolderScanner folderScanner) : ICommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;

    public string LongCommand => "scan";

    public Option<string> ShortCommand => Option.Some("s");

    public string ShortHelp => "scans a folder and its content";

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        return folderScanner.Scan(arguments);
    }
}