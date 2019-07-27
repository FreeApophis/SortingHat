using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.CliAbstractions;

namespace SortingHat.CLI.Commands.Tags
{
    [UsedImplicitly]
    internal class RenameTagCommand : ICommand
    {
        private readonly ILogger<RenameTagCommand> _logger;
        private readonly IConsoleWriter _consoleWriter;
        private readonly ITagParser _tagParser;

        public RenameTagCommand(ILogger<RenameTagCommand> logger, IConsoleWriter consoleWriter, ITagParser tagParser)
        {
            _logger = logger;
            _consoleWriter = consoleWriter;
            _tagParser = tagParser;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var result = false;

            if (arguments.Count() == 2)
            {
                result = RenameTag(arguments);
            } else
            {
                _logger.LogWarning("rename tag has exactly two arguments, the tag to rename, and a new name: hat rename-tag :tag new_name");
                _consoleWriter.WriteLine("rename tag has exactly two arguments, the tag to rename, and a new name: hat rename-tag :tag new_name");
            }

            if (result == false)
            {
                _logger.LogWarning("Rename failed");
            }

            return result;
        }

        private bool RenameTag(IEnumerable<string> arguments)
        {
            var renamePair = arguments.ToList();

            var tagString = renamePair.First();
            var newName = renamePair.Last();

            if (CheckArguments(tagString, newName))
            {
                var tag = _tagParser.Parse(tagString);
                if (tag != null)
                {
                    return tag.Rename(newName);

                }
            }

            return false;
        }

        private bool CheckArguments(string tagString, string newName)
        {
            if (tagString.IsTag() == false)
            {
                _logger.LogWarning($"First Argument '{tagString}' must be a tag");
                _consoleWriter.WriteLine($"First Argument '{tagString}' must be a tag");

                return false;
            }

            if (newName.IsTag())
            {
                _logger.LogWarning($"Second Argument '{newName}' cannot be a tag");
                _consoleWriter.WriteLine($"Second Argument '{newName}' cannot be a tag");

                return false;
            }

            return true;
        }

        public string LongCommand => "rename-tag";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Renames a tag from database";
        public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    }
}
