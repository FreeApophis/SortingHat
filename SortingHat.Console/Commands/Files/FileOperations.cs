using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.DI;
using SortingHat.API.Models;
using static Funcky.Functional;

namespace SortingHat.CLI.Commands.Files;

/// <summary>
/// Puts the files of a search into a folder: choose a source per file (asking when several copies exist), let <see cref="ExportPlan"/> decide the targets,
/// then copy or move. One file that cannot be done is reported and skipped, the others still go.
/// </summary>
public class FileOperations<TFileOperation>(TFileOperation fileOperation, IExistsFile existsFile, IExistsDirectory existsDirectory, IConsoleWriter consoleWriter, IUserPrompt prompt, IFile fileStore)
    where TFileOperation : IFileOperation
{
    /// <summary>Windows ignores the case of file names, other systems do not.</summary>
    private static readonly StringComparer FileNameComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private string Verb => fileOperation is IMoveFile ? "move" : "copy";

    private string PastVerb => fileOperation is IMoveFile ? "moved" : "copied";

    /// <summary>Puts the files found by <paramref name="search"/> into the folder <paramref name="destination"/>, relative to the current folder.</summary>
    /// <param name="search">The search query.</param>
    /// <param name="destination">The destination folder.</param>
    /// <param name="dryRun">Only shows what would happen: no file and nothing in the database changes.</param>
    /// <returns>Whether every file was (or in a dry run would be) copied or moved.</returns>
    public bool ExportFiles(string search, string destination, bool dryRun)
    {
        var destinationPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), destination));

        var exported = existsDirectory.Exists(destinationPath)
            ? ExportSearch(search, destinationPath, dryRun)
            : Report($"The destination folder '{destinationPath}' does not exist, nothing was done.");

        if (dryRun)
        {
            consoleWriter.WriteLine($"Dry run: nothing was {PastVerb}.");
        }

        return exported;
    }

    private bool ExportSearch(string search, string destinationPath, bool dryRun)
    {
        consoleWriter.WriteLine($"Find Files: {search}");

        return fileStore.Search(search).Match(left: Report, right: result => ExportResult(result, destinationPath, dryRun));
    }

    private bool ExportResult(SearchResult result, string destinationPath, bool dryRun)
    {
        result.UnknownTags.ForEach(unknownTag => consoleWriter.WriteLine($"Unknown tag: {unknownTag}"));

        var filesByHash = result.Files.GroupByHash().ToList();

        return filesByHash.Count == 0
            ? Report("No files found for your search query...")
            : Export([.. filesByHash.Select(ChooseSource)], destinationPath, dryRun);
    }

    private bool Export(IReadOnlyList<Either<string, FileAt>> sources, string destinationPath, bool dryRun)
    {
        var chosen = sources.WhereSelect(source => source.RightOrNone()).ToList();
        var plan = ExportPlan.Targets(chosen.Select(file => file.Path), destinationPath, NamesInDestination(chosen, destinationPath), FileNameComparer);

        var skipped = sources.WhereSelect(source => source.LeftOrNone()).Concat(plan.WhereSelect(step => step.LeftOrNone())).ToList();
        skipped.ForEach(problem => consoleWriter.WriteLine(problem));

        // every file is tried, also after one failed
        var done = chosen.Zip(plan, (file, step) => step.Match(left: False, right: target => dryRun ? Preview(target) : ExportFile(file, target))).ToList();

        return skipped.Count == 0 && done.All(Identity);
    }

    private IEnumerable<string> NamesInDestination(IEnumerable<FileAt> files, string destinationPath)
        => files
            .Select(file => Path.GetFileName(file.Path))
            .Where(name => existsFile.Exists(Path.Combine(destinationPath, name)));

    /// <summary>Only copies that still exist on disk can be a source; the user chooses when several do.</summary>
    private Either<string, FileAt> ChooseSource(HashGroup fileGroup)
        => fileGroup.Files.Where(file => existsFile.Exists(file.Path)).ToList() switch
        {
            [] => Either<string, FileAt>.Left($"'{string.Join("', '", fileGroup.Files.Select(file => file.Path))}' skipped: the file does not exist any more."),
            [var file] => Either<string, FileAt>.Right(file),
            var files => AskForSource(fileGroup.Hash, files),
        };

    /// <summary>The user chooses with the arrow keys; where hat cannot ask, such as in a script, the file is skipped.</summary>
    private Either<string, FileAt> AskForSource(Hash hash, IReadOnlyList<FileAt> files)
        => prompt.Choose($"The file with hash {hash.Short} has several copies, which one do you want to {Verb}?", files, file => file.Path).Match(
            none: () => Either<string, FileAt>.Left($"The file with hash {hash.Short} skipped: it has several copies, and hat can only ask which one in an interactive terminal."),
            some: Either<string, FileAt>.Right);

    private bool Preview(ExportTarget target)
    {
        consoleWriter.WriteLine($"would {Verb}: {target.Source} -> {target.Target}");

        return true;
    }

    /// <summary>A moved file gets its new path in the database, but only after the move on disk has worked.</summary>
    private bool ExportFile(FileAt source, ExportTarget target)
    {
        try
        {
            switch (fileOperation)
            {
                case ICopyFile copyFile:
                    consoleWriter.WriteLine($"cp {target.Source} {target.Target}");
                    copyFile.Copy(target.Source, target.Target);
                    break;
                case IMoveFile moveFile:
                    consoleWriter.WriteLine($"mv {target.Source} {target.Target}");
                    moveFile.Move(target.Source, target.Target);
                    fileStore.MovePath(source, target.Target);
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
        consoleWriter.WriteLine(problem);

        return false;
    }
}
