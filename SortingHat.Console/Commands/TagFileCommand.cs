using SortingHat.API.DI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace SortingHat.CLI.Commands
{
    class TagFileCommand : ICommand
    {
        private const string Command = "tag-file";
        private readonly IDatabase _db;
        private readonly IHashService _hashService;

        public TagFileCommand(IDatabase db, IHashService hashService)
        {
            _db = db;
            _hashService = hashService;
        }

        private static bool IsTag(string value)
        {
            return value.StartsWith(":");
        }

        private static bool IsFile(string value)
        {
            return value.StartsWith(":") == false;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            var tags = arguments.Skip(1).Where(IsTag);
            var files = GetFilePaths(arguments.Skip(1).Where(IsFile));

            foreach (var file in files.Select(file => new API.Models.File(file, _hashService)))
            {
                foreach (var tag in tags.Select(tag => API.Models.Tag.Parse(tag)))
                {
                    file.Tag(_db, tag);
                }
            }

            return true;

        }

        private static IEnumerable<string> GetFilePaths(IEnumerable<string> filePatterns)
        {
            var paths = new List<string>();
            foreach (var filePattern in filePatterns)
            {
                //Directory.GetFiles(path, pattern);
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filePattern);
                paths.Add(absolutePath);
            }
            return paths;
        }

        private static string FullFilePath(string arg)
        {
            throw new NotImplementedException();
        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }

    }
}
