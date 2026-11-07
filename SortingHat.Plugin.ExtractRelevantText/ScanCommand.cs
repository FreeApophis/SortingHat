using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.Plugin.ExtractRelevantText;

[UsedImplicitly]
internal class ScanCommand(FolderScanner folderScanner) : IDeclaredCommand
{
    private static readonly ArgumentDefinition Folders = new() { Name = "folder", Description = "Folders to scan with their subfolders, every '*.txt' in them is read.", Arity = Arity.OneOrMore };

    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "scan";
    public IReadOnlyList<string> Aliases => ["s"];
    public string Description => "Lists the most frequent words of the text files in folders.";
    public IReadOnlyList<ArgumentDefinition> Arguments => [Folders];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(folderScanner.Scan(input.Values(Folders)));
}
