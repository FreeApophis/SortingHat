using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.Plugin.FileType.Detectors;
using System;
using System.Collections.Generic;
using Funcky.Monads;

namespace SortingHat.Plugin.FileType
{
    [UsedImplicitly]
    internal class IdentifyCommand : ICommand
    {
        private readonly IFileTypeFinder _fileTypeFinder;
        private readonly IFilePathExtractor _filePathExtractor;

        public IdentifyCommand(IFileTypeFinder fileTypeFinder, IFilePathExtractor filePathExtractor)
        {
            _fileTypeFinder = fileTypeFinder;
            _filePathExtractor = filePathExtractor;
        }

        private FileType FileType(string argument) => _fileTypeFinder.Identify(new System.IO.FileInfo(argument));

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            foreach (var argument in _filePathExtractor.FromFilePatterns(arguments))
            {
                Console.WriteLine($"File: {argument}");
                var fileType = FileType(argument);

                if (fileType != null)
                {
                    Console.WriteLine($"Category : {fileType.Category}");
                    Console.WriteLine($"Name     : {fileType.Name}");
                    Console.WriteLine($"Extension: {string.Join(",", fileType.Extensions)}");
                } else
                {
                    Console.WriteLine("Unknown filetype");
                }
            }

            return true;
        }

        public string LongCommand => "identify";
        public Option<string> ShortCommand => Option.Some("id");
        public string ShortHelp => "This command identifies the real file type (ignoring the file-extension)";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

    }
}
