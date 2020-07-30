using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using apophis.CLI;
using apophis.FileSystem;
using SortingHat.API.DI;
using Console = apophis.CLI.Console;
using File = SortingHat.API.Models.File;

namespace SortingHat.CLI.Commands.Files
{
    public class FileOperations<TFileOperation>
        where TFileOperation : IFileOperation
    {
        private readonly TFileOperation _fileOperation;
        private readonly IExistsFile _existsFile;
        private readonly Console _console;
        private readonly IFile _file;

        public FileOperations(TFileOperation fileOperation, IExistsFile existsFile, Console console, IFile file)
        {
            _fileOperation = fileOperation;
            _existsFile = existsFile;
            _console = console;
            _file = file;
        }

        public bool ExportFiles(IEnumerable<string> lazyArguments, IOptionParser options, string longCommand)
        {
            var arguments = lazyArguments.ToList();

            return arguments.Count switch
            {
                0 => NotEnoughArguments(longCommand),
                1 => NotEnoughArguments(longCommand),
                2 => ExportFiles(GetSearchArgument(arguments), GetDestinationPath(arguments)),
                _ => TooManyArguments(longCommand)
            };
        }

        private bool TooManyArguments(string longCommand)
        {
            _console.Writer.WriteLine("Too many arguments given, give complexes searches in quotes.");
            _console.Writer.WriteLine($"{_console.Application.Name} {longCommand} search destination");

            return false;
        }


        private bool NotEnoughArguments(string longCommand)
        {
            _console.Writer.WriteLine("Too few arguments given, please specify files with a search query and give a destination.");
            _console.Writer.WriteLine($"{_console.Application.Name} {longCommand} search destination");

            return false;
        }

        public bool ExportFiles(string search, string destinationPath)
        {
            _console.Writer.WriteLine($"Find Files: {search}");

            var filesByHash = _file.Search(search).GroupByHash().ToList();


            return filesByHash.Count switch
            {
                0 => EmptyQuery(),
                _ => ExportEachFile(filesByHash, destinationPath)
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
                    _ => ChoseFileToExport(file, destinationPath)
                };
            }

            if (result == false)
            {
                _console.Writer.WriteLine("Last file failed, process aborted...");
            }

            return result;
        }

        private bool ChoseFileToExport(HashGroup fileGroup, string destinationPath)
        {
            _console.Writer.WriteLine();
            _console.Writer.WriteLine($" The file with hash {fileGroup.Hash.ShortHash()} has multiple sources, which one you want to copy:");

            var consoleTable = new ConsoleTable(2);

            foreach (var (file, index) in fileGroup.Files.Select((f, i) => (f, i)))
            {
                consoleTable.Append(FileSelectionIndex(file.Path, index), file.Path);
            }

            consoleTable.WriteTo(_console.Writer);

            return _console.Reader
                .ReadInt()
                .AndThen(selectedFileIndex => ExportFile(fileGroup.Files.Skip(selectedFileIndex).FirstOrDefault(), destinationPath))
                .GetOrElse(false);
        }

        private bool EmptyQuery()
        {
            _console.Writer.WriteLine("No files found for your search query...");

            return false;
        }

        private static string GetDestinationPath(IEnumerable<string> arguments)
        {
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), arguments.Last()));
        }

        private static string GetSearchArgument(IEnumerable<string> arguments)
        {
            return arguments.First();
        }

        private bool ExportFile(File? sourceFile, string destinationPath)
        {
            if (sourceFile is {} && Path.GetFileName(sourceFile.Path) is { } fileName)
            {
                var destinationFilePath = Path.Combine(destinationPath, fileName);

                switch (_fileOperation)
                {
                    case ICopyFile copyFile:
                        _console.Writer.WriteLine($"cp {sourceFile.Path} {destinationFilePath}");
                        copyFile.Copy(sourceFile.Path, destinationFilePath);
                        break;
                    case IMoveFile moveFile:
                        _console.Writer.WriteLine($"mv {sourceFile.Path} {destinationFilePath}");
                        moveFile.Move(sourceFile.Path, destinationFilePath);
                        break;
                    default:
                        throw new NotImplementedException("Unknown file operation");
                }
                return true;
            }

            _console.Writer.WriteLine("Source unusable.");
            return false;
        }

        private string FileSelectionIndex(string filePath, in int index)
        {
            return _existsFile.Exists(filePath)
                ? $"[{index}]"
                : "[-]";
        }
    }
}
