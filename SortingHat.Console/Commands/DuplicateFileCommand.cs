using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SortingHat.CLI.Output;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class DuplicateFileCommand : ICommand
    {
        private readonly IDatabase _db;

        public DuplicateFileCommand(IDatabase db)
        {
            _db = db;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            foreach (var file in _db.File.GetDuplicates())
            {
                Console.WriteLine("--- Duplicate ---");
                Console.WriteLine($"CreatedAt (oldest): {file.CreatedAt}");
                Console.WriteLine($"File Size: {file.Size}");
                Console.WriteLine($"File Tags: {string.Join(", ", file.GetTags().Result.Select(t => t.FullName))}");

                var table = FileTable();
                foreach (var path in file.GetPaths().Result)
                {
                    var fileInfo = new FileInfo(path);

                    if (fileInfo.Exists)
                    {
                        table.Append(fileInfo.CreationTimeUtc, fileInfo.Length.HumanSize(), fileInfo.Name, string.Empty, fileInfo.Directory.FullName);
                    }
                    else
                    {
                        table.Append(string.Empty, string.Empty, fileInfo.Name, "*", fileInfo.Directory.FullName);
                    }

                }
                Console.WriteLine(table.ToString());

            }

            return true;
        }

        private ConsoleTable FileTable()
        {
            var table = new ConsoleTable(5);

            return table;
        }

        public string LongCommand => "duplicate";
        public string ShortCommand => "d";
        public string ShortHelp => "Find duplicate files in your dms";
        public CommandGrouping CommandGrouping => CommandGrouping.File;
    }
}
