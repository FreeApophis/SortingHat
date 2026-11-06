using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI;
using Apophis.FileSystem;
using SortingHat.API.DI;
using Console = Apophis.CLI.Console;
using File = SortingHat.API.Models.File;

namespace SortingHat.CLI.Commands.Files;

public class FileOperations<TFileOperation>(TFileOperation fileOperation, IExistsFile existsFile, Console console, IFile fileStore)
    where TFileOperation : IFileOperation
{
    /// <summary>Puts the files found by <paramref name="search"/> into the folder <paramref name="destination"/>, relative to the current folder.</summary>
    public bool ExportFiles(string search, string destination)
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
            _ => ExportEachFile(filesByHash, destinationPath),
        };
    }

    private bool ExportEachFile(List<HashGroup> filesByHash, string destinationPath)
    {
        bool result = true;
        foreach (var file in filesByHash)
        {
            result = result && file.Files.Count() switch
            {
                0 => throw new NotImplementedException("How did that happen?"),
                1 => ExportFile(file.Files.First(), destinationPath),
                _ => ChoseFileToExport(file, destinationPath),
            };
        }

        if (!result)
        {
            console.Writer.WriteLine("Last file failed, process aborted...");
        }

        return result;
    }

    private bool ChoseFileToExport(HashGroup fileGroup, string destinationPath)
    {
        console.Writer.WriteLine();
        console.Writer.WriteLine($" The file with hash {fileGroup.Hash.ShortHash().GetOrElse(fileGroup.Hash)} has multiple sources, which one you want to copy:");

        var consoleTable = new ConsoleTable(2);

        foreach (var (file, index) in fileGroup.Files.Select((f, i) => (f, i)))
        {
            consoleTable.Append(FileSelectionIndex(file.Path, index), file.Path);
        }

        consoleTable.WriteTo(console.Writer);

        return console.Reader
            .ReadInt()
            .AndThen(selectedFileIndex => ExportFile(fileGroup.Files.Skip(selectedFileIndex).FirstOrDefault(), destinationPath))
            .GetOrElse(false);
    }

    private bool EmptyQuery()
    {
        console.Writer.WriteLine("No files found for your search query...");

        return false;
    }

    private bool ExportFile(File? sourceFile, string destinationPath)
    {
        if (sourceFile is { } && Path.GetFileName(sourceFile.Path) is { } fileName)
        {
            var destinationFilePath = Path.Combine(destinationPath, fileName);

            switch (fileOperation)
            {
                case ICopyFile copyFile:
                    console.Writer.WriteLine($"cp {sourceFile.Path} {destinationFilePath}");
                    copyFile.Copy(sourceFile.Path, destinationFilePath);
                    break;
                case IMoveFile moveFile:
                    console.Writer.WriteLine($"mv {sourceFile.Path} {destinationFilePath}");
                    moveFile.Move(sourceFile.Path, destinationFilePath);
                    break;
                default:
                    throw new NotImplementedException("Unknown file operation");
            }

            return true;
        }

        console.Writer.WriteLine("Source unusable.");
        return false;
    }

    private string FileSelectionIndex(string filePath, in int index)
    {
        return existsFile.Exists(filePath)
            ? $"[{index}]"
            : "[-]";
    }
}
