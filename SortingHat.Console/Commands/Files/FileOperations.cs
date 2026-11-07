using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Apophis.CLI;
using Apophis.FileSystem;
using Funcky.Extensions;
using SortingHat.API.DI;
using Console = Apophis.CLI.Console;
using File = SortingHat.API.Models.File;

namespace SortingHat.CLI.Commands.Files;

public class FileOperations<TFileOperation>(TFileOperation fileOperation, IExistsFile existsFile, Console console, IFile fileStore)
    where TFileOperation : IFileOperation
{
    /// <summary>Puts the files found by <paramref name="search"/> into the folder <paramref name="destination"/>, relative to the current folder.</summary>
    public async Task<bool> ExportFiles(string search, string destination)
    {
        var destinationPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), destination));

        console.Writer.WriteLine($"Find Files: {search}");

        var result = fileStore.Search(search);

        foreach (var unknownTag in result.UnknownTags)
        {
            console.Writer.WriteLine($"Unknown tag: {unknownTag}");
        }

        var filesByHash = result.Files.GroupByHash().ToList();

        return filesByHash.Count switch
        {
            0 => EmptyQuery(),
            _ => await ExportEachFile(filesByHash, destinationPath),
        };
    }

    /// <summary>Stops at the first file that fails, the files before it are already exported.</summary>
    private async Task<bool> ExportEachFile(List<HashGroup> filesByHash, string destinationPath)
    {
        foreach (var fileGroup in filesByHash)
        {
            if (!await ExportFileGroup(fileGroup, destinationPath))
            {
                console.Writer.WriteLine("Last file failed, process aborted...");

                return false;
            }
        }

        return true;
    }

    private Task<bool> ExportFileGroup(HashGroup fileGroup, string destinationPath)
        => fileGroup.Files.ToList() switch
        {
            [var file] => ExportFile(file, destinationPath),
            var files => ChooseFileToExport(fileGroup.Hash, files, destinationPath),
        };

    private Task<bool> ChooseFileToExport(string hash, IReadOnlyList<File> files, string destinationPath)
    {
        console.Writer.WriteLine();
        console.Writer.WriteLine($" The file with hash {hash.ShortHash().GetOrElse(hash)} has multiple sources, which one you want to copy:");

        var consoleTable = new ConsoleTable(2);

        foreach (var (file, index) in files.Select((file, index) => (file, index)))
        {
            consoleTable.Append(FileSelectionIndex(file.Path, index), file.Path);
        }

        consoleTable.WriteTo(console.Writer);

        return console.Reader
            .ReadInt()
            .SelectMany(index => files.ElementAtOrNone(index))
            .Match(none: NoFileChosen, some: file => ExportFile(file, destinationPath));
    }

    private Task<bool> NoFileChosen()
    {
        console.Writer.WriteLine("No file chosen.");

        return Task.FromResult(false);
    }

    private bool EmptyQuery()
    {
        console.Writer.WriteLine("No files found for your search query...");

        return false;
    }

    /// <summary>A moved file gets its new path in the database, but only after the move on disk has worked.</summary>
    private async Task<bool> ExportFile(File sourceFile, string destinationPath)
    {
        var destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(sourceFile.Path));

        switch (fileOperation)
        {
            case ICopyFile copyFile:
                console.Writer.WriteLine($"cp {sourceFile.Path} {destinationFilePath}");
                copyFile.Copy(sourceFile.Path, destinationFilePath);
                break;
            case IMoveFile moveFile:
                console.Writer.WriteLine($"mv {sourceFile.Path} {destinationFilePath}");
                moveFile.Move(sourceFile.Path, destinationFilePath);
                await fileStore.MovePath(sourceFile, destinationFilePath);
                break;
            default:
                throw new NotImplementedException("Unknown file operation");
        }

        return true;
    }

    private string FileSelectionIndex(string filePath, int index)
        => existsFile.Exists(filePath)
            ? $"[{index}]"
            : "[-]";
}
