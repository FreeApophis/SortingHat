using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class FileInfoCommand : ICommand
    {
        private readonly Func<File> _newFile;

        public FileInfoCommand(Func<File> newFile)
        {
            _newFile = newFile;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var filePaths = new FilePathExtractor(arguments);

            foreach (var filePath in filePaths.FilePaths)
            {
                Console.WriteLine();
                Console.WriteLine($"File: {filePath}");

                var file = _newFile();
                file.Path = filePath;
                file.LoadByPath(true);
                if (string.IsNullOrEmpty(file.Hash.Result))
                {
                    Console.WriteLine("File not in index!");
                }
                else
                {
                    Console.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
                    Console.WriteLine($"File Size: {file.Size}");
                    Console.WriteLine($"File Hash: {file.Hash.Result}");

                    foreach (var tag in file.GetTags())
                    {
                        Console.WriteLine($"Tag: {tag.FullName}");
                    }

                    foreach (var name in file.GetNames())
                    {
                        Console.WriteLine($"Name: {name}");
                    }

                    foreach (var path in file.GetPaths())
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
    }
}
