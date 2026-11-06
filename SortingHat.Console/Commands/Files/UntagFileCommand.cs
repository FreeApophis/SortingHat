using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class UntagFileCommand(IFilePathExtractor filePathExtractor, ITagParser tagParser, Func<File> newFile) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.File;
    public string Name => "untag-files";
    public string Description => "Remove tags from the indicated files";
    public IReadOnlyList<ArgumentDefinition> Arguments => [FileOptions.TagsAndFiles];
    public IReadOnlyList<OptionDefinition> Options => [FileOptions.Recursive];

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
    {
        var arguments = input.Values(FileOptions.TagsAndFiles);
        var tags = arguments.Where(a => a.IsTag).WhereSelect(tag => Option.FromNullable(tagParser.Parse(tag))).ToList();
        var files = arguments.Where(a => a.IsFile);

        foreach (var file in filePathExtractor.FromFilePatterns(files, input.Has(FileOptions.Recursive)).Select(FileFromPath))
        {
            tags.ForEach(file.Untag);
        }

        return Task.FromResult(true);
    }

    private File FileFromPath(string filePath)
    {
        var file = newFile();

        file.LoadByPath(filePath);

        return file;
    }
}
