using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System;
using System.Collections.Generic;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class FileInfoCommand : ICommand
    {
        private readonly IFilePathExtractor _filePathExtractor;
        private readonly Func<File> _newFile;

        public FileInfoCommand(IFilePathExtractor filePathExtractor, Func<File> newFile)
        {
            _filePathExtractor = filePathExtractor;
            _newFile = newFile;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            foreach (var filePath in _filePathExtractor.FromFilePatterns(arguments))
            {
                Console.WriteLine();
                Console.WriteLine($"File: {filePath}");

                var file = _newFile();
                file.Path = filePath;
                file.DBLoadByPath();
                if (file.Hash == null)
                {
                    Console.WriteLine("File not in index!");
                }
                else
                {
                    Console.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
                    Console.WriteLine($"File Size: {file.Size}");
                    Console.WriteLine($"File Hash: {file.Hash.Result}");

                    foreach (var tag in file.GetTags().Result)
                    {
                        Console.WriteLine($"Tag: {tag.FullName}");
                    }

                    foreach (var name in file.GetNames().Result)
                    {
                        Console.WriteLine($"Name: {name}");
                    }

                    foreach (var path in file.GetPaths().Result)
                    {
                        Console.WriteLine($"Path: {path}");
                    }
                }
            }

            return true;
        }

        public string LongCommand => "file-info";
        public string ShortCommand => "info";
        public string ShortHelp => "Shows all available information about the current file.";
        public CommandGrouping CommandGrouping => CommandGrouping.File;

    }
}
