using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API;
using System.Collections.Generic;
using System;

namespace SortingHat.Plugin.FileType
{
    [UsedImplicitly]
    internal class IdentifyCommand : ICommand
    {
        private readonly IFileTypeFinder _fileTypeFinder;

        public IdentifyCommand(IFileTypeFinder fileTypeFinder)
        {
            _fileTypeFinder = fileTypeFinder;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var filePathExtractor = new FilePathExtractor(arguments);

            foreach (var argument in filePathExtractor.FilePaths)
            {
                Console.WriteLine($"File: {argument}");
                var fileType = _fileTypeFinder.Identify(argument);

                if (fileType != null)
                {
                    Console.WriteLine($"C: {fileType.Category}");
                    Console.WriteLine($"N: {fileType.Name}");
                    Console.WriteLine($"E: {string.Join(",", fileType.Extensions)}");
                }
                else
                {
                    Console.WriteLine("Unknown filetype");
                }
            }
            return true;
        }

        public string LongCommand => "identify";
        public string ShortCommand => "id";
        public string ShortHelp => "This command identifies the real file type (ignoring the file-extension)";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

    }
}
