using System.Collections.Generic;
using System.Linq;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class RenameTagCommand(ILogger<RenameTagCommand> logger, IConsoleWriter consoleWriter, ITagParser tagParser) : ICommand
{
    public string LongCommand => "rename-tag";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Renames a tag from database";
    public CommandGrouping CommandGrouping => CommandGrouping.Tag;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        var result = false;

        if (arguments.Count() == 2)
        {
            result = RenameTag(arguments);
        }
        else
        {
            logger.LogWarning("rename tag has exactly two arguments, the tag to rename, and a new name: hat rename-tag :tag new_name");
            consoleWriter.WriteLine("rename tag has exactly two arguments, the tag to rename, and a new name: hat rename-tag :tag new_name");
        }

        if (!result)
        {
            logger.LogWarning("Rename failed");
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
            var tag = tagParser.Parse(tagString);
            if (tag != null)
            {
                return tag.Rename(newName);
            }
        }

        return false;
    }

    private bool CheckArguments(string tagString, string newName)
    {
        if (!tagString.IsTag)
        {
            logger.LogWarning($"First Argument '{tagString}' must be a tag");
            consoleWriter.WriteLine($"First Argument '{tagString}' must be a tag");

            return false;
        }

        if (newName.IsTag)
        {
            logger.LogWarning($"Second Argument '{newName}' cannot be a tag");
            consoleWriter.WriteLine($"Second Argument '{newName}' cannot be a tag");

            return false;
        }

        return true;
    }
}
