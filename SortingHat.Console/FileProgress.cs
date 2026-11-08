using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Spectre.Console;

namespace SortingHat.CLI;

/// <summary>
/// Works through files one after the other, with a progress bar that names the current file, in a terminal that can redraw it.
/// When the output goes to a file or another program, there is no bar: the output stays the plain lines of the command.
/// </summary>
[UsedImplicitly]
internal sealed class FileProgress(IAnsiConsole console)
{
    /// <summary>Runs <paramref name="work"/> for each of the <paramref name="paths"/>; the bar names it with the <paramref name="verb"/>, such as 'Tagging 3/120: a.pdf'.</summary>
    public Task ForEach(string verb, IReadOnlyList<string> paths, Func<string, Task> work)
        => console.Profile.Capabilities is { Interactive: true, Ansi: true }
            ? console.Progress()
                .AutoClear(true)
                .Columns(new TaskDescriptionColumn { Alignment = Justify.Left }, new ProgressBarColumn(), new PercentageColumn())
                .StartAsync(context => WithBar(context.AddTask(verb, maxValue: paths.Count), verb, paths, work))
            : WithoutBar(paths, work);

    private static async Task WithBar(ProgressTask bar, string verb, IReadOnlyList<string> paths, Func<string, Task> work)
    {
        foreach (var (index, path) in paths.Index())
        {
            bar.Description = Markup.Escape($"{verb} {index + 1}/{paths.Count}: {Path.GetFileName(path)}");
            await work(path);
            bar.Increment(1);
        }
    }

    private static async Task WithoutBar(IEnumerable<string> paths, Func<string, Task> work)
    {
        foreach (var path in paths)
        {
            await work(path);
        }
    }
}
