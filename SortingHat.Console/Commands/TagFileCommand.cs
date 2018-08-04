using SortingHat.API.DI;
using SortingHat.API.Models;
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
            var parameters = arguments.Skip(1);

            var tags = parameters.Where(IsTag);
            var files = parameters.Where(IsFile);

            foreach (var file in files.Select(file => new API.Models.File(_services, file)))
            {
                foreach (var tag in tags.Select(tag => Tag.Parse(tag)))
                {
                    file.Tag(_services, tag);
                }
            }

            return true;

        }

        public bool Match(IEnumerable<string> arguments)
        {
            return arguments.Any() && arguments.First() == Command;
        }

    }
}
