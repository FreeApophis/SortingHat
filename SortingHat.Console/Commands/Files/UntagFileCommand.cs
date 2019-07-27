using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files
{
    [UsedImplicitly]
    internal class UntagFileCommand : ICommand
    {
        private readonly ILogger<TagFileCommand> _logger;
        private readonly IFilePathExtractor _filePathExtractor;
        private readonly ITagParser _tagParser;
        private readonly Func<File> _newFile;

        public UntagFileCommand(ILogger<TagFileCommand> logger, IFilePathExtractor filePathExtractor, IHashService hashService, ITagParser tagParser, Func<File> newFile)
        {
            _logger = logger;
            _filePathExtractor = filePathExtractor;
            _tagParser = tagParser;
            _newFile = newFile;
        }

        private static bool IsFile(string value)
        {
            return value.StartsWith(":") == false;
        }

        private File FileFromPath(string filePath)
        {
            var file = _newFile();

            file.Path = filePath;
            file.LoadByPath();

            return file;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var tags = arguments.Where(a => a.IsTag());
            var files = arguments.Where(IsFile);

            foreach (var file in _filePathExtractor.FromFilePatterns(files).Select(FileFromPath))
            {
                foreach (var tag in tags.Select(_tagParser.Parse))
                {
                    if (tag != null)
                    {
                        _logger.LogInformation($"File {file.Path} tagged with {tag.Name}");
                        file.Untag(tag);
                    }
                }
            }

            return true;

        }

        public string LongCommand => "untag-files";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Remove tags from the indicated files";
        public CommandGrouping CommandGrouping => CommandGrouping.File;
    }
}
