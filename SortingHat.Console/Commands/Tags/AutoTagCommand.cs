using System;
using System.Collections.Generic;
using System.Linq;
using apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags
{
    [UsedImplicitly]
    internal class AutoTagCommand : ICommand
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly IFilePathExtractor _filePathExtractor;
        private readonly IAutoTagHandler _autoTagHandler;
        private readonly Func<File> _newFile;

        public AutoTagCommand(IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, IAutoTagHandler autoTagHandler, Func<File> newFile)
        {
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
        private bool ListTagVariables(IOptions options)
        {
            _consoleWriter.WriteLine("Possible Tag Variables:");
            _consoleWriter.WriteLine();
            foreach (var tag in _autoTagHandler.AutoTags.OrderBy(tag => tag.AutoTagKey))
            {
                _consoleWriter.WriteLine($"* {tag.HumanReadableAutoTagsKey}");
                if (options.HasOption("v", "verbose"))
                {
                    _consoleWriter.WriteLine($"=>  {tag.Description}");
                    _consoleWriter.WriteLine();
                }
            }

            return true;
        }

        private bool TagFiles(IEnumerable<string> lazyArguments, IOptions options)
        {
            var arguments = lazyArguments.ToList();
            var tags = arguments.Where(a => a.IsTag()).ToList();
            var files = arguments.Where(a => a.IsFile());

            foreach (var file in FilesFromPattern(files))
            {
                foreach (var tag in ReplacedTags(tags, file))
                {
                    _consoleWriter.WriteLine($"Tag '{file.Path}' with '{tag.FullName}'");

                    if (options.HasOption(null, "dry-run"))
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
            foreach (var tag in tags.Select(t => TagFromMask(t, file)))
            {
                if (tag is { })
                {
                    yield return tag;
                }
            }
        }

        private Tag? TagFromMask(string tagMask, File file)
        {
            return _autoTagHandler.TagFromMask(tagMask, new System.IO.FileInfo(file.Path));
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            return arguments.Any()
                ? TagFiles(arguments, options)
                : ListTagVariables(options);
        }

        public string LongCommand => "auto-tag";
        public Option<string> ShortCommand => Option.Some("auto");
        public string ShortHelp => "Automatically tags stuff ...";
        public CommandGrouping CommandGrouping => CommandGrouping.AutoTagging;
    }
}
