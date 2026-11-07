using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using Cli = System.CommandLine;
using Console = Apophis.CLI.Console;
using Flag = System.CommandLine.Option<bool>;

namespace SortingHat.CLI;

/// <summary>
/// Maps every command, built-in and from plugins, onto a System.CommandLine command tree.
/// Every <see cref="IDeclaredCommand"/> gets exactly the arguments and options it declares.
/// </summary>
[UsedImplicitly]
internal sealed class CommandLine(Console console, IEnumerable<IDeclaredCommand> commands)
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

    private static string HelpDescription(IDeclaredCommand command)
        => command.Details is ""
            ? command.Description
            : $"{command.Description}{Environment.NewLine}{Environment.NewLine}{command.Details}";

    private static Flag ToFlag(OptionDefinition option)
        => new($"--{option.Name}", [.. option.ShortName.Select(shortName => $"-{shortName}").ToEnumerable()]) { Description = option.Description };

    /// <summary>System.CommandLine parses a single value only into a <c>string</c>, more values into a <c>string[]</c>.</summary>
    private static ParsedArgument ToArgument(ArgumentDefinition argument)
        => argument.Arity switch
        {
            Arity.ExactlyOne or Arity.ZeroOrOne => SingleValue(argument),
            _ => ManyValues(argument),
        };

    private static ParsedArgument SingleValue(ArgumentDefinition definition)
    {
        var argument = new Cli.Argument<string>(definition.Name) { Description = definition.Description, Arity = ToArity(definition.Arity) };

        return new ParsedArgument(definition, argument, parseResult => [.. Option.FromNullable(parseResult.GetValue(argument)).ToEnumerable()]);
    }

    private static ParsedArgument ManyValues(ArgumentDefinition definition)
    {
        var argument = new Cli.Argument<string[]>(definition.Name) { Description = definition.Description, Arity = ToArity(definition.Arity) };

        return new ParsedArgument(definition, argument, parseResult => Values(parseResult, argument));
    }

    private static Cli.ArgumentArity ToArity(Arity arity)
        => arity switch
        {
            Arity.ExactlyOne => Cli.ArgumentArity.ExactlyOne,
            Arity.ZeroOrOne => Cli.ArgumentArity.ZeroOrOne,
            Arity.ZeroOrMore => Cli.ArgumentArity.ZeroOrMore,
            Arity.OneOrMore => Cli.ArgumentArity.OneOrMore,
            _ => throw new ArgumentOutOfRangeException(nameof(arity), arity, "Unknown arity."),
        };

    private static CommandInput Input(Cli.ParseResult parseResult, IEnumerable<ParsedArgument> arguments, IEnumerable<(OptionDefinition Definition, Flag Flag)> flags)
        => new(
            arguments.ToDictionary(argument => argument.Definition, argument => argument.Values(parseResult)),
            flags.Where(flag => parseResult.GetValue(flag.Flag)).Select(flag => flag.Definition).ToHashSet());

    private static IReadOnlyList<string> Values(Cli.ParseResult parseResult, Cli.Argument<string[]> argument)
        => Option.FromNullable(parseResult.GetValue(argument)).GetOrElse([]);

    private static async Task<int> ExecuteAsync(IDeclaredCommand command, CommandInput input, CancellationToken cancellationToken)
        => await command.ExecuteAsync(input, cancellationToken)
            ? ExitCode.Success
            : ExitCode.Failure;

    private Cli.RootCommand RootCommand()
    {
        var root = new Cli.RootCommand("SortingHat: find your documents with tags");

        commands.ForEach(command => root.Add(Command(command)));
        root.SetAction(_ => Hint());

        return root;
    }

    private Cli.Command Command(IDeclaredCommand command)
    {
        var result = new Cli.Command(command.Name, HelpDescription(command));
        var arguments = command.Arguments.Select(ToArgument).ToList();
        var flags = command.Options.Select(option => (Definition: option, Flag: ToFlag(option))).ToList();

        command.Aliases.ForEach(result.Aliases.Add);
        arguments.ForEach(argument => result.Add(argument.Argument));
        flags.ForEach(flag => result.Add(flag.Flag));
        result.SetAction((parseResult, cancellationToken) => UnknownOptions(parseResult.Tokens) switch
        {
            [] => ExecuteAsync(command, Input(parseResult, arguments, flags), cancellationToken),
            var unknownOptions => Task.FromResult(ReportUnknown(unknownOptions)),
        });

        return result;
    }

    /// <summary>
    /// An argument that takes many values would take an unknown option such as <c>--dryrun</c> as a value. Before the separator <c>--</c>,
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
