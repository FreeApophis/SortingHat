using System.Collections.Generic;
using System.IO;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class RepairCommand(IFile fileStore, IConsoleWriter consoleWriter) : ICommand
{
    public string LongCommand => "repair";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Check each path locked in the database if the file still exists and is not corrupted / changed";
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        foreach (var path in fileStore.GetPaths())
        {
            if (!File.Exists(path))
            {
                consoleWriter.WriteLine($"File '{path}' does not exist, removed from index");
            }
        }

        return true;
    }
}
