using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Autofac;
using SortingHat.API.DI;

namespace SortingHat.Test;

/// <summary>A command like a plugin would contribute it, with a name and aliases of the test's choice; it says that it ran.</summary>
internal sealed class NamedCommand(string name, IReadOnlyList<string> aliases, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public string Name => name;

    public IReadOnlyList<string> Aliases => aliases;

    public string Description => $"The test command {name}.";

    /// <summary>Registers the command for <c>new HatSandbox(...)</c>, after the built-in commands.</summary>
    public static void Register(ContainerBuilder builder, string name, params IReadOnlyList<string> aliases)
        => builder.Register(context => new NamedCommand(name, aliases, context.Resolve<IConsoleWriter>())).As<IDeclaredCommand>();

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        consoleWriter.WriteLine($"{name} ran");

        return Task.FromResult(true);
    }
}
