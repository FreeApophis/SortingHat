using System.Collections.Generic;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.Plugin.FileType.Detectors;

namespace SortingHat.Plugin.FileType;

[UsedImplicitly]
internal class IdentifyCommand(IConsoleWriter consoleWriter, IFileTypeFinder fileTypeFinder, IFilePathExtractor filePathExtractor) : ICommand
{
    public string LongCommand => "identify";
    public Option<string> ShortCommand => Option.Some("id");
    public string ShortHelp => "This command identifies the real file type (ignoring the file-extension)";
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        foreach (var argument in filePathExtractor.FromFilePatterns(arguments, false))
        {
            consoleWriter.WriteLine($"File: {argument}");

            if (FileType(argument) is { } fileType)
            {
                consoleWriter.WriteLine($"Category : {fileType.Category}");
                consoleWriter.WriteLine($"Name     : {fileType.Name}");
                consoleWriter.WriteLine($"Extension: {string.Join(",", fileType.Extensions)}");
            }
            else
            {
                consoleWriter.WriteLine("Unknown filetype");
            }
        }

        return true;
    }

    private FileType? FileType(string argument) => fileTypeFinder.Identify(new System.IO.FileInfo(argument));
}
