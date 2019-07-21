using System.Collections.Generic;
using System.IO;
using SortingHat.ConsoleWriter;

namespace SortingHat.Plugin.ExtractRelevantText
{
    public class FolderScanner
    {
        private readonly IConsoleWriter _consoleWriter;

        FolderScanner(IConsoleWriter consoleWriter)
        {
            _consoleWriter = consoleWriter;
        }

        internal bool Scan(IEnumerable<string> folders)
        {
            _consoleWriter.WriteLine("Scanning...");
            foreach (var folder in folders)
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories))
                {
                    _consoleWriter.WriteLine(file);
                }
            }

            return true;
        }
    }
}