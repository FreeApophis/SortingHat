using JetBrains.Annotations;
using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky.Monads;
using SortingHat.CliAbstractions;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class CopyFilesCommand : ICommand
    {
        private readonly IFile _file;
        private readonly IConsoleWriter _consoleWriter;

        public CopyFilesCommand(IFile file, IConsoleWriter consoleWriter)
        {
            _file = file;
            _consoleWriter = consoleWriter;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            if (arguments.Count() != 2) throw new ArgumentOutOfRangeException(nameof(arguments));

            var search = arguments.First();
            var path = Path.Combine(Directory.GetCurrentDirectory(), arguments.Last());

            _consoleWriter.WriteLine($"Find Files: {search}");

            var files = _file.Search(search).ToList();

            if (files.Any())
            {
                foreach (var file in files)
                {
                    if (Path.GetFileName(file.Path) is { } fileName)
                    {
                        _consoleWriter.WriteLine($"cp {file.Path} {Path.Combine(path, fileName)}");
                        File.Copy(file.Path, Path.Combine(path, fileName));
                    }
                }
            } else
            {
                _consoleWriter.WriteLine("No files found for your search query...");
            }

            return true;
        }

        public string LongCommand => "copy-files";
        public Option<string> ShortCommand => Option.Some("cp");
        public string ShortHelp => "This command copies all files which match the search query to a specified folder location.";
        public CommandGrouping CommandGrouping => CommandGrouping.File;
    }
}
