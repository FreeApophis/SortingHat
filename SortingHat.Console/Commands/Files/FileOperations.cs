using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using apophis.CLI;
using apophis.FileSystem;
using SortingHat.API.DI;
using Console = apophis.CLI.Console;

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

        public bool ExportFiles(IEnumerable<string> arguments, IOptionParser options)
        {
            if (arguments.Count() != 2) throw new ArgumentOutOfRangeException(nameof(arguments));

            var search = arguments.First();
            var absolutePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), arguments.Last()));

            _console.Writer.WriteLine($"Find Files: {search}");

            var filesByHash = _file.Search(search).GroupByHash().ToList();

            if (filesByHash.Any())
            {
                foreach (var file in filesByHash)
                {
                    _console.Writer.WriteLine();
                    _console.Writer.WriteLine(" File");
                    var consoleTable = new ConsoleTable(2);
                    foreach (var (filePath, index) in file.Paths.Select((path, index) => (path, index)))
                    {
                        consoleTable.Append(FileSelectionIndex(filePath, index), filePath);
                        _console.Writer.WriteLine($"  {filePath}");
                    }

                    _console.Reader
                        .ReadInt()
                        .AndThen(selectedFileIndex => ExportFile(file, selectedFileIndex, absolutePath));
                }
            }
            else
            {
                _console.Writer.WriteLine("No files found for your search query...");
            }

            return true;
        }

        private void ExportFile(HashGroup file, int selectedFileIndex, string absolutePath)
        {
            var selectedPath = file.Paths.Skip(selectedFileIndex).First();

            if (Path.GetFileName(selectedPath) is { } selectedFileName)
            {
                _console.Writer.WriteLine($"cp {file.Paths.First()} {Path.Combine(absolutePath, selectedFileName)}");
                switch (_fileOperation)
                {
                    case ICopyFile copyFile:
                        copyFile.Copy(file.Paths.First(), Path.Combine(absolutePath, selectedFileName));
                        break;
                    case IMoveFile moveFile:
                        moveFile.Move(file.Paths.First(), Path.Combine(absolutePath, selectedFileName));
                        break;
                    default:
                        throw new NotImplementedException("Unknown file operation");
                }

            }
        }

        private string FileSelectionIndex(string filePath, in int index)
        {
            return _existsFile.Exists(filePath)
                ? $"[{index}]"
                : "[-]";
        }
    }
}
