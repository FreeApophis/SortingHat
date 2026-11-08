using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.FileSystem;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class MoveFilesCommand(FileOperations<IMoveFile> moveFileOperation) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "move-files";
    public IReadOnlyList<string> Aliases => ["mv"];
    public string Description => "Moves the files that match a search query into a folder.";
    public string Details => FileOperationArguments.Details;
    public IReadOnlyList<ArgumentDefinition> Arguments => [FileOperationArguments.Search, FileOperationArguments.Destination];
    public IReadOnlyList<OptionDefinition> Options => [FileOperationArguments.DryRun];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => FileOperationArguments.Run(input, moveFileOperation.ExportFiles);
}
