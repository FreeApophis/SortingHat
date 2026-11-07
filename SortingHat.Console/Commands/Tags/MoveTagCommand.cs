using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Tags;

[UsedImplicitly]
internal class MoveTagCommand(ITag tagStore, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    /// <summary>Only the last argument can take many values, so the sources and the destination share one argument.</summary>
    private static readonly ArgumentDefinition Tags = new() { Name = "tags", Description = "The tags to move, followed by their new parent tag.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.Tag;
    public string Name => "move-tags";
    public string Description => "Moves tags with their children and files under another parent tag.";
    public string Details => "The last tag is the new parent: 'move-tags :a :b :destination' turns ':a' into ':destination:a' and ':b' into ':destination:b'.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Tags];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Values(Tags).ToArray() switch
        {
            [.. var sources, var destination] when sources.Length > 0 => MoveTags(sources, destination),
            _ => Report("Give the destination tag after the tags to move, for example 'move-tags :a :destination'."),
        });

    private bool MoveTags(IEnumerable<string> sources, string destinationText)
        => Tag.Parse(destinationText).Match(
            left: Report,
            right: destination => sources.Aggregate(true, (moved, source) => MoveTag(source, destination) && moved));

    private bool MoveTag(string text, Tag destination)
        => Tag.Parse(text).Match(
            left: Report,
            right: tag => tagStore.Move(tag, destination) || Report($"The tag '{text}' could not be moved: it or '{destination.FullName}' does not exist, or '{destination.FullName}' already has a child named '{tag.Name}'."));

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
