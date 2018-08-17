using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CLI.Commands
{
    class TagFileCommand : ICommand
    {
        private readonly IDatabase _db;
        private readonly ILogger<TagFileCommand> _logger;
        private readonly IHashService _hashService;

        public TagFileCommand(IDatabase db, ILogger<TagFileCommand> logger, IHashService hashService)
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
                foreach (var tag in tags.Select(API.Models.Tag.Parse))
                {
                    _logger.LogInformation($"File {file.Path} tagged with {tag.Name}");
                    file.Tag(_db, tag);
                }
            }
            return true;
        }

        public string LongCommand => "tag-files";
        public string ShortCommand => null;

        public string ShortHelp => "";
    }
}
