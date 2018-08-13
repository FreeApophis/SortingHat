using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.CLI.Commands
{
    class UntagFileCommand : ICommand
    {
        private readonly IDatabase _db;
        private readonly ILogger<TagFileCommand> _logger;
        private readonly IHashService _hashService;

        public UntagFileCommand(IDatabase db, ILogger<TagFileCommand> logger, IHashService hashService)
        {
            _db = db;
            _logger = logger;
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
            var tags = arguments.Where(IsTag);
            var files = new FilePathExtractor(arguments.Where(IsFile));

            foreach (var file in files.FilePaths.Select(file => new API.Models.File(file, _hashService)))
            {
                foreach (var tag in tags.Select(tag => API.Models.Tag.Parse(tag)))
                {
                    _logger.LogInformation($"File {file.Path} tagged with {tag.Name}");
                    file.Untag(_db, tag);
                }
            }

            return true;

        }

        public string LongCommand => "untag-files";
        public string ShortCommand => null;

        public string ShortHelp => "Remove tags from the indicated files";
    }
}
