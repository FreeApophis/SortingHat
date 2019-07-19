using System;
using System.Collections.Generic;
using System.IO;

namespace SortingHat.Plugin.ExtractRelevant
{
    public class FolderScanner
    {
        internal bool Scan(IEnumerable<string> folders)
        {
            Console.WriteLine("Scanning...");
            foreach (var folder in folders)
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories))
                {
                    Console.WriteLine(file);
                }
            }

            return true;
        }
    }
}