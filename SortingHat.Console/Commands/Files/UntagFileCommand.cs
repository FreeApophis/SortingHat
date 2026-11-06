using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.CLI.Options;

namespace SortingHat.CLI.Commands.Files;

[UsedImplicitly]
internal class UntagFileCommand(ILogger<TagFileCommand> logger, IFilePathExtractor filePathExtractor, ITagParser tagParser, Func<File> newFile) : ICommand
{
    public string LongCommand => "untag-files";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Remove tags from the indicated files";
    public CommandGrouping CommandGrouping => CommandGrouping.File;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        var tags = arguments.Where(a => a.IsTag);
        var files = arguments.Where(IsFile);

        foreach (var file in filePathExtractor.FromFilePatterns(files, options.HasOption<RecursiveOption>()).Select(FileFromPath))
        {
            foreach (var tag in tags.Select(tagParser.Parse))
            {
                if (tag != null)
                {
                    logger.LogInformation($"File {file.Path} tagged with {tag.Name}");
                    file.Untag(tag);
                }
            }
        }

        return true;
    }

    private static bool IsFile(string value)
    {
        return !value.StartsWith(":");
    }

    private File FileFromPath(string filePath)
    {
        var file = newFile();

        file.LoadByPath(filePath);

        return file;
    }
}
