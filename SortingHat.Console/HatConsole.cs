using Spectre.Console;

namespace SortingHat.CLI;

/// <summary>The terminal hat renders to with Spectre.Console: colours, tables and prompts only where the terminal has them.</summary>
internal static class HatConsole
{
    /// <summary>
    /// The width when the output goes to a file or another program: Spectre would wrap at 80 columns,
    /// but a script reading the output of hat needs every path on one line.
    /// </summary>
    public const int RedirectedWidth = 4096;

    public static IAnsiConsole Create()
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings());

        if (System.Console.IsOutputRedirected)
        {
            console.Profile.Width = RedirectedWidth;
        }

        return console;
    }
}
