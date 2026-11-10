using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.App.Tags;
using static Funcky.Functional;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class RenameTagCommand(IConsoleWriter consoleWriter, TagEdits tagEdits) : IDeclaredCommand
{
    private static readonly ArgumentDefinition TagArgument = new() { Name = "tag", Description = "The tag to rename, for example ':tax:2018'." };

    private static readonly ArgumentDefinition NewName = new() { Name = "new-name", Description = "The new name of its last level, without colon, for example '2019'." };

    public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    public string Name => "rename-tag";
    public string Description => "Renames a tag.";
    public string Details => "Only the last level changes: 'rename-tag :tax:2018 2019' gives ':tax:2019'. To give a tag another parent, use move-tags.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [TagArgument, NewName];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult((from tag in input.Value(TagArgument)
                            from newName in input.Value(NewName)
                            select RenameTag(tag, newName))
            .GetOrElse(false));

    private bool RenameTag(string text, string newName)
        => newName.IsTag
            ? Report($"The new name '{newName}' cannot be a tag, give only the new name of the last level, for example '2019'.")
            : (from tag in Tag.Parse(text)
               from name in TagName.Parse(newName)
               from renamed in tagEdits.Rename(tag, name)
               select renamed).Match(left: Report, right: True);

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
