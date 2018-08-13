using System.Collections.Generic;
using System;

namespace SortingHat.CLI.Commands
{
    class FileInfoCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            foreach (var file in arguments)
            {
                Console.WriteLine($"File: {file}");

            }

            return true;
        }

        public string LongCommand => "file-info";
        public string ShortCommand => null;
        public string ShortHelp => "Shows file information of a certain file...";
    }
}
