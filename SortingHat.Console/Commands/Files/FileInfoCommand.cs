using System;
using System.Collections.Generic;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.CLI.Options;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class FileInfoCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, Func<File> newFile) : ICommand
{
    public string LongCommand => "file-info";
    public Option<string> ShortCommand => Option.Some("info");
    public string ShortHelp => "Shows all available information about the current file.";
    public CommandGrouping CommandGrouping => CommandGrouping.File;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        foreach (var filePath in filePathExtractor.FromFilePatterns(arguments, options.HasOption<RecursiveOption>()))
        {
            consoleWriter.WriteLine();
            consoleWriter.WriteLine($"File: {filePath}");

            var file = newFile();

            file.LoadByPathFromDb(filePath);
            if (file.Hash == null)
            {
                consoleWriter.WriteLine("File not in index!");
            }
            else
            {
                consoleWriter.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
                consoleWriter.WriteLine($"File Size: {file.Size}");
                consoleWriter.WriteLine($"File Hash: {file.Hash.Result}");

                foreach (var tag in file.GetTags().Result)
                {
                    consoleWriter.WriteLine($"Tag: {tag.FullName}");
                }

                foreach (var name in file.GetNames().Result)
                {
                    consoleWriter.WriteLine($"Name: {name}");
                }

                foreach (var path in file.GetPaths().Result)
                {
                    consoleWriter.WriteLine($"Path: {path}");
                }
            }
        }

        return true;
    }
}
