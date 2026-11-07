using System;
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
internal class TagFileCommand(ILogger<TagFileCommand> logger, IConsoleWriter consoleWriter, IFilePathExtractor filePathExtractor, Func<File> newFile, IFile fileStore, IAutoTagHandler autoTagHandler) : IDeclaredCommand
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

        foreach (var file in filePathExtractor.FromFilePatterns(files, input.Has(FileOptions.Recursive)).Select(FileFromPath))
        {
            if (await file.HasChanged())
            {
                consoleWriter.WriteLine($"'{file.Path}' has changed, its tags were carried over to the new content.");
            }

            if (tags.Count == 0)
            {
                fileStore.Store(file);
            }
            else
            {
                await TagFile(tags, file);
            }
        }

        return true;
    }

    private File FileFromPath(string filePath)
    {
        var file = newFile();

        file.LoadByPathFromDbWithFallback(filePath);

        return file;
    }

    private async Task TagFile(List<string> tags, File file)
    {
        foreach (var tag in ReplacedTags(tags, file))
        {
            logger.LogInformation($"File {file.Path} tagged with {tag.FullName}");

            consoleWriter.WriteLine($"File {file.Path} queued with tag {tag.FullName}");
            await file.Tag(tag);
        }
    }

    private IEnumerable<Tag> ReplacedTags(IEnumerable<string> tags, File file)
        => tags.WhereSelect(tag => Option.FromNullable(autoTagHandler.TagFromMask(tag, new System.IO.FileInfo(file.Path))));
}
