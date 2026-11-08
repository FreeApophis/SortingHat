using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.Plugin.MoveOnTag;

[UsedImplicitly]
internal sealed class MoveRuleCommand(MoveRuleStore store, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    private static readonly ArgumentDefinition TagArgument = new() { Name = "tag", Description = "The tag of the rule, for example ':tax'. It also matches its children, such as ':tax:2018'." };

    private static readonly ArgumentDefinition Folder = new() { Name = "folder", Description = "The existing folder the files move into.", Arity = Arity.ZeroOrOne };

    private static readonly OptionDefinition Remove = new() { Name = "remove", Description = "Removes the rule of the tag instead." };

    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "move-rule";
    public string Description => "Moves the files that get a tag into a folder, from now on.";

    public string Details
        => """
           When tag-files gives a file the tag or one of its children, the file moves into the folder and hat records its new path.
           When several rules match, the most specific tag wins. A file stays where it is when equally specific rules match
           or the folder already has a file with its name. A rule for a tag that has one already replaces it.
           """;

    public IReadOnlyList<ArgumentDefinition> Arguments => [TagArgument, Folder];
    public IReadOnlyList<OptionDefinition> Options => [Remove];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(TagArgument).Match(
            none: false,
            some: text => Tag.Parse(text)
                .SelectMany(tag => Change(tag, input.Value(Folder), input.Has(Remove), store.Load()))
                .Match(left: Report, right: Apply)));

    private static Either<string, MoveRule> Rule(Tag tag, Option<string> folder)
        => folder.Match(
            none: () => Either<string, MoveRule>.Left("Give the folder the files move into, or '--remove' to remove the rule."),
            some: path => Directory.Exists(path)
                ? new MoveRule(tag, Path.GetFullPath(path))
                : Either<string, MoveRule>.Left($"There is no folder '{path}'."));

    /// <summary>The rules after the change, and what to tell the user; nothing is saved yet.</summary>
    private static Either<string, (MoveRules Rules, string Message)> Change(Tag tag, Option<string> folder, bool remove, MoveRules rules)
        => remove
            ? rules.Without(tag).Select(rest => (rest, $"The move rule for '{tag.FullName}' is removed."))
            : Rule(tag, folder).Select(rule => (rules.With(rule), $"Files tagged {tag.FullName} move into '{rule.Folder}' from now on."));

    private bool Apply((MoveRules Rules, string Message) change)
    {
        store.Save(change.Rules);
        consoleWriter.WriteLine(change.Message);

        return true;
    }

    private bool Report(string problem)
    {
        consoleWriter.WriteLine(problem);

        return false;
    }
}
