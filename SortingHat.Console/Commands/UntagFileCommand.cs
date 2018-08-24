using System;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class UntagFileCommand : ICommand
    {
        private readonly ILogger<TagFileCommand> _logger;
        private readonly ITagParser _tagParser;
        private readonly Func<string, bool, File> _newFile;

        public UntagFileCommand(ILogger<TagFileCommand> logger, IHashService hashService, ITagParser tagParser, Func<string, bool, File> newFile)
        {
            _logger = logger;
            _tagParser = tagParser;
            _newFile = newFile;
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

            foreach (var file in files.FilePaths.Select(file => _newFile(file, true)))
            {
                foreach (var tag in tags.Select(_tagParser.Parse))
                {
                    _logger.LogInformation($"File {file.Path} tagged with {tag.Name}");
                    file.Untag(tag);
                }
            }

            return true;

        }

        public string LongCommand => "untag-files";
        public string ShortCommand => null;

        public string ShortHelp => "Remove tags from the indicated files";
    }
}
