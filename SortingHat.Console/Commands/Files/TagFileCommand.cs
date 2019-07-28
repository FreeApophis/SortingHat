using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files
{
    [UsedImplicitly]
    internal class TagFileCommand : ICommand
    {
        private readonly ILogger<TagFileCommand> _logger;
        private readonly IConsoleWriter _consoleWriter;
        private readonly ITagParser _tagParser;
        private readonly IFilePathExtractor _filePathExtractor;
        private readonly Func<File> _newFile;
        private readonly IFile _file;

        public TagFileCommand(ILogger<TagFileCommand> logger, IConsoleWriter consoleWriter, ITagParser tagParser, IFilePathExtractor filePathExtractor, Func<File> newFile, IFile file)
        {
            _logger = logger;
            _consoleWriter = consoleWriter;
            _tagParser = tagParser;
            _filePathExtractor = filePathExtractor;
            _newFile = newFile;
            _file = file;
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

        public async Task ExecuteAsync(IEnumerable<string> lazyArguments)
        {
            var arguments = lazyArguments.ToList();
            var tags = arguments.Where(a => a.IsTag()).ToList();
            var files = arguments.Where(IsFile);

            foreach (var file in _filePathExtractor.FromFilePatterns(files).Select(FileFromPath))
            {
                if (tags.Count == 0)
                {
                    _file.Store(file);
                }
                else
                {
                    await TagFile(tags, file);
                }
                
            }
        }

        private async Task TagFile(List<string> tags, File file)
        {
            foreach (var tag in tags.Select(_tagParser.Parse))
            {
                if (tag is { })
                {
                    _logger.LogInformation($"File {file.Path} tagged with {tag.Name}");

                    _consoleWriter.WriteLine($"File {file.Path} queued with tag {tag.Name}");
                    await file.Tag(tag);
                }
            }
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
