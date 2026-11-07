using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apophis.FileSystem;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal sealed class CopyFilesCommand(FileOperations<ICopyFile> copyFileOperation) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "copy-files";
    public IReadOnlyList<string> Aliases => ["cp"];
    public string Description => "This command copies all files which match the search query to a specified folder location.";
    public string Details => FileOperationArguments.Details;
    public IReadOnlyList<ArgumentDefinition> Arguments => [FileOperationArguments.Search, FileOperationArguments.Destination];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
        => FileOperationArguments.Run(input, copyFileOperation.ExportFiles);
}
