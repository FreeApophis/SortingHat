using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using apophis.CLI.Writer;
using apophis.FileSystem;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files
{
    [UsedImplicitly]
    internal class CopyFilesCommand : ICommand
    {
        private readonly IFile _file;
        private readonly ICopyFile _copyFile;
        private readonly IConsoleWriter _consoleWriter;

        public CopyFilesCommand(IFile file, ICopyFile copyFile, IConsoleWriter consoleWriter)
        {
            _file = file;
            _copyFile = copyFile;
            _consoleWriter = consoleWriter;
        }

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            if (arguments.Count() != 2) throw new ArgumentOutOfRangeException(nameof(arguments));

            var search = arguments.First();
            var compbinedPath = Path.Combine(Directory.GetCurrentDirectory(), arguments.Last());

            _consoleWriter.WriteLine($"Find Files: {search}");

            var files = _file.Search(search).ToList();

            if (files.Any())
            {
                foreach (var file in GroupByHash(files))
                {
                    _consoleWriter.WriteLine();
                    foreach (var (fileName, index) in file.Paths.Select((path, index) => (path, index)))
                    {
                        _consoleWriter.WriteLine($"* {fileName} ({index})");
                    }

                    var selectedPath = "";
                    if (Path.GetFileName(selectedPath) is { } x)
                    {
                        _consoleWriter.WriteLine($"cp {file.Paths.First()} {Path.Combine(compbinedPath, x)}");
                        _copyFile.Copy(file.Paths.First(), Path.Combine(compbinedPath, x));
                    }
                }
            }
            else
            {
                _consoleWriter.WriteLine("No files found for your search query...");
            }

            return true;
        }

        private static IEnumerable<HashGroup> GroupByHash(List<API.Models.File> files)
        {
            return files.GroupBy(f => f.Hash.Result, (hash, file) => new HashGroup(hash, file.Select(f => f.Path)));
        }

        public string LongCommand => "copy-files";
        public Option<string> ShortCommand => Option.Some("cp");
        public string ShortHelp => "This command copies all files which match the search query to a specified folder location.";
        public CommandGrouping CommandGrouping => CommandGrouping.File;
    }
}
