using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class RepairCommand(IFile fileStore, IConsoleWriter consoleWriter) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "repair";
    public string Description => "Check each path locked in the database if the file still exists and is not corrupted / changed";
    public string Details => "For now it only reports the missing files, it does not change the database.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        foreach (var path in fileStore.GetPaths())
        {
            if (!File.Exists(path))
            {
                consoleWriter.WriteLine($"File '{path}' is missing.");
            }
        }

        return Task.FromResult(true);
    }
}
