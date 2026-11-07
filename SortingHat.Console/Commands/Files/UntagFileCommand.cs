using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class UntagFileCommand(IFilePathExtractor filePathExtractor, IFileScanner fileScanner, IFile fileStore) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "untag-files";
    public string Description => "Remove tags from the indicated files";
    public IReadOnlyList<ArgumentDefinition> Arguments => [FileOptions.TagsAndFiles];
    public IReadOnlyList<OptionDefinition> Options => [FileOptions.Recursive];

    public async Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var arguments = input.Values(FileOptions.TagsAndFiles);
        var tags = arguments.Where(a => a.IsTag).WhereSelect(tag => Tag.Parse(tag).RightOrNone()).ToList();
        var files = arguments.Where(a => a.IsFile);

        foreach (var path in filePathExtractor.FromFilePatterns(files, input.Has(FileOptions.Recursive)))
        {
            var file = (await fileScanner.Examine(path)).Current.File;

            tags.ForEach(tag => fileStore.Untag(file, tag));
        }

        return true;
    }
}
