using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using JetBrains.Annotations;
using Serilog;
using SortingHat.API.DI;
using SortingHat.API.Events;
using SortingHat.API.Models;
using File = System.IO.File;

namespace SortingHat.Plugin.MoveOnTag;

/// <summary>Moves a file that got a tag with a move rule into the folder of the rule, and tells hat its new path.</summary>
[UsedImplicitly]
internal sealed class MoveOnTagHandler(MoveRuleStore store, IFile fileStore, IMoveFile moveFile, IConsoleWriter consoleWriter, ILogger logger) : IFileEventHandler<FileTagged>
{
    public Task Handle(FileTagged fileEvent, CancellationToken cancellationToken)
    {
        store.Load().For(fileEvent.Tags).AndThen(rule => rule.Switch(
            left: reason => Stay(fileEvent.File, reason),
            right: matching => Move(fileEvent.File, matching)));

        return Task.CompletedTask;
    }

    private static string Target(FileAt file, MoveRule rule)
        => Path.Combine(rule.Folder, Path.GetFileName(file.Path));

    /// <summary>A file already in the folder stays; a file with its name in the folder is never overwritten.</summary>
    private void Move(FileAt file, MoveRule rule)
    {
        var target = Target(file, rule);

        if (Path.GetFullPath(file.Path) == target)
        {
            return;
        }

        if (File.Exists(target))
        {
            Stay(file, $"'{target}' already exists");

            return;
        }

        consoleWriter.WriteLine($"mv {file.Path} {target}");
        moveFile.Move(file.Path, target);
        fileStore.MovePath(file, target);
        logger.Information("Moved {Source} to {Target} by the rule {Rule}", file.Path, target, rule.ToString());
    }

    private void Stay(FileAt file, string reason)
        => consoleWriter.WriteLine($"'{file.Path}' not moved: {reason}.");
}
