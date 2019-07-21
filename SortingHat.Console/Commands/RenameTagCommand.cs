using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class RenameTagCommand : ICommand
    {
        private readonly ILogger<RenameTagCommand> _logger;
        private readonly ITagParser _tagParser;

        public RenameTagCommand(ILogger<RenameTagCommand> logger, ITagParser tagParser)
        {
            _logger = logger;
            _tagParser = tagParser;
        }

        public bool Execute(IEnumerable<string> arguments, IOptions options)
        {
            var result = false;

            if (arguments.Count() == 2)
            {
                result = RenameTag(arguments);
            }
            else
            {
                _logger.LogWarning("rename tag has exactly two arguments, the tag to rename, and a new name: hat rename-tag :tag new_name");
                System.Console.WriteLine("rename tag has exactly two arguments, the tag to rename, and a new name: hat rename-tag :tag new_name");
            }

            if (result == false)
            {
                _logger.LogWarning("Rename failed");
            }

            return result;
        }

        private bool RenameTag(IEnumerable<string> arguments)
        {
            var tagString = arguments.First();
            var newName = arguments.Last();

            if (CheckArguments(tagString, newName))
            {
                var tag = _tagParser.Parse(tagString);
                return tag.Rename(newName);
            }

            return false;
        }

        private bool CheckArguments(string tagString, string newName)
        {
            if (tagString.IsTag() == false)
            {
                _logger.LogWarning($"First Argument '{tagString}' must be a tag");
                System.Console.WriteLine($"First Argument '{tagString}' must be a tag");

                return false;
            }

            if (newName.IsTag())
            {
                _logger.LogWarning($"Second Argument '{newName}' cannot be a tag");
                System.Console.WriteLine($"Second Argument '{newName}' cannot be a tag");

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
