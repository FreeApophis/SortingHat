using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SortingHat.API.Models;
using System.Threading.Tasks;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class TagFileCommand : ICommand
    {
        private readonly ILogger<TagFileCommand> _logger;
        private readonly ITagParser _tagParser;
        private readonly Func<File> _newFile;

        public TagFileCommand(ILogger<TagFileCommand> logger, ITagParser tagParser, Func<File> newFile)
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

        private File FileFromPath(string filePath)
        {
            var file = _newFile();

            file.Path = filePath;
            file.LoadByPath();

            return file;
        }

        public Task<bool> ExecuteAsync(IEnumerable<string> arguments)
        {
            var tags = arguments.Where(IsTag);
            var files = new FilePathExtractor(arguments.Where(IsFile));

            foreach (var file in files.FilePaths.Select(FileFromPath))
            {
                foreach (var tag in tags.Select(_tagParser.Parse))
                {
                    _logger.LogInformation($"File {file.Path} tagged with {tag.Name}");

                    Console.WriteLine($"File {file.Path} queued with tag {tag.Name}");
                    file.Tag(tag);
                }
            }
            return Task.FromResult(true);
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            return ExecuteAsync(arguments).Result;
        }

        public string LongCommand => "tag-files";
        public string ShortCommand => "tag";

        public string ShortHelp => "Is tagging the files the given tags";
    }
}
