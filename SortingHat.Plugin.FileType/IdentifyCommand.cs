using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.Plugin.FileType.Detectors;
using System.Collections.Generic;
using Funcky.Monads;
using SortingHat.CliAbstractions;

namespace SortingHat.Plugin.FileType
{
    [UsedImplicitly]
    internal class IdentifyCommand : ICommand
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly IFileTypeFinder _fileTypeFinder;
        private readonly IFilePathExtractor _filePathExtractor;

        public IdentifyCommand(IConsoleWriter consoleWriter, IFileTypeFinder fileTypeFinder, IFilePathExtractor filePathExtractor)
        {
            _consoleWriter = consoleWriter;
            _fileTypeFinder = fileTypeFinder;
            _filePathExtractor = filePathExtractor;
        }

        private FileType FileType(string argument) => _fileTypeFinder.Identify(new System.IO.FileInfo(argument));

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            foreach (var argument in _filePathExtractor.FromFilePatterns(arguments))
            {
                _consoleWriter.WriteLine($"File: {argument}");
                var fileType = FileType(argument);

                if (fileType != null)
                {
                    _consoleWriter.WriteLine($"Category : {fileType.Category}");
                    _consoleWriter.WriteLine($"Name     : {fileType.Name}");
                    _consoleWriter.WriteLine($"Extension: {string.Join(",", fileType.Extensions)}");
                } else
                {
                    _consoleWriter.WriteLine("Unknown filetype");
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
