﻿using System;
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
    internal class MoveFilesCommand : ICommand
    {
        private readonly IFile _file;
        private readonly IMoveFile _moveFile;
        private readonly IConsoleWriter _consoleWriter;

        public MoveFilesCommand(IFile file, IMoveFile moveFile, IConsoleWriter consoleWriter)
        {
            _file = file;
            _moveFile = moveFile;
            _consoleWriter = consoleWriter;
        }

        public bool Execute(IEnumerable<string> larzyArguments, IOptionParser options)
        {
            var arguments = larzyArguments.ToList();
            if (arguments.Count != 2) { throw new ArgumentOutOfRangeException(nameof(arguments)); }

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
                        _moveFile.Move(file.Path, Path.Combine(path, fileName));
                    }
                }
            }
            else
            {
                _consoleWriter.WriteLine("No files found for your search query...");
            }

            return true;
        }

        public string LongCommand => "move-files";
        public Option<string> ShortCommand => Option.Some("mv");
        public string ShortHelp => "This moves all files which match the search query to a specified folder location";
        public CommandGrouping CommandGrouping => CommandGrouping.File;

    }
}