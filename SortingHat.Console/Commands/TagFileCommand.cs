using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SortingHat.CLI.Commands
{
    class TagFileCommand : ICommand
    {
        private const string Command = "tag-file";
        private readonly IServices _services;

        public TagFileCommand(IServices services)
        {
            _services = services;
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

            foreach (var file in files.Select(file => new API.Models.File(file, _services)))
            {
                foreach (var tag in tags.Select(tag => API.Models.Tag.Parse(tag)))
                {
                    file.Tag(_services, tag);
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
