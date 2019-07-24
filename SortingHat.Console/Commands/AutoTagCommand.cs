using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using SortingHat.CliAbstractions;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class AutoTagCommand : ICommand
    {
        private readonly ILogger<TagFileCommand> _logger;
        private readonly IConsoleWriter _consoleWriter;
        private readonly IFilePathExtractor _filePathExtractor;
        private readonly IAutoTagHandler _autoTagHandler;
        private readonly Func<File> _newFile;
        private IOptions _options;

        public AutoTagCommand(ILogger<TagFileCommand> logger, IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, IAutoTagHandler autoTagHandler, Func<File> newFile)
        {
            _logger = logger;
            _consoleWriter = consoleWriter;
            _filePathExtractor = filePathExtractor;
            _autoTagHandler = autoTagHandler;
            _newFile = newFile;
        }

        private File FileFromPath(string filePath)
        {
            var file = _newFile();

            file.Path = filePath;
            file.LoadByPath();

            return file;
        }
        private bool ListTagVariables()
        {
            _consoleWriter.WriteLine("Possible Tag Variables:");
            _consoleWriter.WriteLine();
            foreach (var tag in _autoTagHandler.AutoTags.OrderBy(tag => tag.AutoTagKey))
            {
                _consoleWriter.WriteLine($"* {tag.HumanReadableAutoTagsKey}");
                if (_options.HasOption("v", "verbose"))
                {
                    _consoleWriter.WriteLine($"=>  {tag.Description}");
                    _consoleWriter.WriteLine();
                }
            }

            return true;
        }

        private bool TagFiles(IEnumerable<string> arguments)
        {
            var tags = arguments.Where(a => a.IsTag());
            var files = arguments.Where(a => a.IsFile());

            foreach (var file in FilesFromPattern(files))
            {
                foreach (var tag in ReplacedTags(tags, file))
                {
                    _consoleWriter.WriteLine($"Tag '{file.Path}' with '{tag.FullName}'");

                    if (_options.HasOption(null, "dry-run"))
                    {
                        continue;
                    }

                    file.Tag(tag).Wait();
                }
            }

            return true;
        }

        private IEnumerable<File> FilesFromPattern(IEnumerable<string> files)
        {
            return _filePathExtractor.FromFilePatterns(files).Select(FileFromPath);
        }

        private IEnumerable<Tag> ReplacedTags(IEnumerable<string> tags, File file)
        {
            return tags.Select(t => TagFromMask(t, file)).Where(t => t != null);
        }

        private Tag TagFromMask(string t, File file)
        {
            return _autoTagHandler.TagFromMask(t, new System.IO.FileInfo(file.Path));
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            _options = options;

            return arguments.Any()
                ? TagFiles(arguments)
                : ListTagVariables();
        }

        public string LongCommand => "auto-tag";
        public Option<string> ShortCommand => Option.Some("auto");
        public string ShortHelp => "Automatically tags stuff ...";
        public CommandGrouping CommandGrouping => CommandGrouping.AutoTagging;
    }
}
