using Autofac;
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
        private readonly IContainer _container;

        public TagFileCommand(IContainer container)
        {
            _container = container;
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

            using (var scope = _container.BeginLifetimeScope())
            {
                foreach (var file in files.Select(file => new API.Models.File(file, scope.Resolve<HashService>())))
                {
                    foreach (var tag in tags.Select(tag => API.Models.Tag.Parse(tag)))
                    {
                        file.Tag(_container, tag);
                    }
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
