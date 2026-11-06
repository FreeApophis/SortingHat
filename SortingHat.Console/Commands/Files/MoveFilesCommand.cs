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
    public string Description => "This moves all files which match the search query to a specified folder location";
    public string Details => FileOperationArguments.Details;
    public IReadOnlyList<ArgumentDefinition> Arguments => [FileOperationArguments.Search, FileOperationArguments.Destination];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => Task.FromResult(FileOperationArguments.Run(input, moveFileOperation.ExportFiles));
}
