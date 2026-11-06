using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using SortingHat.API.DI;

namespace SortingHat.Test;

/// <summary>
/// A command like a plugin would contribute it, to test the <see cref="IDeclaredCommand"/> extension point through the real application.
/// </summary>
internal sealed class GreetCommand(IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public static readonly ArgumentDefinition Who = new() { Name = "who", Description = "The one to greet." };

    public static readonly OptionDefinition Loud = new() { Name = "loud", ShortName = "l", Description = "Greets in capitals." };

    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public string Name => "greet";

    public IReadOnlyList<string> Aliases => ["hi"];

    public string Description => "Greets someone.";

    public IReadOnlyList<ArgumentDefinition> Arguments => [Who];

    public IReadOnlyList<OptionDefinition> Options => [Loud];

    /// <summary>Greeting 'nobody' fails, so tests can see a failing command.</summary>
    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(input.Value(Who).Match(none: false, some: who => Greet(who, input.Has(Loud))));

    private bool Greet(string who, bool loud)
    {
        consoleWriter.WriteLine(loud ? $"HELLO, {who.ToUpperInvariant()}!" : $"Hello, {who}!");

        return who != "nobody";
    }
}
