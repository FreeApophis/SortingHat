using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.API.Tagging;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class AutoTagCommand : ICommand
    {
        private readonly ILogger<TagFileCommand> _logger;
        private readonly ITagParser _tagParser;
        private readonly IEnumerable<IAutoTag> _autoTags;
        private readonly Func<File> _newFile;

        public AutoTagCommand(ILogger<TagFileCommand> logger, ITagParser tagParser, IEnumerable<IAutoTag> autoTags, Func<File> newFile)
        {
            _logger = logger;
            _tagParser = tagParser;
            _autoTags = autoTags;
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

        private string RemoveFirstAndLastCharacter(string bracedString)
        {
            return bracedString.Substring(1, bracedString.Length - 2);
        }

        private string ReplaceVariable(string name, string filePath)
        {
            var variable = RemoveFirstAndLastCharacter(name);

            foreach (var autoTag in _autoTags)
            {
                if (autoTag.PossibleAutoTags.Any(t => t == variable))
                {
                    return autoTag.HandleTag(variable, filePath);
                }
            }

            throw new Exception($"Unknown Variable: '{variable}'");
        }

        private string ReplaceVariables(string tagPattern, string filePath)
        {
            var variableRegex = new Regex("{.*?}");

            MatchCollection matches = variableRegex.Matches(tagPattern);

            foreach (Match match in matches)
            {
                tagPattern = tagPattern.Replace(match.Value, ReplaceVariable(match.Value, filePath));
            }

            Console.WriteLine(tagPattern);
            return tagPattern;
        }

        private Tag TagFromPattern(string tagPattern, string filePath)
        {
            return _tagParser.Parse(ReplaceVariables(tagPattern, filePath));
        }

        private bool ListTagVariables()
        {
            Console.WriteLine("Possible Tag Variables:");
            Console.WriteLine();
            foreach (var autoTag in _autoTags)
            {
                foreach (var tags in autoTag.PossibleAutoTags)
                {
                    Console.WriteLine($"* {tags}");
                }
            }

            return true;
        }

        private bool TagFiles(IEnumerable<string> arguments)
        {
            var tags = arguments.Where(a => a.IsTag());
            var files = new FilePathExtractor(arguments.Where(IsFile));

            foreach (var file in files.FilePaths.Select(FileFromPath))
            {
                foreach (var tag in tags.Select(t => TagFromPattern(t, file.Path)))
                {
                    file.Tag(tag).Wait();
                }
            }

            return true;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Any() == false)
            {
                return ListTagVariables();
            }
            else
            {
                return TagFiles(arguments);
            }
        }

        public string LongCommand => "auto-tag";
        public string ShortCommand => "auto";
        public string ShortHelp => "Automatically tags stuff ...";
        public CommandGrouping CommandGrouping => CommandGrouping.AutoTagging;
    }
}
