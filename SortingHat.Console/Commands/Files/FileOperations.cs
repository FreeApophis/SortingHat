using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.FileSystem;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.DI;
using static Funcky.Functional;
using Console = Apophis.CLI.Console;
using File = SortingHat.API.Models.File;

namespace SortingHat.CLI.Commands.Files;

/// <summary>
/// Puts the files of a search into a folder: choose a source per file (asking when several copies exist), let <see cref="ExportPlan"/> decide the targets,
/// then copy or move. One file that cannot be done is reported and skipped, the others still go.
/// </summary>
public class FileOperations<TFileOperation>(TFileOperation fileOperation, IExistsFile existsFile, IExistsDirectory existsDirectory, Console console, IFile fileStore)
    where TFileOperation : IFileOperation
{
    /// <summary>Windows ignores the case of file names, other systems do not.</summary>
    private static readonly StringComparer FileNameComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private string Verb => fileOperation is IMoveFile ? "move" : "copy";

    /// <summary>Puts the files found by <paramref name="search"/> into the folder <paramref name="destination"/>, relative to the current folder.</summary>
    /// <returns>Whether every file was copied or moved.</returns>
    public async Task<bool> ExportFiles(string search, string destination)
    {
        var destinationPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), destination));

        return existsDirectory.Exists(destinationPath)
            ? await ExportSearch(search, destinationPath)
            : Report($"The destination folder '{destinationPath}' does not exist, nothing was done.");
    }

    private async Task<bool> ExportSearch(string search, string destinationPath)
    {
        console.Writer.WriteLine($"Find Files: {search}");

        var result = fileStore.Search(search);
        result.UnknownTags.ForEach(unknownTag => console.Writer.WriteLine($"Unknown tag: {unknownTag}"));

        var filesByHash = result.Files.GroupByHash().ToList();

        return filesByHash.Count == 0
            ? Report("No files found for your search query...")
            : await Export([.. filesByHash.Select(ChooseSource)], destinationPath);
    }

    private async Task<bool> Export(IReadOnlyList<Either<string, File>> sources, string destinationPath)
    {
        var chosen = sources.WhereSelect(source => source.RightOrNone()).ToList();
        var plan = ExportPlan.Targets(chosen.Select(file => file.Path), destinationPath, NamesInDestination(chosen, destinationPath), FileNameComparer);

        var skipped = sources.WhereSelect(source => source.LeftOrNone()).Concat(plan.WhereSelect(step => step.LeftOrNone())).ToList();
        skipped.ForEach(problem => console.Writer.WriteLine(problem));

        var done = new List<bool>();
        foreach (var (file, step) in chosen.Zip(plan))
        {
            done.Add(await step.Match(left: _ => Task.FromResult(false), right: target => ExportFile(file, target)));
        }

        return skipped.Count == 0 && done.All(Identity);
    }

    private IEnumerable<string> NamesInDestination(IEnumerable<File> files, string destinationPath)
        => files
            .Select(file => Path.GetFileName(file.Path))
            .Where(name => existsFile.Exists(Path.Combine(destinationPath, name)));

    /// <summary>Only copies that still exist on disk can be a source; the user chooses when several do.</summary>
    private Either<string, File> ChooseSource(HashGroup fileGroup)
        => fileGroup.Files.Where(file => existsFile.Exists(file.Path)).ToList() switch
        {
            [] => Either<string, File>.Left($"'{string.Join("', '", fileGroup.Files.Select(file => file.Path))}' skipped: the file does not exist any more."),
            [var file] => Either<string, File>.Right(file),
            var files => AskForSource(fileGroup.Hash, files),
        };

    private Either<string, File> AskForSource(string hash, IReadOnlyList<File> files)
    {
        console.Writer.WriteLine();
        console.Writer.WriteLine($" The file with hash {hash.ShortHash().GetOrElse(hash)} has multiple sources, which one do you want to {Verb}:");

        var table = new ConsoleTable(2);
        files.Select((file, index) => (file, index)).ForEach(choice => table.Append($"[{choice.index}]", choice.file.Path));
        table.WriteTo(console.Writer);

        return console.Reader
            .ReadInt()
            .SelectMany(index => files.ElementAtOrNone(index))
            .Match(
                none: () => Either<string, File>.Left($"The file with hash {hash.ShortHash().GetOrElse(hash)} skipped: no valid source chosen."),
                some: Either<string, File>.Right);
    }

    /// <summary>A moved file gets its new path in the database, but only after the move on disk has worked.</summary>
    private async Task<bool> ExportFile(File source, ExportTarget target)
    {
        try
        {
            switch (fileOperation)
            {
                case ICopyFile copyFile:
                    console.Writer.WriteLine($"cp {target.Source} {target.Target}");
                    copyFile.Copy(target.Source, target.Target);
                    break;
                case IMoveFile moveFile:
                    console.Writer.WriteLine($"mv {target.Source} {target.Target}");
                    moveFile.Move(target.Source, target.Target);
                    await fileStore.MovePath(source, target.Target);
                    break;
                default:
                    throw new NotSupportedException($"Unknown file operation {fileOperation.GetType().Name}.");
            }

            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Report($"'{target.Source}' failed: {exception.Message}");
        }
    }

    private bool Report(string problem)
    {
        console.Writer.WriteLine(problem);

        return false;
    }
}
