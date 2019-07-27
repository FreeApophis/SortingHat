using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using File = SortingHat.API.Models.File;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class IndexCommand : ICommand
    {
        private readonly Func<File> _newFile;
        private readonly IFile _file;

        [UsedImplicitly]
        public IndexCommand(Func<File> newFile, IFile file)
        {
            _newFile = newFile;
            _file = file;
        }
        public CommandGrouping CommandGrouping => CommandGrouping.General;
        public string LongCommand => "index";
        public Option<string> ShortCommand => Option.Some("i");
        public string ShortHelp => "index files in the path recursivly without taggging.";
        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            return IndexFiles(arguments.FirstOrDefault() ?? ".");
        }

        private bool IndexFiles(string path)
        {
            return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).All(StoreFile);
        }

        private bool StoreFile(string filePath)
        {
            var file = _newFile();

            file.Path = filePath;
            file.LoadByPath();

            return _file.Store(file);
        }
    }
}
