using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.DI;

namespace SortingHat.CLI;

/// <summary>
/// Gives every command name and alias to the first command that claims it: the built-in commands first, then the others in the order they were registered.
/// A later command whose name is taken is left out, a taken alias is dropped from it, and every clash becomes a warning.
/// </summary>
internal sealed record CommandNames(ImmutableList<OfferedCommand> Offered, ImmutableList<string> Warnings)
{
    /// <summary>Who has a name already, such as "an alias of 'tag-files'".</summary>
    private ImmutableDictionary<string, string> Claims { get; init; } = [];

    public static CommandNames Assign(IEnumerable<IDeclaredCommand> commands)
        => commands.OrderBy(IsPlugin).Aggregate(new CommandNames([], []), (names, command) => names.Add(command));

    private static bool IsPlugin(IDeclaredCommand command)
        => command.GetType().Assembly != typeof(CommandNames).Assembly;

    private static string Source(IDeclaredCommand command)
        => Option.FromNullable(command.GetType().Assembly.GetName().Name).GetOrElse("unknown assembly");

    private CommandNames Add(IDeclaredCommand command)
        => Claims.GetValueOrNone(command.Name).Match(
            none: () => Offer(command),
            some: owner => this with { Warnings = Warnings.Add($"Warning: command '{command.Name}' ({Source(command)}) left out: '{command.Name}' is already {owner}.") });

    private CommandNames Offer(IDeclaredCommand command)
    {
        var aliases = command.Aliases.Distinct().Where(alias => alias != command.Name).ToList();
        var kept = aliases.Where(alias => !Claims.ContainsKey(alias)).ToList();

        return this with
        {
            Offered = Offered.Add(new OfferedCommand(command, kept)),
            Warnings = Warnings.AddRange(aliases.WhereSelect(alias => Claims.GetValueOrNone(alias)
                .Select(owner => $"Warning: alias '{alias}' of '{command.Name}' ({Source(command)}) dropped: '{alias}' is already {owner}."))),
            Claims = Claims
                .Add(command.Name, $"the command '{command.Name}'")
                .AddRange(kept.Select(alias => KeyValuePair.Create(alias, $"an alias of '{command.Name}'"))),
        };
    }
}
