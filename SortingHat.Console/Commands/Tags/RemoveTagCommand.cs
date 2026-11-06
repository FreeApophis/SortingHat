using System.Collections.Generic;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class RemoveTagCommand(ITagParser tagParser, ILogger<RemoveTagCommand> logger) : ICommand
{
    public string LongCommand => "remove-tags";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Removes a tag from database";
    public CommandGrouping CommandGrouping => CommandGrouping.Tag;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        foreach (var tagString in arguments)
        {
            var tag = tagParser.Parse(tagString);

            if (tag is null)
            {
                logger.LogWarning($"Remove tag '{tagString}' failed (parse)");
            }
            else if (!tag.Destroy())
            {
                logger.LogWarning($"Remove tag '{tagString}' failed (db)");
            }
        }

        return true;
    }
}
