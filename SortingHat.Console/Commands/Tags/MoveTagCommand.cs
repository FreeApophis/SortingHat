﻿using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags
{
    [UsedImplicitly]
    internal class MoveTagCommand : ICommand
    {
        private readonly ILogger<RenameTagCommand> _logger;
        private readonly ITagParser _tagParser;

        public MoveTagCommand(ILogger<RenameTagCommand> logger, ITagParser tagParser)
        {
            _logger = logger;
            _tagParser = tagParser;
        }
        public string LongCommand => "move-tags";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "this moves a tag to another parent tag, if you want to move it to the root, use the empty tag ':'";
        public CommandGrouping CommandGrouping => CommandGrouping.Tag;

        public bool Execute(IEnumerable<string> lazyArguments, IOptionParser options)
        {
            var arguments = lazyArguments.ToList();
            var result = false;

            if (arguments.Count >= 2)
            {
                result = MoveTags(arguments);
            }

            if (result == false)
            {
                _logger.LogWarning("Rename failed");
            }

            return result;
        }

        private bool MoveTags(IEnumerable<string> lazyArguments)
        {
            var arguments = lazyArguments.ToList();
            if (CheckArguments(arguments))
            {
                if (_tagParser.Parse(arguments.Last()) is { } destinationTag)
                {
                    return arguments.SkipLast(1).Select(_tagParser.Parse).All(t => t != null && t.Move(destinationTag));
                }
            }

            return false;
        }

        private bool CheckArguments(IEnumerable<string> arguments)
        {
            return arguments.All(a => a.IsTag());
        }
    }
}
