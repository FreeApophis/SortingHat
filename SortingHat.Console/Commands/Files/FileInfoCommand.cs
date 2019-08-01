using System;
using System.Collections.Generic;
using apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.CLI.Options;

namespace SortingHat.CLI.Commands.Files
{
    [UsedImplicitly]
    internal class FileInfoCommand : ICommand
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly IFilePathExtractor _filePathExtractor;
        private readonly Func<File> _newFile;

        public FileInfoCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, Func<File> newFile)
        {
            _consoleWriter = consoleWriter;
            _filePathExtractor = filePathExtractor;
            _newFile = newFile;
        }

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            foreach (var filePath in _filePathExtractor.FromFilePatterns(arguments, options.HasOption(new RecursiveOption())))
            {
                _consoleWriter.WriteLine();
                _consoleWriter.WriteLine($"File: {filePath}");

                var file = _newFile();

                file.LoadByPathFromDb(filePath);
                if (file.Hash == null)
                {
                    _consoleWriter.WriteLine("File not in index!");
                }
                else
                {
                    _consoleWriter.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
                    _consoleWriter.WriteLine($"File Size: {file.Size}");
                    _consoleWriter.WriteLine($"File Hash: {file.Hash.Result}");

                    foreach (var tag in file.GetTags().Result)
                    {
                        _consoleWriter.WriteLine($"Tag: {tag.FullName}");
                    }

                    foreach (var name in file.GetNames().Result)
                    {
                        _consoleWriter.WriteLine($"Name: {name}");
                    }

                    foreach (var path in file.GetPaths().Result)
                    {
                        _consoleWriter.WriteLine($"Path: {path}");
                    }
                }
            }

            return true;
        }

        public string LongCommand => "file-info";
        public Option<string> ShortCommand => Option.Some("info");
        public string ShortHelp => "Shows all available information about the current file.";
        public CommandGrouping CommandGrouping => CommandGrouping.File;

    }
}
