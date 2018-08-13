using System.Collections.Generic;
using System;

namespace SortingHat.CLI.Commands
{
    class FileInfoCommand : ICommand
    {
        public FileInfoCommand()
        {
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var filePaths = new FilePathExtractor(arguments);

            foreach (var filePath in filePaths.FilePaths)
            {
                
                Console.WriteLine($"File: {filePath}");
                Console.WriteLine($"File not in index");
            }

            return true;
        }

        public string LongCommand => "file-info";
        public string ShortCommand => null;
        public string ShortHelp => "Shows file information of a certain file...";
    }
}
