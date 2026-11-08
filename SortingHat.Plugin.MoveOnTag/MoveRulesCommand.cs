using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.Plugin.MoveOnTag;

[UsedImplicitly]
internal sealed class MoveRulesCommand(MoveRuleStore store, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "move-rules";
    public string Description => "Lists the move rules: which tag moves files into which folder.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var rules = store.Load().Rules;

        if (rules.IsEmpty)
        {
            consoleWriter.WriteLine("There are no move rules.");
        }

        rules.ForEach(rule => consoleWriter.WriteLine(rule.ToString()));

        return Task.FromResult(true);
    }
}
