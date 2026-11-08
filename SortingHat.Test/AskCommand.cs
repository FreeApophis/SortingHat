using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using SortingHat.API.DI;

namespace SortingHat.Test;

/// <summary>A command like a plugin would contribute it, that asks through <see cref="IUserPrompt"/> and writes the answer.</summary>
internal sealed class AskCommand(IUserPrompt prompt, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public static readonly ArgumentDefinition Question = new() { Name = "question", Description = "'confirm' or 'choose'." };

    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "ask";
    public string Description => "Asks the user a question.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Question];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        consoleWriter.WriteLine(input.Value(Question).GetOrElse(string.Empty) switch
        {
            "confirm" => prompt.Confirm("Go on [really]?").Match(none: "answer: cannot ask", some: yes => yes ? "answer: yes" : "answer: no"),
            _ => prompt.Choose("Which [colour]?", ["red", "green", "blue [dark]"], colour => colour.ToUpperInvariant()).Match(none: "answer: cannot ask", some: colour => $"answer: {colour}"),
        });

        return Task.FromResult(true);
    }
}
