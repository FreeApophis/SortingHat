using System.Collections.Generic;
using System.Linq;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using Cli = System.CommandLine;
using Console = Apophis.CLI.Console;
using Flag = System.CommandLine.Option<bool>;
using IOption = SortingHat.API.DI.IOption;

namespace SortingHat.CLI;

/// <summary>
/// Maps every <see cref="ICommand"/> and <see cref="IOption"/>, built-in and from plugins, onto a System.CommandLine command tree.
/// The commands keep their contract: they get their arguments as strings and the given options through <see cref="IOptionParser"/>.
/// </summary>
[UsedImplicitly]
internal sealed class CommandLine(Console console, IEnumerable<ICommand> commands, IEnumerable<IOption> options)
{
    /// <summary>Response files (<c>@file</c>) are off, so a file name starting with <c>@</c> stays an argument.</summary>
    private static readonly Cli.ParserConfiguration ParserConfiguration = new() { ResponseFileTokenReplacer = null };

    internal int Run(IReadOnlyList<string> arguments)
    {
        using var output = new ConsoleTextWriter(console.Writer);

        return RootCommand()
            .Parse(arguments, ParserConfiguration)
            .Invoke(new Cli.InvocationConfiguration { Output = output, Error = output, EnableDefaultExceptionHandler = false });
    }

    /// <summary>Every option is a flag that every command accepts, anywhere on the command line.</summary>
    private static Option<Flag> ToFlag(IOption option)
        => option.LongOption.Select(name => $"--{name}")
            .OrElse(option.ShortOption.Select(name => $"-{name}"))
            .Select(name => new Flag(name, [.. Aliases(option, name)]) { Description = option.ShortHelp, Recursive = true });

    private static IEnumerable<string> Aliases(IOption option, string name)
        => option.ShortOption
            .Select(shortOption => $"-{shortOption}")
            .Where(alias => alias != name)
            .ToEnumerable();

    private static IReadOnlyList<IOption> GivenOptions(Cli.ParseResult parseResult, IEnumerable<(IOption Option, Flag Flag)> flags)
        => [.. flags.Where(flag => parseResult.GetValue(flag.Flag)).Select(flag => flag.Option)];

    private static int Execute(ICommand command, IEnumerable<string> arguments, IReadOnlyList<IOption> givenOptions)
        => command.Execute(arguments, new OptionParser(givenOptions))
            ? ExitCode.Success
            : ExitCode.Failure;

    private Cli.RootCommand RootCommand()
    {
        var flags = options.WhereSelect(option => ToFlag(option).Select(flag => (Option: option, Flag: flag))).ToList();
        var root = new Cli.RootCommand("SortingHat: find your documents with tags");

        flags.ForEach(flag => root.Add(flag.Flag));
        commands.ForEach(command => root.Add(Command(command, flags)));
        root.SetAction(_ => Hint());

        return root;
    }

    private Cli.Command Command(ICommand command, IReadOnlyList<(IOption Option, Flag Flag)> flags)
    {
        var result = new Cli.Command(command.LongCommand, command.ShortHelp);
        var arguments = new Cli.Argument<string[]>("arguments") { Arity = Cli.ArgumentArity.ZeroOrMore };

        command.ShortCommand.AndThen(result.Aliases.Add);
        result.Add(arguments);
        result.SetAction(parseResult => UnknownOptions(parseResult.Tokens) switch
        {
            [] => Execute(command, Option.FromNullable(parseResult.GetValue(arguments)).GetOrElse([]), GivenOptions(parseResult, flags)),
            var unknownOptions => ReportUnknown(unknownOptions),
        });

        return result;
    }

    /// <summary>
    /// The catch-all argument would take an unknown option such as <c>--dryrun</c> as a value. Before the separator <c>--</c>,
    /// an argument that starts with <c>-</c> is a typo or an option this command does not know. A single <c>-</c> stays an argument.
    /// </summary>
    private static IReadOnlyList<string> UnknownOptions(IEnumerable<Cli.Parsing.Token> tokens)
        =>
        [
            .. tokens
                .TakeWhile(token => token.Type != Cli.Parsing.TokenType.DoubleDash)
                .Where(token => token.Type == Cli.Parsing.TokenType.Argument && token.Value.StartsWith('-') && token.Value != "-")
                .Select(token => token.Value),
        ];

    private int ReportUnknown(IEnumerable<string> unknownOptions)
    {
        unknownOptions.ForEach(option => console.Writer.WriteLine($"Unknown option '{option}', use '--' before an argument that starts with '-'."));

        return ExitCode.Failure;
    }

    private int Hint()
    {
        console.Writer.WriteLine($"Maybe run '{console.Application.Name} help'");

        return ExitCode.Failure;
    }
}
