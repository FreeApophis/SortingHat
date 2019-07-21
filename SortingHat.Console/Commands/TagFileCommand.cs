using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Funcky.Monads;
using SortingHat.ConsoleWriter;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class TagFileCommand : ICommand
    {
        private readonly ILogger<TagFileCommand> _logger;
        private readonly IConsoleWriter _consoleWriter;
        private readonly ITagParser _tagParser;
        private readonly IFilePathExtractor _filePathExtractor;
        private readonly Func<File> _newFile;

        public TagFileCommand(ILogger<TagFileCommand> logger, IConsoleWriter consoleWriter, ITagParser tagParser, IFilePathExtractor filePathExtractor, Func<File> newFile)
        {
            _logger = logger;
            _consoleWriter = consoleWriter;
            _tagParser = tagParser;
            _filePathExtractor = filePathExtractor;
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
            var files = arguments.Where(IsFile);

            var tasks = new List<Task>();
            foreach (var file in _filePathExtractor.FromFilePatterns(files).Select(FileFromPath))
            {
                foreach (var tag in tags.Select(_tagParser.Parse))
                {
                    _logger.LogInformation($"File {file.Path} tagged with {tag.Name}");

                    _consoleWriter.WriteLine($"File {file.Path} queued with tag {tag.Name}");
                    await file.Tag(tag);
                    //tasks.Add(file.Tag(tag));
                }
            }
            //await Task.WhenAll(tasks);
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            ExecuteAsync(arguments).Wait();

            return true;
        }

        public string LongCommand => "tag-files";
        public Option<string> ShortCommand => Option.Some("tag");
        public string ShortHelp => "Is tagging the files the given tags";
        public CommandGrouping CommandGrouping => CommandGrouping.File;
    }
}
