using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API.AutoTag;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class TagFileCommand(ILogger<TagFileCommand> logger, IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, IFileScanner fileScanner, IFile fileStore, IAutoTagHandler autoTagHandler) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "tag-files";
    public IReadOnlyList<string> Aliases => ["tag"];
    public string Description => "Is tagging the files the given tags";

    public string Details
        => """
           Without tags, the files are only added to the database.
           A tag can contain variables, which are replaced per file, for example ':Files:{FileType.Category}'.
           'hat help' lists the variables of the loaded plugins.
           """;

    public IReadOnlyList<ArgumentDefinition> Arguments => [FileOptions.TagsAndFiles];
    public IReadOnlyList<OptionDefinition> Options => [FileOptions.Recursive];

    public async Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var arguments = input.Values(FileOptions.TagsAndFiles);
        var tags = arguments.Where(a => a.IsTag).ToList();
        var files = arguments.Where(a => a.IsFile);

        foreach (var path in filePathExtractor.FromFilePatterns(files, input.Has(FileOptions.Recursive)))
        {
            var state = await fileScanner.Examine(path);

            if (state is PathState.Changed)
            {
                consoleWriter.WriteLine($"'{path}' has changed, its tags were carried over to the new content.");
            }

            if (tags.Count == 0)
            {
                fileStore.Store(state.Current);
            }
            else
            {
                TagFile(tags, state.Current);
            }
        }

        return true;
    }

    private void TagFile(List<string> tags, FileAt file)
    {
        foreach (var tag in ReplacedTags(tags, file))
        {
            logger.LogInformation($"File {file.Path} tagged with {tag.FullName}");

            consoleWriter.WriteLine($"File {file.Path} queued with tag {tag.FullName}");
            fileStore.Tag(file, tag);
        }
    }

    private IEnumerable<Tag> ReplacedTags(IEnumerable<string> tags, FileAt file)
        => tags.WhereSelect(tag => Option.FromNullable(autoTagHandler.TagFromMask(tag, new System.IO.FileInfo(file.Path))));
}
