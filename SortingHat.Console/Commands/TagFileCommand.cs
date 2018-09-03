using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SortingHat.API.Models;
using System.Threading.Tasks;
using SortingHat.API;
using SortingHat.API.DI;

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

        public async Task ExecuteAsync(IEnumerable<string> arguments)
        {
            var tags = arguments.Where(a => a.IsTag());
            var files = new FilePathExtractor(arguments.Where(IsFile));

            var tasks = new List<Task>();
            foreach (var file in files.FilePaths.Select(FileFromPath))
            {
                foreach (var tag in tags.Select(_tagParser.Parse))
                {
                    _logger.LogInformation($"File {file.Path} tagged with {tag.Name}");

                    Console.WriteLine($"File {file.Path} queued with tag {tag.Name}");
                    await file.Tag(tag);
                    //tasks.Add(file.Tag(tag));
                }
            }
            //await Task.WhenAll(tasks);
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            ExecuteAsync(arguments).Wait();

            return true;
        }

        public string LongCommand => "tag-files";
        public string ShortCommand => "tag";

        public string ShortHelp => "Is tagging the files the given tags";
    }
}
